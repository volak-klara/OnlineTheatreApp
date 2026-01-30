using Microsoft.AspNetCore.Identity;
using OnlineTheatre.Domain.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTheatre.Domain.IdenitiyModels
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ShoppingCart? UserCart { get; set; }
    }
}
