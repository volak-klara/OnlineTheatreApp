using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTheatre.Domain.DomainModels
{
    public class Ticket : BaseEntity
    {
        public Guid ShowId { get; set; }
        public Show? Show { get; set; }

        public decimal Price { get; set; }
        public string SeatLabel { get; set; } = string.Empty;
        public bool IsSold { get; set; } = false;

        public virtual ICollection<TicketInShoppingCart>? TicketInShoppingCarts { get; set; }
    }
}
