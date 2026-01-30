using OnlineTheatre.Domain.IdenitiyModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTheatre.Domain.DomainModels
{
    public class ShoppingCart : BaseEntity
    {
        public string OwnerId { get; set; } = string.Empty;   
        public ApplicationUser? Owner { get; set; }
        public virtual ICollection<TicketInShoppingCart>? TicketInShoppingCarts { get; set; }
    }
}
