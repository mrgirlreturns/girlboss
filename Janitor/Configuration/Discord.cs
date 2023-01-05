using System.Collections.Generic;

namespace Janitor.Configuration;

public class Discord
{
    public required IList<Server> Servers { get; init; }
}
