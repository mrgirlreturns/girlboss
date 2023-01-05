using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace Announcer.Configuration;

public class Server
{
    public required UInt64 Id { get; init; }

    public required String Name { get; init; }

    public required IList<Channel> Channels { get; init; }

    public async Task SendMessageAsync(DiscordSocketClient client, Embed embed)
    {
        var guild = client.Guilds
            .Single(g => g.Id == this.Id);

        if (guild.Name != this.Name)
        {
            throw new NotSupportedException($"Discord `Guild.Name: {guild.Name}` does not match configured `Server.Name: {this.Name}` for `Id: {this.Id}`."); // TODO
        }

        var tasks = this.Channels.Select(async channel => {
            var textChannel = guild.TextChannels
                .Single(c => c.Id == channel.Id);

            if (textChannel.Name != channel.Name)
            {
                throw new NotSupportedException($"Discord `TextChannel.Name: {guild.Name}` does not match configured `Channel.Name: {this.Name}` for `Id: {this.Id}`."); // TODO
            }

            String text = null;
            if (channel.Roles.Any())
            {
                text = channel.Mentions(guild.Roles);
            }

            await textChannel.SendMessageAsync(
                text: text,
                embed: embed
            );
        });

        await Task.WhenAll(tasks);
    }
}
