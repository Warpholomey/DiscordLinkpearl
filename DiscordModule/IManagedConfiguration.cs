namespace DiscordModule;

public interface IManagedConfiguration
{
	string DiscordKey { get; set; }
	ulong GuildId { get; set; }
	bool IsEnabled { get; set; }
	void Save(bool restartDiscordModule = false);
}
