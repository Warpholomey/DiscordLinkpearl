using Discord;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordModule;

public sealed class DiscordModuleManager
{
	private DiscordService? _discordService;
	private CancellationTokenSource? _cancellationTokenSource;

	public static ulong RequiredPermissionsCode => (ulong) Convert.ChangeType(RequiredPermissions, typeof(ulong));
	public ConnectionState ConnectionState => _discordService?.ConnectionState ?? ConnectionState.Disconnected;
	public ulong DiscordId => _discordService?.DiscordId ?? default;
	public string? GuildName => _discordService?.GuildName;

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

	public void TryRestartDiscordModule(IManagedConfiguration managedConfiguration)
	{
		TryStopDiscordModule();

		if (!managedConfiguration.IsEnabled || string.IsNullOrWhiteSpace(managedConfiguration.DiscordKey))
		{
			return;
		}

		_cancellationTokenSource = new();
		_discordService = new DiscordService(managedConfiguration);
		Task.Run(
			async() => await _discordService.DiscordConnectionHandlerAsync(_cancellationTokenSource.Token),
			_cancellationTokenSource.Token);
	}

	public void TrySendMessage(string userId, string userName, string message)
	{
		if (_discordService == null || _cancellationTokenSource == null)
		{
			return;
		}

		_discordService.MessageQueue.Enqueue(
			new QueueMessage(userId, userName, message));
	}
}
