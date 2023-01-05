using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace Janitor.Configuration;

public class Channel
{
    public required UInt64 Id { get; init; }

    public required String Name { get; init; }

    public required DateTime Trigger { get; set; } // TODO: Persist

    public Int32 InvocationDelay => (Int32) this.Trigger
                                                .Subtract(DateTime.Now)
                                                .TotalMilliseconds;

    public async Task<Channel> InvokeAsync(DiscordSocketClient discordClient)
    {
        var delay = this.InvocationDelay;

        if (0 < delay) { await Task.Delay(delay); }

        var difference = DateTime.Now.Subtract(this.Trigger);

        this.Trigger = this.Trigger.AddDays(1 + difference.Days);

        await this.DeleteMessagesAsync(discordClient);

        return this;
    }

    public async Task DeleteMessagesAsync(DiscordSocketClient discordClient)
    {
        var textChannel = (ITextChannel) discordClient.GetChannel(this.Id);

        if (textChannel.Name != this.Name)
        {
            throw new NotSupportedException($"Discord `TextChannel.Name: {textChannel.Name}` does not match configured `Channel.Name: {this.Name}` for `Id: {this.Id}`."); // TODO
        }

        IEnumerable<IMessage> messages;

        while (true)
        {
            messages = await textChannel.GetMessagesAsync(
                    limit: 1100
                )
                .FlattenAsync();

            messages = messages.Skip(100);

            await textChannel.DeleteMessagesAsync(
                messages: messages
            );

            if (!messages.Any()) { break; }
        }
    }
}
