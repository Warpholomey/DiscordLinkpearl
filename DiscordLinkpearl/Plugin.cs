using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;

using DiscordModule;

using ECommons;
using ECommons.Automation;

using FFXIVClientStructs.FFXIV.Component.GUI;

using System;
using System.Linq;

namespace DiscordLinkpearl;

public sealed class Plugin : IDalamudPlugin
{
	private readonly Services _services;

	public Plugin(DalamudPluginInterface dalamudPluginInterface)
	{
		_services = dalamudPluginInterface.Create<Services>()
			?? throw new InvalidOperationException(
				"Error initializing services!");

		ECommonsMain.Init(_services.PluginInterface, this);

		_services.PluginInterface.UiBuilder.OpenConfigUi += OpenConfigurationWindow;
		_services.PluginInterface.UiBuilder.Draw += _services.PluginConfigurationWindow.Draw;

		_services.CommandManager.AddHandler("/discordlinkpearl", new CommandInfo((command, args) => OpenConfigurationWindow())
		{
			HelpMessage = "Show settings window.",
		});

		_services.CommandManager.AddHandler("/dlp", new CommandInfo((command, args) => ToggleMessagesRouting())
		{
			HelpMessage = "Toggle messages routing.",
		});

		_services.DiscordModuleManager.OnDiscordMessage += DiscordModuleManagerOnDiscordMessage;
		_services.Configuration.OnConfigurationChanged += TryRestartDiscordModule;

		TryRestartDiscordModule();

		_services.ChatGui.ChatMessage += OnChatGuiChatMessageReceived;
	}

	private unsafe void DiscordModuleManagerOnDiscordMessage(DiscordMessage discordMessage)
	{
		if (_services.ClientState.LocalPlayer == null)
		{
			return;
		}
		else if (discordMessage.Topic == "me")
		{
			_services.Logger.LogInfo(discordMessage.Message);

			return;
		}
		else if (!discordMessage.Topic.Contains('@'))
		{
			return;
		}

		AtkUnitBase* chatLogAddon;

		var chatLogAddonPointer = _services.GameGui.GetAddonByName("ChatLog");

		if (chatLogAddonPointer == IntPtr.Zero)
		{
			return;
		}
		else
		{
			chatLogAddon = (AtkUnitBase*) chatLogAddonPointer;
		}

		if (chatLogAddon->IsVisible)
		{
			var args = discordMessage.Topic.Split('@');

			if (args.Length != 2)
			{
				return;
			}

			var message = Chat.Instance.SanitiseText(discordMessage.Message);

			try
			{
				Chat.Instance.SendMessage($"/tell {args[0]}@{args[1]} {message}");
			}
			catch
			{
				return;
			}
		}
	}

	private void OnChatGuiChatMessageReceived(XivChatType messageType, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
	{
		if (isHandled || _services.ClientState.LocalPlayer == null)
		{
			return;
		}

		if (messageType == XivChatType.TellIncoming)
		{
			string userId;
			string userName;

			if (sender.Payloads.FirstOrDefault(p => p.Type == PayloadType.Player) is PlayerPayload playerLink)
			{
				userId = playerLink.PlayerName + "@" + playerLink.World.Name.RawString;
				userName = playerLink.PlayerName;
			}
			else
			{
				if (sender.TextValue.Contains(_services.ClientState.LocalPlayer.Name.TextValue))
				{
					userId = "me";
					userName = _services.ClientState.LocalPlayer.Name.TextValue;
				}
				else
				{
					return;
				}
			}

			_services.DiscordModuleManager.TrySendMessage(
				userId,
				userName,
				message.TextValue);
		}
	}

	private void TryRestartDiscordModule(bool shouldRestartDiscordModule = true)
	{
		if (shouldRestartDiscordModule)
		{
			_services.DiscordModuleManager.TryRestartDiscordModule();
		}
	}

	private void OpenConfigurationWindow()
	{
		_services.PluginConfigurationWindow.ReloadConfigurationValues();
		_services.PluginConfigurationWindow.Show();
	}

	private void ToggleMessagesRouting()
	{
		_services.Configuration.IsEnabled = !_services.Configuration.IsEnabled;

		if (_services.Configuration.IsEnabled)
		{
			_services.Logger.LogInfo("Going online...");
		}
		else
		{
			_services.Logger.LogInfo("Going offline...");
		}

		_services.PluginConfigurationWindow.ReloadConfigurationValues();
		_services.Configuration.Save(true);
	}

	public void Dispose()
	{
		_services.ChatGui.ChatMessage -= OnChatGuiChatMessageReceived;
		_services.DiscordModuleManager.OnDiscordMessage -= DiscordModuleManagerOnDiscordMessage;
		_services.DiscordModuleManager.TryStopDiscordModule();
		_services.Configuration.ClearHandlers();
		_services.CommandManager.RemoveHandler("/dlp");
		_services.CommandManager.RemoveHandler("/discordlinkpearl");
		_services.PluginInterface.UiBuilder.Draw -= _services.PluginConfigurationWindow.Draw;
		_services.PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigurationWindow;
		ECommonsMain.Dispose();
	}
}
