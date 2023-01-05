using System;
using System.Collections.Generic;

namespace Janitor.Configuration;

public class Server
{
    public required UInt64 Id { get; init; }

    public required String Name { get; init; }

    public required IList<Channel> Channels { get; init; }
}
