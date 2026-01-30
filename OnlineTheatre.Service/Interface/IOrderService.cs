using OnlineTheatre.Domain.DomainModels;
using OnlineTheatre.Domain.DTO;
using System;
using System.Collections.Generic;

namespace OnlineTheatre.Service.Interface
{
    public interface IOrderService
    {
        List<Order> GetAllOrders();
        Order? GetOrder(Guid id);
        List<PurchasedTicketDTO> GetActiveTickets(string userId);
        List<PurchasedTicketDTO> GetPastTickets(string userId);
    }
}
