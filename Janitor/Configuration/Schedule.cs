using System;
using System.Text.Json.Serialization;

namespace Janitor.Configuration;

public class Schedule
{
    public required DateTime Date { get; init; }

    public required DateTime Time { get; init; }

    public Int32 InvocationDelay => (Int32) this.Date.Date.Add(this.Time.TimeOfDay) // TODO
                                                     .AddDays(1)
                                                     .Subtract(DateTime.Now)
                                                     .TotalMilliseconds;
}
