using System;

namespace Conservator.Configuration;

public class Channel
{
    public required UInt64 Id { get; init; }

    public required String Name { get; init; }
}
