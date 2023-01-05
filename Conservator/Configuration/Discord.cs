using System.Collections.Generic;

namespace Conservator.Configuration;

public class Discord
{
    public required IList<Server> Servers { get; init; }
}
