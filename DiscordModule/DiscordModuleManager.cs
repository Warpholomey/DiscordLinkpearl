using Discord;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordModule;

public sealed class DiscordModuleManager(IManagedConfiguration managedConfiguration, ILogger logger)
{
	private DiscordService? _discordService;
	private CancellationTokenSource? _cancellationTokenSource;
	private readonly ILogger _logger = logger;
	private readonly IManagedConfiguration _managedConfiguration = managedConfiguration;

	public event Action<DiscordMessage> OnDiscordMessage = null!;

	public ConnectionState ConnectionState => _discordService?.ConnectionState ?? ConnectionState.Disconnected;
	public ulong DiscordId => _discordService?.DiscordId ?? default;
	public string? GuildName => _discordService?.GuildName;

	public static ulong RequiredPermissionsCode => (ulong) Convert.ChangeType(RequiredPermissions, typeof(ulong));

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

	public void TryRestartDiscordModule()
	{
		TryStopDiscordModule();

		if (!_managedConfiguration.IsEnabled || string.IsNullOrWhiteSpace(_managedConfiguration.DiscordKey))
		{
			return;
		}

		_cancellationTokenSource = new();
		_discordService = new DiscordService(
			_managedConfiguration,
			_logger);

		Task.Run(
			async() => await _discordService.DiscordConnectionHandlerAsync(
				TriggerOnDiscordMessage,
				_cancellationTokenSource.Token),
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

	private Task TriggerOnDiscordMessage(DiscordMessage discordMessage)
	{
		return Task.Run(() => OnDiscordMessage?.Invoke(discordMessage));
	}
}
