using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;

using DiscordModule;

namespace DiscordLinkpearl;

public sealed class Logger(IChatGui chatGui) : ILogger
{
	private readonly IChatGui _chatGui = chatGui;

	public void LogError(string message)
	{
		_chatGui.Print(
			new XivChatEntry
			{
				Type = XivChatType.Debug,
				Message = new SeStringBuilder()
					.AddUiForeground(17)
					.AddText($"[Discord Linkpearl] Error: {message}")
					.AddUiForegroundOff()
					.Build(),
			});
	}

	public void LogInfo(string message)
	{
		_chatGui.Print(
			new XivChatEntry
			{
				Type = XivChatType.Debug,
				Message = new SeStringBuilder()
					.AddUiForeground(32)
					.AddText($"[Discord Linkpearl] Info: {message}")
					.AddUiForegroundOff()
					.Build(),
			});
	}
}
