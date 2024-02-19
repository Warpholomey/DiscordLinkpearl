using DiscordModule;

using System.Runtime.Serialization;

namespace DiscordModuleDebugger;

[DataContract]
public sealed class Configuration : IManagedConfiguration
{
	[DataMember(Name = "discordKey")]
	public string DiscordKey { get; set; } = null!;

	[DataMember(Name = "guildId")]
	public ulong GuildId { get; set; }

	public void Save(bool restartDiscordModule = false)
	{
	}
}
