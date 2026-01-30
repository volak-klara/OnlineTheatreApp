using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineTheatre.Domain.DomainModels;
using OnlineTheatre.Domain.IdenitiyModels;

namespace OnlineTheatre.Repository
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public virtual DbSet<Show> Shows { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }
        public virtual DbSet<TicketInShoppingCart>  TicketInShoppingCarts{ get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<TicketInOrder> TicketInOrders { get; set; }
    }
}
