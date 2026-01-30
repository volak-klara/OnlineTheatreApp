using Microsoft.Extensions.Configuration;
using OnlineTheatre.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnlineTheatre.Service.Implementation
{
    public class SeatGeekService : ISeatGeekService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public SeatGeekService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<SeatGeekEventsResponse?> GetTheatreEventsAsync(int page = 1, int perPage = 30)
        {
            var clientId = _config["SeatGeek:ClientId"];
            if (string.IsNullOrWhiteSpace(clientId))
                throw new Exception("SeatGeek ClientId missing in appsettings.json");

            // SeatGeek events endpoint: /2/events со query params (client_id, per_page, page, type=theater)
            // pagination е со per_page и page. :contentReference[oaicite:2]{index=2}
            var url = $"https://api.seatgeek.com/2/events?type=theater&per_page={perPage}&page={page}&client_id={clientId}";

            var resp = await _http.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<SeatGeekEventsResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
