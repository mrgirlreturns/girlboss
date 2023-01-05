using System;
using System.Text.Json.Serialization;

namespace Announcer;

public class Stream
{
    public Boolean Live { get; set; }

    public String? Game { get; set; }

    public String? Preview { get; set; }

    [JsonPropertyName("status_text")]
    public String? Status { get; set; }

    [JsonPropertyName("started_at")]
    public DateTimeOffset? Started { get; set; }

    [JsonPropertyName("ended_at")]
    public DateTimeOffset? Ended { get; set; }

    public Int32? Duration { get; set; }

    public Int32? Viewers { get; set; }

    public String? Id { get; set; }

    public String? Platform { get; set; }

    public String? Type { get; set; }
}
