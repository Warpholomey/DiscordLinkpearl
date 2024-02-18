using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin;

using DiscordModule;

using System;

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
		if (isHandled)
		{
			return;
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

	public void Dispose()
	{
		_services.ChatGui.ChatMessage -= OnChatGuiChatMessageReceived;
		_services.DiscordModuleManager.TryStopDiscordModule();
		_configuration.ClearHandlers();
		_services.CommandManager.RemoveHandler("/discordlinkpearl");
		_services.PluginInterface.UiBuilder.Draw -= _pluginConfigurationWindow.Draw;
		_services.PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigurationWindow;
	}
}
