using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace Announcer;

using Configuration;

public class Service
{
    public Service(String index, Publications publications)
    {
        this.index = index;

        this.publications = publications;
    }

    public required DiscordSocketClient DiscordClient { get; init; }

    public required HttpClient HttpClient { get; init; }

    protected readonly Publications publications;

    protected String index; // TODO: Persist

    public async Task InvokeAsync()
    {
        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };

        jsonSerializerOptions.Converters.Add(
            item: new DateTimeOffsetConverter()
        );

        var timer = new PeriodicTimer(
            period: TimeSpan.FromMinutes(1)
        );

        while (await timer.WaitForNextTickAsync())
        {
            var info = await this.HttpClient.GetFromJsonAsync<Info>(
                requestUri: "https://mrgirl.tv/api/info/stream",
                options: jsonSerializerOptions,
                cancellationToken: CancellationToken.None
            );

            var facebook = info.Data.Streams["facebook"];
            var rumble   = info.Data.Streams["rumble"];

            if (rumble.Live)
            if (rumble.Id != this.index)
            {
                this.index = rumble.Id;

                await AnnounceAsync(facebook, rumble);
            }
        }
    }

    protected async Task AnnounceAsync(Stream facebook, Stream rumble)
    {
        var summary = new EmbedBuilder()
        {
            ImageUrl = facebook.Preview,
            Title = facebook.Status,
            Timestamp = rumble.Started,
            Url = "https://mrgirl.tv/bigscreen"
        };

        var tasks = this.publications
            .Discord
            .Servers
            .Select(async server => {
                await server.SendMessageAsync(
                    client: this.DiscordClient,
                    embed: summary.Build()
                );
            });

        await Task.WhenAll(tasks);
    }
}