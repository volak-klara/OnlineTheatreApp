using OnlineTheatre.Domain.DomainModels;
using OnlineTheatre.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTheatre.Service.Interface
{
    public interface ITicketService
    {
        //List<Ticket> GetAll();
        //Ticket? GetById(Guid id);
        ////Ticket Insert(Ticket ticket);
        ////Ticket Update(Ticket ticket);
        ////Ticket DeleteById(Guid id);
        //AddToCartDTO GetSelectedShoppingCartTicket(Guid id);
        //void AddTicketToShoppingCart(Guid id, string userId
        //
        void AddTicketsToShoppingCart(AddToCartDTO model, string userId);
        Ticket? GetById(Guid id);
        List<Ticket> GetAll();
        void SyncSelectedSeats(AddToCartDTO model, string userId);
    }
}
