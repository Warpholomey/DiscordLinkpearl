using Discord;
using Discord.WebSocket;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordModule;

public sealed class DiscordModuleManager
{
	private const TokenType DiscordTokenType = TokenType.Bot;

	private CancellationTokenSource? _cancellationTokenSource;
	private readonly DiscordSocketClient _discordSocketClient = new();

	public static ulong RequiredPermissionsCode => (ulong) Convert.ChangeType(RequiredPermissions, typeof(ulong));
	public ConnectionState ConnectionState => _discordSocketClient.ConnectionState;
	public ulong DiscordId { get; private set; }

	public static GuildPermission RequiredPermissions =>
		GuildPermission.ViewChannel |
		GuildPermission.ManageChannels |
		GuildPermission.ManageWebhooks |
		GuildPermission.SendMessages |
		GuildPermission.EmbedLinks |
		GuildPermission.AttachFiles |
		GuildPermission.AddReactions |
		GuildPermission.UseExternalEmojis |
		GuildPermission.ManageMessages |
		GuildPermission.ReadMessageHistory |
		GuildPermission.UseApplicationCommands;

	public void TryStopDiscordModule()
	{
		_cancellationTokenSource?.Cancel();
	}

	public void TryRestartDiscordModule(string discordKey)
	{
		TryStopDiscordModule();
		_cancellationTokenSource = new();
		Task.Run(
			async() => await DiscordConnectionHandler(discordKey, _cancellationTokenSource.Token),
			_cancellationTokenSource.Token);
	}

	private async Task DiscordConnectionHandler(string discordKey, CancellationToken cancellationToken)
	{
		await _discordSocketClient.LoginAsync(DiscordTokenType, discordKey);

		await _discordSocketClient.StartAsync();
		_discordSocketClient.Connected += OnDiscordSocketClientConnected;
		cancellationToken.WaitHandle.WaitOne();
		await _discordSocketClient.StopAsync();
	}

	private Task OnDiscordSocketClientConnected()
	{
		DiscordId = _discordSocketClient.CurrentUser.Id;
		return Task.CompletedTask;
	}
}
