using Microsoft.EntityFrameworkCore;
using OnlineTheatre.Domain.IdenitiyModels;
using OnlineTheatre.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTheatre.Repository.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<ApplicationUser> entites;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
            this.entites = _context.Set<ApplicationUser>();
        }

        public ApplicationUser GetUserById(string id)
        {
            return entites.First(ent => ent.Id == id);
        }
    }
}

