using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTheatre.Domain.DomainModels
{
    public class TicketInOrder : BaseEntity
    {
        public Guid TicketId { get; set; }
        public Ticket? OrderedTicket { get; set; }

        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public int Quantity { get; set; }

        public string QrToken { get; set; } = string.Empty;  
        public string QrPayload { get; set; } = string.Empty; 

    }
}
