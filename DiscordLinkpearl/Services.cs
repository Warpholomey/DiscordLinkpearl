using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using DiscordModule;

namespace DiscordLinkpearl;

public sealed class Services(
	IDalamudPluginInterface pluginInterface,
	ICommandManager commandManager,
	IClientState clientState,
	IGameGui gameGui,
	IChatGui chatGui)
{
	private Logger? _logger;
	private Configuration? _configuration;
	private DiscordModuleManager? _discordModuleManager;
	private PluginConfigurationWindow? _pluginConfigurationWindow;

	public IDalamudPluginInterface PluginInterface { get; } = pluginInterface;
	public ICommandManager CommandManager { get; } = commandManager;
	public IClientState ClientState { get; } = clientState;
	public IGameGui GameGui { get; } = gameGui;
	public IChatGui ChatGui { get; } = chatGui;

	public Configuration Configuration
	{
		get
		{
			if (_configuration == null)
			{
				_configuration = (Configuration?) PluginInterface.GetPluginConfig() ?? new Configuration();

				_configuration.OnConfigurationChanged += _ =>
				{
					PluginInterface.SavePluginConfig(_configuration);
				};
			}

			return _configuration;
		}
	}

	public DiscordModuleManager DiscordModuleManager
	{
		get
		{
			_discordModuleManager ??= new DiscordModuleManager(Configuration, Logger);
			return _discordModuleManager;
		}
	}

	public PluginConfigurationWindow PluginConfigurationWindow
	{
		get
		{
			_pluginConfigurationWindow ??= new PluginConfigurationWindow(
				DiscordModuleManager,
				Configuration,
				PluginInterface.AssemblyLocation.DirectoryName!);
			return _pluginConfigurationWindow;
		}
	}

	public ILogger Logger
	{
		get
		{
			_logger ??= new Logger(ChatGui);
			return _logger;
		}
	}
}
