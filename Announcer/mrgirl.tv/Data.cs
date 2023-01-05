using System;
using System.Collections.Generic;

namespace Announcer;

public class Data
{
    public String? HostedChannel { get; set; }

    public Dictionary<String, Stream>? Streams { get; set; }
}
