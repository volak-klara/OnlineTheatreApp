using Microsoft.Extensions.Configuration;
using OnlineTheatre.Domain.DomainModels;
using OnlineTheatre.Service.Integrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTheatre.Service.Integration
{
    public class TicketmasterService : ITicketmasterService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public TicketmasterService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<List<Show>> GetTheatreShowsAsync(int count)
        {
            var apiKey = _config["Ticketmaster:ApiKey"];
            var country = _config["Ticketmaster:CountryCode"] ?? "US";

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Missing Ticketmaster:ApiKey in appsettings.json");

            var url =
                "https://app.ticketmaster.com/discovery/v2/events.json" +
                $"?apikey={apiKey}&classificationName=theatre&size={count}&countryCode={country}";

            // TicketmasterResponse е од твојот TicketmasterDtos.cs
            var data = await _http.GetFromJsonAsync<TicketmasterResponse>(url);

            var events = data?.Embedded?.Events ?? new List<TmEvent>();

            // ✅ Hardcoded dates: од денес па натаму, секоја вечер во 20:00
            var start = DateTime.Today.AddHours(20);
            var dates = Enumerable.Range(0, events.Count).Select(i => start.AddDays(i)).ToList();

            var shows = new List<Show>();

            for (int i = 0; i < events.Count; i++)
            {
                var e = events[i];

                // земи “најголема” слика
                var imageUrl = e.Images?
                    .OrderByDescending(img => img.Width)
                    .FirstOrDefault()?.Url;

                var venue = e.Embedded?.Venues?.FirstOrDefault();

                shows.Add(new Show
                {
                    Id = Guid.NewGuid(),
                    // ако уште ти е Titile, смени во Titile = ...
                    Titile = e.Name ?? "Untitled show",
                    StartTime = dates[i],
                    BasePrice = 250,
                    TotalSeats = 50,
                    ImageUrl = imageUrl,
                    Venue = venue?.Name,
                    City = venue?.City?.Name,

                    ExternalSource = "Ticketmaster",
                    ExternalId = e.Id
                });
            }

            return shows;
        }
    }
}
