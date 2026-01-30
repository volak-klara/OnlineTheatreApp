using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTheatre.Domain.DomainModels
{
    public class Show : BaseEntity
    {
        [Required]
        public string? Titile { get; set; }
        [Required]
        public DateTime StartTime { get; set; }

        public int TotalSeats { get; set; } = 50;
        public decimal BasePrice { get; set; } = 250;

        public virtual ICollection<Ticket>? Tickets { get; set; }

        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public string? Venue { get; set; }
        public string? City { get; set; }

        public int DurationMinutes { get; set; } = 120;
        public DateTime EndTime => StartTime.AddMinutes(DurationMinutes);
        public string? ExternalSource { get; set; }   // "Ticketmaster"
        public string? ExternalId { get; set; }       // id од API
    }
}
