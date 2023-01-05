using System;
using System.Net.Http;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Application;

public class Startup
{
    public Startup()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile(
                path: "Configuration.json",
                optional: false,
                reloadOnChange: true
            );

        this.configuration = builder.Build();

        this.discordClient = Startup.ConfigureDiscordClient(configuration);
        this.httpClient = Startup.ConfigureHttpClient(configuration);

        this.announcer = Startup.ConfigureAnnouncer(configuration, discordClient, httpClient);
        this.conservator = Startup.ConfigureConservator(configuration, discordClient);
        this.janitor = Startup.ConfigureJanitor(configuration, discordClient);

        this.discordClient.Ready += this.OnReadyAsync;
    }

    public static Announcer.Service ConfigureAnnouncer(IConfigurationRoot configuration, DiscordSocketClient discordClient, HttpClient httpClient)
    {
        var index = configuration.GetSection("Announcer:Index")
            .Get<String>();

        var publications = configuration.GetSection("Announcer:Publications")
            .Get<Announcer.Configuration.Publications>();

        return new Announcer.Service(index, publications)
        { 
            DiscordClient = discordClient,
            HttpClient    = httpClient
        };
    }

    public static Conservator.Service ConfigureConservator(IConfigurationRoot configuration, DiscordSocketClient discordClient)
    {
        var publications = configuration.GetSection("Conservator:Publications")
            .Get<Conservator.Configuration.Publications>();

        var subscriptions = configuration.GetSection("Conservator:Subscriptions")
            .Get<Conservator.Configuration.Subscriptions>();

        return new Conservator.Service(publications, subscriptions)
        {
            DiscordClient = discordClient
        };
    }

    public static DiscordSocketClient ConfigureDiscordClient(IConfigurationRoot configuration)
    {
        var discordClient = new DiscordSocketClient(
            config: new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.Guilds
                           | GatewayIntents.GuildMessages
                           | GatewayIntents.MessageContent,
                MessageCacheSize = 100
            }
        );

        discordClient.Log += Startup.OnLogAsync;

        return discordClient;
    }

    public static HttpClient ConfigureHttpClient(IConfigurationRoot configuration)
    {
        return new HttpClient();
    }

    public static Janitor.Service ConfigureJanitor(IConfigurationRoot configuration, DiscordSocketClient discordClient)
    {
        var subscriptions = configuration.GetSection("Janitor:Subscriptions")
            .Get<Janitor.Configuration.Subscriptions>();

        return new Janitor.Service(subscriptions)
        {
            DiscordClient = discordClient
        };
    }

    protected readonly IConfigurationRoot configuration;

    protected readonly Announcer.Service announcer;
    
    protected readonly Conservator.Service conservator;
    
    protected readonly Janitor.Service janitor;

    protected readonly DiscordSocketClient discordClient;

    protected readonly HttpClient httpClient;

    public static Task Main() => new Startup().MainAsync();

    public async Task MainAsync()
    {
        var token = this.configuration
            .GetSection("Token")
            .Get<String>();

        await this.discordClient.LoginAsync(TokenType.Bot, token);
        await this.discordClient.StartAsync();
        await this.discordClient.SetStatusAsync(UserStatus.Online);

        await Task.Delay(-1);
    }

    protected static async Task OnLogAsync(LogMessage message)
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

    protected async Task OnReadyAsync()
    {
        Task.Run(this.announcer.InvokeAsync);

        Task.Run(this.janitor.InvokeAsync);
    }
}