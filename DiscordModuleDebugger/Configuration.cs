using DiscordModule;

using System.Runtime.Serialization;

namespace DiscordModuleDebugger;

[DataContract]
public sealed class Configuration : IManagedConfiguration
{
	[DataMember(Name = "discordKey")]
	public string DiscordKey { get; set; } = string.Empty;

	[DataMember(Name = "guildId")]
	public ulong GuildId { get; set; }

	[DataMember(Name = "isEnabled")]
	public bool IsEnabled { get; set; } = true;

	public void Save(bool restartDiscordModule = false)
	{
	}
}
