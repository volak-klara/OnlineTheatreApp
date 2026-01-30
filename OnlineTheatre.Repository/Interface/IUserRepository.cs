using OnlineTheatre.Domain.IdenitiyModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTheatre.Repository.Interface
{
    public interface IUserRepository
    {
        ApplicationUser GetUserById(string id);
    }
}
