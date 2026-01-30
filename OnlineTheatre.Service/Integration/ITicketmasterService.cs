using OnlineTheatre.Domain.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTheatre.Service.Integration
{
    public interface ITicketmasterService
    {
        Task<List<Show>> GetTheatreShowsAsync(int count);
    }
}
