using OnlineTheatre.Domain.IdenitiyModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTheatre.Domain.DomainModels
{
    public class Order : BaseEntity
    {
        public string OwnerId { get; set; } = string.Empty;
        public ApplicationUser? Owner { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalPrice { get; set; }
        public virtual ICollection<TicketInOrder>? TicketsInOrder { get; set; }
    }
}
