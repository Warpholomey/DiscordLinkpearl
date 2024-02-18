using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using DiscordModule;

namespace DiscordLinkpearl;

public sealed class Services(
	DalamudPluginInterface pluginInterface,
	DiscordModuleManager discordModuleManager,
	ICommandManager commandManager,
	IChatGui chatGui)
{
	public DalamudPluginInterface PluginInterface { get; } = pluginInterface;
	public DiscordModuleManager DiscordModuleManager { get; } = discordModuleManager;
	public ICommandManager CommandManager { get; } = commandManager;
	public IChatGui ChatGui { get; } = chatGui;
}
