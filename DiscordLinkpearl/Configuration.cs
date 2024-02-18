namespace DiscordLinkpearl;

public sealed partial class Configuration
{
	public int Version { get; set; }
	public string DiscordKey { get; set; } = string.Empty;
	public ulong GuildId { get; set; }
}
