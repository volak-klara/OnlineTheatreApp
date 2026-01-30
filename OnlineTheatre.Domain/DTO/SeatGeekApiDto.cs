using System.Text.Json.Serialization;

public class SeatGeekEventsResponse
{
    [JsonPropertyName("events")]
    public List<SeatGeekEvent> Events { get; set; } = new();
}

public class SeatGeekEvent
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    // обично го има, ама може да е null
    [JsonPropertyName("datetime_local")]
    public string? DateTimeLocal { get; set; }

    [JsonPropertyName("venue")]
    public SeatGeekVenue? Venue { get; set; }

    [JsonPropertyName("performers")]
    public List<SeatGeekPerformer>? Performers { get; set; }
}

public class SeatGeekVenue
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }
}

public class SeatGeekPerformer
{
    [JsonPropertyName("image")]
    public string? Image { get; set; }
}
