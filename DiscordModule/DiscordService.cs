using Discord.WebSocket;

using System.Threading.Tasks;
using System.Threading;

using Discord;

using System.Linq;

namespace DiscordModule;

public sealed class DiscordService(IManagedConfiguration managedConfiguration)
{
	private const TokenType DiscordTokenType = TokenType.Bot;

	private readonly IManagedConfiguration _managedConfiguration = managedConfiguration;
	private readonly DiscordSocketClient _discordSocketClient = new();

	public ConnectionState ConnectionState => _discordSocketClient.ConnectionState;
	public ulong DiscordId { get; private set; }
	public string? GuildName { get; private set; }

	public async Task DiscordConnectionHandlerAsync(CancellationToken cancellationToken)
	{
		await _discordSocketClient.LoginAsync(
			DiscordTokenType,
			_managedConfiguration.DiscordKey);

		_discordSocketClient.Connected += OnDiscordSocketClientConnected;
		_discordSocketClient.JoinedGuild += OnDiscordSocketClientJoinedGuild;
		_discordSocketClient.GuildAvailable += OnDiscordSocketClientGuildAvailable;

		await _discordSocketClient.StartAsync();
		cancellationToken.WaitHandle.WaitOne();
		await _discordSocketClient.StopAsync();
	}

	private Task OnDiscordSocketClientGuildAvailable(SocketGuild guild)
	{
		if (guild.Id == _managedConfiguration.GuildId)
		{
			GuildName = guild.Name;
		}
		
		return Task.CompletedTask;
	}

	private Task OnDiscordSocketClientJoinedGuild(SocketGuild guild)
	{
		_managedConfiguration.GuildId = guild.Id;
		_managedConfiguration.Save();
		return Task.CompletedTask;
	}

	private Task OnDiscordSocketClientConnected()
	{
		DiscordId = _discordSocketClient.CurrentUser.Id;
		return Task.CompletedTask;
	}
}
