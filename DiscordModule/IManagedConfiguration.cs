namespace DiscordModule;

public interface IManagedConfiguration
{
	string DiscordKey { get; set; }
	ulong GuildId { get; set; }
	void Save(bool restartDiscordModule = false);
}
