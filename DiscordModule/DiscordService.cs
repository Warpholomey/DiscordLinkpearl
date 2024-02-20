using Discord;
using Discord.WebSocket;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordModule;

public sealed class DiscordService(IManagedConfiguration managedConfiguration, ILogger logger)
{
	private const TokenType DiscordTokenType = TokenType.Bot;

	private readonly DiscordSocketClient _discordSocketClient = new(_config);
	private readonly IManagedConfiguration _managedConfiguration = managedConfiguration;
	private readonly ILogger _logger = logger;

	private static readonly DiscordSocketConfig _config = new()
	{
		GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
	};

	public ConnectionState ConnectionState => _discordSocketClient.ConnectionState;
	public ulong DiscordId { get; private set; }
	public string? GuildName { get; private set; }
	public ConcurrentQueue<QueueMessage> MessageQueue { get; } = new();

	public async Task DiscordConnectionHandlerAsync(
		Func<DiscordMessage, Task> discordMessageHandler,
		CancellationToken cancellationToken)
	{
		await _discordSocketClient.LoginAsync(
			DiscordTokenType,
			_managedConfiguration.DiscordKey);

		var messageQueueProcessing = new Thread(() => RunMessageProcessingAsync(cancellationToken));

		_discordSocketClient.Connected += OnDiscordSocketClientConnected;
		_discordSocketClient.JoinedGuild += OnDiscordSocketClientJoinedGuild;
		_discordSocketClient.GuildAvailable += OnDiscordSocketClientGuildAvailable;
		_discordSocketClient.MessageReceived += (socketMessage) => DiscordSocketClientMessageReceived(
			socketMessage,
			discordMessageHandler);

		await _discordSocketClient.StartAsync();
		messageQueueProcessing.Start();
		cancellationToken.WaitHandle.WaitOne();
		await _discordSocketClient.StopAsync();
	}

	private async Task DiscordSocketClientMessageReceived(SocketMessage socketMessage, Func<DiscordMessage, Task> discordMessageHandler)
	{
		if (socketMessage.Source != MessageSource.User)
		{
			return;
		}

		if (socketMessage.Channel is SocketTextChannel socketTextChannel)
		{
			if (socketTextChannel.Guild.Id != _managedConfiguration.GuildId)
			{
				_logger.LogError("Have you added a bot to more than one server?");
				_logger.LogError("Messages can only be read from the last authorized server.");
				_logger.LogError("Consider creating a new bot or removing it from redundant servers.");

				return;
			}

			var discordMessage = new DiscordMessage(
				socketTextChannel.Topic,
				socketMessage.Content,
				socketTextChannel);

			await discordMessageHandler(discordMessage);
		}
	}

	private async Task SendMessageAsync(QueueMessage queueMessage)
	{
		var channel = await GetChannelAsync(queueMessage.UserId, queueMessage.UserName);

		await channel.SendMessageAsync(embed: GenerateEmbedForMessage(queueMessage.UserName, queueMessage.Message));
	}

	private static Embed GenerateEmbedForMessage(string userName, string message)
	{
		return new EmbedBuilder()
			.WithDescription(message)
			.WithColor(Color.Green)
			.WithAuthor(userName)
			.Build();
	}

	private async Task<ITextChannel> GetChannelAsync(string userId, string userName)
	{
		var guild = _discordSocketClient.GetGuild(_managedConfiguration.GuildId);

		var channel = guild.Channels.OfType<ITextChannel>().FirstOrDefault(c => userId.Equals(c.Topic))
			?? await guild.CreateTextChannelAsync(userName, ConfigureChannel);

		return channel;

		void ConfigureChannel(TextChannelProperties properties)
		{
			properties.Topic = userId;
		}
	}

	private async void RunMessageProcessingAsync(CancellationToken cancellationToken)
	{
		while (true)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}

			if (GuildName != null && ConnectionState == ConnectionState.Connected)
			{
				if (MessageQueue.TryDequeue(out var queueMessage))
				{
					await SendMessageAsync(queueMessage);
				}
			}

			Thread.Yield();
		}
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
