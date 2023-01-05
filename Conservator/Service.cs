using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace Conservator;

using Configuration;

public class Service
{
    public Service(Publications publications, Subscriptions subscriptions)
    {
        this.publications = publications;

        this.subscriptions = subscriptions;
    }

    public required DiscordSocketClient DiscordClient
    {
        get => this.discordClient;

        init
        {
            this.discordClient = value;

            this.discordClient.MessageReceived += this.OnMessageReceivedAsync;
            this.discordClient.MessageUpdated  += this.OnMessageUpdatedAsync;
            this.discordClient.MessageDeleted  += this.OnMessageDeletedAsync;
        }
    }

    protected readonly DiscordSocketClient discordClient;

    protected readonly Publications publications;

    protected readonly Subscriptions subscriptions;

    protected async Task OnMessageReceivedAsync(SocketMessage message)
    {
        if (!message.Author.IsBot)
        {
            var origin = message.Channel as SocketGuildChannel;

            var summary = new EmbedBuilder()
            {
                Color = Color.Green,
                Author = new EmbedAuthorBuilder()
                {
                    Name = $"{message.Id}  •  {origin.Name}",
                    Url = Service.GetMessageUrl(origin.Guild.Id, origin.Id, message.Id)
                },
                Description = message.Content,
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"{message.Author.Id}  •  {Service.GetAccount(message.Author)}",
                    IconUrl = message.Author.GetAvatarUrl()
                },
                Timestamp = DateTimeOffset.Now
            };

            await this.ArchiveMessageAsync(
                guildId: origin.Guild.Id,
                embed: summary.Build()
            );
        }
    }

    protected async Task OnMessageUpdatedAsync(Cacheable<IMessage, UInt64> before, SocketMessage after, ISocketMessageChannel channel)
    {
        if (!after.Author.IsBot)
        {
            var origin = channel as SocketGuildChannel;

            var summary = new EmbedBuilder()
            {
                Color = Color.Gold,
                Author = new EmbedAuthorBuilder()
                {
                    Name = $"{after.Id}  •  {origin.Name}",
                    Url = Service.GetMessageUrl(origin.Guild.Id, origin.Id, after.Id)
                },
                Description = after.Content,
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"{after.Author.Id}  •  {Service.GetAccount(after.Author)}",
                    IconUrl = after.Author.GetAvatarUrl()
                },
                Timestamp = DateTimeOffset.Now
            };

            await this.ArchiveMessageAsync(
                guildId: origin.Guild.Id,
                embed: summary.Build()
            );
        }
    }

    protected async Task OnMessageDeletedAsync(Cacheable<IMessage, UInt64> before, Cacheable<IMessageChannel, UInt64> channel)
    {
        var origin = channel.Value as SocketGuildChannel;

        var message = await before.GetOrDownloadAsync();

        var summary = new EmbedBuilder()
        {
            Color = Color.Red,
            Author = new EmbedAuthorBuilder()
            {
                Name = $"{before.Id}  •  {origin.Name}",
                Url = Service.GetMessageUrl(origin.Guild.Id, origin.Id, before.Id)
            },
            Footer = new EmbedFooterBuilder()
            {
                Text = "N/A"
            },
            Timestamp = DateTimeOffset.Now
        };

        if (message is not null)
        {
            summary.Description = message.Content;

            summary.Footer = new EmbedFooterBuilder()
            {
                Text = $"{message.Author.Id}  •  {Service.GetAccount(message.Author)}",
                IconUrl = message.Author.GetAvatarUrl()
            };
        }

        await this.ArchiveMessageAsync(
            guildId: origin.Guild.Id,
            embed: summary.Build()
        );
    }

    public async Task ArchiveMessageAsync(UInt64 guildId, Embed embed)
    {
        var server = this.publications
            .Discord
            .Servers
            .Single(s => s.Id == guildId);

        await server.SendMessageAsync(this.discordClient, embed);
    }

    protected static String GetAccount(IUser author) => $"{author.Username}#{author.DiscriminatorValue:0000}";

    protected static String GetMessageUrl(UInt64 guildId, UInt64 channelId, UInt64 messageId) => $"https://discord.com/channels/{guildId}/{channelId}/{messageId}";
}