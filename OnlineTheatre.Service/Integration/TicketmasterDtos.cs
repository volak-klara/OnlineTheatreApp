using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OnlineTheatre.Service.Integrations
{
    internal class TicketmasterResponse
    {
        [JsonPropertyName("_embedded")]
        public Embedded? Embedded { get; set; }
    }

    internal class Embedded
    {
        [JsonPropertyName("events")]
        public List<TmEvent>? Events { get; set; }
    }

    internal class TmEvent
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("images")]
        public List<TmImage>? Images { get; set; }

        [JsonPropertyName("_embedded")]
        public TmEmbedded? Embedded { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    internal class TmEmbedded
    {
        [JsonPropertyName("venues")]
        public List<TmVenue>? Venues { get; set; }
    }

    internal class TmVenue
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("city")]
        public TmCity? City { get; set; }
    }

    internal class TmCity
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    internal class TmImage
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }
    }
}
