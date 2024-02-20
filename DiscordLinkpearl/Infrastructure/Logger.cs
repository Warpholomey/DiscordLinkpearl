using Dalamud.Plugin.Services;

using DiscordModule;

namespace DiscordLinkpearl;

public sealed class Logger(IChatGui chatGui) : ILogger
{
	private readonly IChatGui _chatGui = chatGui;

	public void LogError(string message)
	{
		_chatGui.PrintError($"[Discord Linkpearl] Error: {message}");
	}

	public void LogInfo(string message)
	{
		_chatGui.Print($"[Discord Linkpearl] Info: {message}");
	}
}
