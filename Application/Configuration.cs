using System;
using System.Collections.Generic;

namespace Application;

public class Configuration
{
    public required String Token { get; init; }

    public required Dictionary<UInt64, UInt64> GuildArchives { get; init; }
}
