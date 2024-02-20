using DiscordModule;

using System;
using System.IO;
using System.Text.Json;

namespace DiscordModuleDebugger;

public static class Program
{
	private const string ConfigurationFile = "./settings.json";

	public static void Main()
	{
		var managedConfiguration = File.Exists(ConfigurationFile)
			? LoadConfiguration()
			: InitializeConfiguration();
		var discordModule = new DiscordModuleManager(
			managedConfiguration,
			Logger.Instance);

		if (managedConfiguration.GuildId == default || string.IsNullOrWhiteSpace(managedConfiguration.DiscordKey))
		{
			return;
		}

		discordModule.OnDiscordMessage += DiscordModuleOnDiscordMessage;
		discordModule.TryRestartDiscordModule();
		discordModule.TrySendMessage("Thancred Waters@Twintania", "Thancred Waters", "Hey, this is Thancred!");
		Console.ReadKey();
		discordModule.TryStopDiscordModule();
	}

	private static void DiscordModuleOnDiscordMessage(DiscordMessage discordMessage)
	{
		Console.WriteLine($">> {discordMessage.Topic}: {discordMessage.Message}");
	}

	private static IManagedConfiguration LoadConfiguration()
	{
		var settings = File.ReadAllText(ConfigurationFile);
		return JsonSerializer.Deserialize<Configuration>(settings)
			?? throw new InvalidOperationException(
				"Error loading settings!");
	}

	private static IManagedConfiguration InitializeConfiguration()
	{
		Console.WriteLine("Discord Key:");
		var discordKey = Console.ReadLine()?.Trim();

		Console.WriteLine("Guild Id:");
		var guildId = Console.ReadLine()?.Trim();

		var configuration = new Configuration
		{
			DiscordKey = FormatDiscordKey(discordKey),
			GuildId = FormatGuildId(guildId),
		};

		File.WriteAllText(ConfigurationFile, JsonSerializer.Serialize(configuration));
		return configuration;
	}

	private static string FormatDiscordKey(string? @in) => @in ?? string.Empty;
	private static ulong FormatGuildId(string? @in) => @in == null ? default : ulong.Parse(@in);
}
