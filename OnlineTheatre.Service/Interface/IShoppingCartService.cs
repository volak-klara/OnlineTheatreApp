using OnlineTheatre.Domain.DomainModels;
using OnlineTheatre.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTheatre.Service.Interface
{
    public interface IShoppingCartService
    {
        ShoppingCart? GetByUserId(string userId);
        ShoppingCartDTO GetByUserIdWithIncludedTickets(string userId);

        void DeleteTicketFromShoppingCart(Guid ticketdId);
        Boolean OrderTickets(string userId);

        public ShoppingCart Insert(ShoppingCart shoppingCart);
    }
}
