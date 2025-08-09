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
using System.Threading.Tasks;

namespace DiscordLinkpearl;

public sealed class Plugin : IDalamudPlugin
{
	private readonly Services _services;

	public Plugin(IDalamudPluginInterface dalamudPluginInterface)
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
		_services.PluginInterface.UiBuilder.OpenMainUi += OpenConfigurationWindow;
	}

	private void DiscordModuleManagerOnDiscordMessage(DiscordMessage discordMessage)
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

		var cancelMessageReason = TrySendChatMessage(discordMessage);

		if (cancelMessageReason != null)
		{
			Task.Run(async() => await discordMessage.TryCancelAsync(cancelMessageReason));
		}
	}

	private unsafe string? TrySendChatMessage(DiscordMessage discordMessage)
	{
		var chatLogAddonPointer = _services.GameGui.GetAddonByName("ChatLog");

		if (chatLogAddonPointer == IntPtr.Zero || !chatLogAddonPointer.IsVisible)
		{
			return "Unknown send error or sending messages is currently unavailable!";
		}

		var args = discordMessage.Topic.Split('@');

		if (args.Length != 2)
		{
			return "This text channel has incorrect topic!";
		}

		var message = Chat.SanitiseText(discordMessage.Message);

		try
		{
			Chat.SendMessage($"/tell {args[0]}@{args[1]} {message}");
		}
		catch (ArgumentException)
		{
			var max = 492 - (args[0].Length + args[1].Length);

			var cur = message.Length;

			return $"The message is too big ({cur}/{max})!";
		}
		catch
		{
			return "Unknown send error!";
		}

		return null;
	}

	private void OnChatGuiChatMessageReceived(XivChatType messageType, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
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
				userId = playerLink.PlayerName + "@" + playerLink.World.Value.Name.ExtractText();
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
		_services.PluginInterface.UiBuilder.OpenMainUi -= OpenConfigurationWindow;
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
