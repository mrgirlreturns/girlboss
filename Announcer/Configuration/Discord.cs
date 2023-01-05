using System.Collections.Generic;

namespace Announcer.Configuration;

public class Discord
{
    public required IList<Server> Servers { get; init; }
}
