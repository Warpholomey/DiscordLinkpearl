using DiscordModule;

using System;

namespace DiscordModuleDebugger;

public sealed class Logger : ILogger
{
	public static ILogger Instance { get; } = new Logger();

	private Logger()
	{
	}

	public void LogError(string message)
	{
		var previousColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(message);
		Console.ForegroundColor = previousColor;
	}

	public void LogInfo(string message)
	{
		var previousColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Gray;
		Console.WriteLine(message);
		Console.ForegroundColor = previousColor;
	}
}
