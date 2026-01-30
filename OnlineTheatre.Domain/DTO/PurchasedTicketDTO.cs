using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTheatre.Domain.DTO
{
    public class PurchasedTicketDTO
    {
        public Guid OrderId { get; set; }
        public Guid TicketId { get; set; }

        public string ShowTitle { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime EndTime => StartTime.AddMinutes(DurationMinutes);

        public string SeatLabel { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public string QrPngBase64 { get; set; } = string.Empty;
    }
}
