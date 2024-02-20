using Discord;
using Discord.WebSocket;

using System.Threading.Tasks;

namespace DiscordModule;

public sealed record DiscordMessage(string Topic, string Message, SocketTextChannel SocketTextChannel)
{
	private readonly SocketTextChannel _socketTextChannel = SocketTextChannel;

	public async Task TryCancelAsync(string reason)
	{
		var embed = new EmbedBuilder()
			.WithDescription(reason)
			.WithColor(Color.Red)
			.Build();

		await _socketTextChannel.SendMessageAsync(embed: embed);
	}
}
