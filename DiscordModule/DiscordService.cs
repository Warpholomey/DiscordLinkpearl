using Discord;
using Discord.WebSocket;

using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordModule;

public sealed class DiscordService(IManagedConfiguration managedConfiguration)
{
	private const TokenType DiscordTokenType = TokenType.Bot;

	private readonly IManagedConfiguration _managedConfiguration = managedConfiguration;
	private readonly DiscordSocketClient _discordSocketClient = new();

	public ConnectionState ConnectionState => _discordSocketClient.ConnectionState;
	public ulong DiscordId { get; private set; }
	public string? GuildName { get; private set; }
	public ConcurrentQueue<QueueMessage> MessageQueue { get; } = new();

	public async Task DiscordConnectionHandlerAsync(CancellationToken cancellationToken)
	{
		await _discordSocketClient.LoginAsync(
			DiscordTokenType,
			_managedConfiguration.DiscordKey);

		var messageQueueProcessing = new Thread(() => RunMessageProcessingAsync(cancellationToken));

		_discordSocketClient.Connected += OnDiscordSocketClientConnected;
		_discordSocketClient.JoinedGuild += OnDiscordSocketClientJoinedGuild;
		_discordSocketClient.GuildAvailable += OnDiscordSocketClientGuildAvailable;

		await _discordSocketClient.StartAsync();
		messageQueueProcessing.Start();
		cancellationToken.WaitHandle.WaitOne();
		await _discordSocketClient.StopAsync();
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

		var channel = guild.Channels.OfType<ITextChannel>().FirstOrDefault(c => c.Topic == GetChannelTopicForUser())
			?? await guild.CreateTextChannelAsync(userName, ConfigureChannel);

		return channel;

		void ConfigureChannel(TextChannelProperties properties)
		{
			properties.Topic = GetChannelTopicForUser();
		}

		string GetChannelTopicForUser() => $"DO NOT EDIT! {userId}";
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
