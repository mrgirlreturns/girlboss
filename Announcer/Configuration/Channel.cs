using System;
using System.Collections.Generic;
using System.Linq;

using Discord;
using Discord.WebSocket;

namespace Announcer.Configuration;

public class Channel
{
    public required UInt64 Id { get; init; }

    public required String Name { get; init; }

    public required IList<Role> Roles { get; init; }

    public String Mentions(IReadOnlyCollection<SocketRole> roles)
    {
        var local = this.Roles.Select(r => (r.Id, r.Name));
        var remote = roles.Select(r => (r.Id, r.Name));

        var exceptions = local.Except(remote);

        if (exceptions.Any())
        {
            foreach (var exception in exceptions)
            {
                Console.WriteLine($"Discord `Role.Name: {exception.Name}` (`Role.Id: {exception.Id}`) does not exist for channel `Channel.Name: {this.Name}` (`Channel.Id: {this.Id}`)."); // TODO
            }
        }

        var admissible = local.Except(exceptions)
            .Select(r => MentionUtils.MentionRole(r.Id));

        return String.Join(' ', admissible);
    }
}
