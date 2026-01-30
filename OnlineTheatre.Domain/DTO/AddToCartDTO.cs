using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTheatre.Domain.DTO
{
    public class AddToCartDTO
    {
        //public Guid SelectedTicketId { get; set; }
        //public string? SelectedTicketSeatLabel { get; set; }
        //public int Quantity { get; set; }


        [Required]
        public Guid ShowId { get; set; }

        [Required]
        public List<Guid> SelectedTicketIds { get; set; } = new();
    }
}
