using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord.WebSocket;

namespace Janitor;

using Configuration;

public class Service
{
    public Service(Subscriptions subscriptions)
    {
        this.subscriptions = subscriptions;
    }

    public required DiscordSocketClient DiscordClient { get; init; }

    protected readonly Subscriptions subscriptions;

    public async Task InvokeAsync()
    {
        var tasks = new List<Task<Channel>>();

        foreach (var server in this.subscriptions.Discord.Servers)
        foreach (var channel in server.Channels)
        {
            tasks.Add(
                item: channel.InvokeAsync(this.DiscordClient)
            );
        }

        while (true)
        {
            var task = await Task.WhenAny(tasks);

            var channel = task.Result;

            tasks.Add(
                item: channel.InvokeAsync(this.DiscordClient)
            );

            tasks.Remove(task);
        }
    }
}