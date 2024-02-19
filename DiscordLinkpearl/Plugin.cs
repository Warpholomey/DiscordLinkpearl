using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;

using DiscordModule;

using System;
using System.Linq;

namespace DiscordLinkpearl;

public sealed class Plugin : IDalamudPlugin
{
	private readonly Services _services;
	private readonly Configuration _configuration;
	private readonly PluginConfigurationWindow _pluginConfigurationWindow;

	public Plugin(DalamudPluginInterface dalamudPluginInterface)
	{
		_services = dalamudPluginInterface.Create<Services>(new DiscordModuleManager())
			?? throw new InvalidOperationException(
				"Error initializing services!");

		_configuration = (Configuration?) _services.PluginInterface.GetPluginConfig() ?? new Configuration();
		_pluginConfigurationWindow = new PluginConfigurationWindow(
			_services.DiscordModuleManager,
			_configuration);

		_services.PluginInterface.UiBuilder.OpenConfigUi += OpenConfigurationWindow;
		_services.PluginInterface.UiBuilder.Draw += _pluginConfigurationWindow.Draw;

		_services.CommandManager.AddHandler("/discordlinkpearl", new CommandInfo((command, args) => OpenConfigurationWindow())
		{
			HelpMessage = "Show settings window.",
		});

		_services.CommandManager.AddHandler("/dlp", new CommandInfo((command, args) => ToggleMessagesRouting())
		{
			HelpMessage = "Toggle messages routing.",
		});

		_configuration.OnConfigurationChanged += _ =>
		{
			_services.PluginInterface.SavePluginConfig(_configuration);
		};

		_configuration.OnConfigurationChanged += TryRestartDiscordModule;

		TryRestartDiscordModule();

		_services.ChatGui.ChatMessage += OnChatGuiChatMessageReceived;
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
			_services.DiscordModuleManager.TryRestartDiscordModule(_configuration);
		}
	}

	private void OpenConfigurationWindow()
	{
		_pluginConfigurationWindow.Show();
	}

	private void ToggleMessagesRouting()
	{
		_configuration.IsEnabled = !_configuration.IsEnabled;

		if (_configuration.IsEnabled)
		{
			_services.ChatGui.Print("Discord Linkpearl was enabled!");
		}
		else
		{
			_services.ChatGui.Print("Discord Linkpearl was disabled!");
		}

		_configuration.Save(true);
	}

	public void Dispose()
	{
		_services.ChatGui.ChatMessage -= OnChatGuiChatMessageReceived;
		_services.DiscordModuleManager.TryStopDiscordModule();
		_configuration.ClearHandlers();
		_services.CommandManager.RemoveHandler("/dlp");
		_services.CommandManager.RemoveHandler("/discordlinkpearl");
		_services.PluginInterface.UiBuilder.Draw -= _pluginConfigurationWindow.Draw;
		_services.PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigurationWindow;
	}
}
