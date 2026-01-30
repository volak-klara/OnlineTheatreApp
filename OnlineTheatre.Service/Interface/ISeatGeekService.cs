using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTheatre.Service.Interface
{
    public interface ISeatGeekService
    {
        Task<SeatGeekEventsResponse?> GetTheatreEventsAsync(int page = 1, int perPage = 30);
    }
}
