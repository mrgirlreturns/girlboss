using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Application;

public class Startup
{
    public Startup()
    {
        this.config = new ConfigurationBuilder()
            .AddUserSecrets<Startup>()
            .Build()
            .GetSection("Configuration")
            .Get<Configuration>()!;

        this.client = new DiscordSocketClient(
            config: new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.Guilds
                           | GatewayIntents.GuildMessages
                           | GatewayIntents.MessageContent,
                MessageCacheSize = 100
            }
        );

        this.client.Log += this.OnLogAsync;
        this.client.MessageReceived += this.OnMessageReceivedAsync;
        this.client.MessageUpdated  += this.OnMessageUpdatedAsync;
        this.client.MessageDeleted  += this.OnMessageDeletedAsync;

        this.commandService = new CommandService(
            config: new CommandServiceConfig()
            {
                DefaultRunMode = RunMode.Async
            }
        );

        this.commandService.Log += this.OnLogAsync;

        // this.client.MessageReceived += this.HandleCommandsAsync;
    }

    protected readonly CommandService commandService;

    protected readonly Configuration config;

    protected readonly DiscordSocketClient client;

    public static Task Main() => new Startup().MainAsync();

    public async Task MainAsync()
    {
        await this.commandService.AddModulesAsync(Assembly.GetEntryAssembly(), null);

        await this.client.LoginAsync(TokenType.Bot, this.config.Token);
        await this.client.StartAsync();
        await this.client.SetStatusAsync(UserStatus.Online);

        await Task.Delay(-1);
    }

    protected async Task HandleCommandsAsync(SocketMessage message)
    {
        var command = message as SocketUserMessage;

        if (command is null) { return; }

        var context = new SocketCommandContext(this.client, command);

        var index = 0;
        if (command.HasCharPrefix('!', ref index))
        {
            var result = await this.commandService.ExecuteAsync(context, index, null);

            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }
    }

    protected async Task OnLogAsync(LogMessage message)
    {
        Console.WriteLine(message.Message);

        var exception = message.Exception;

        if (exception is null) { return; }
        
        Console.WriteLine(exception.Message);

        while (exception.InnerException is not null)
        {
            exception = exception.InnerException;

            Console.WriteLine(exception.Message);
        }
    }

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
                    Url = Startup.GetMessageUrl(origin.Guild.Id, origin.Id, message.Id)
                },
                Description = message.Content,
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"{message.Author.Id}  •  {Startup.GetAccount(message.Author)}",
                    IconUrl = message.Author.GetAvatarUrl()
                },
                Timestamp = DateTimeOffset.Now
            };

            await this.ArchiveMessageAsync(
                guildId: origin.Guild.Id,
                embed:   summary.Build()
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
                    Url = Startup.GetMessageUrl(origin.Guild.Id, origin.Id, after.Id)
                },
                Description = after.Content,
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"{after.Author.Id}  •  {Startup.GetAccount(after.Author)}",
                    IconUrl = after.Author.GetAvatarUrl()
                },
                Timestamp = DateTimeOffset.Now
            };

            await this.ArchiveMessageAsync(
                guildId: origin.Guild.Id,
                embed:   summary.Build()
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
                Url = Startup.GetMessageUrl(origin.Guild.Id, origin.Id, before.Id)
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
                Text = $"{message.Author.Id}  •  {Startup.GetAccount(message.Author)}",
                IconUrl = message.Author.GetAvatarUrl()
            };
        }

        await this.ArchiveMessageAsync(
            guildId: origin.Guild.Id,
            embed:   summary.Build()
        );
    }

    public async Task ArchiveMessageAsync(UInt64 guildId, Embed embed)
    {
        var channelId = this.config.GuildArchives[guildId];

        await this.client
            .Guilds
            .Single(g => g.Id == guildId)
            .TextChannels
            .First(c => c.Id == channelId)
            .SendMessageAsync(embed: embed);
    }

    protected static String GetAccount(IUser author) => $"{author.Username}#{author.DiscriminatorValue}";

    protected static String GetMessageUrl(UInt64 guildId, UInt64 channelId, UInt64 messageId) => $"https://discord.com/channels/{guildId}/{channelId}/{messageId}";
}