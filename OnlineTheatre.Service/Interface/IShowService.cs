using OnlineTheatre.Domain.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTheatre.Service.Interface
{
    public interface IShowService
    {
        List<Show> GetAll();
        Show? GetById(Guid id);
        Show Insert(Show show);
        Show Update(Show show);
        Show DeleteById(Guid id);
        bool ExistsByExternalId(string externalId);

        Task<int> ImportFromSeatGeekAsync(int count = 10);

    }
}
