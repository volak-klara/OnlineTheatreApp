//using Microsoft.EntityFrameworkCore;
//using OnlineTheatre.Domain.DomainModels;
//using OnlineTheatre.Domain.DTO;
//using OnlineTheatre.Repository.Interface;
//using OnlineTheatre.Service.Interface;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace OnlineTheatre.Service.Implementation
//{
//    public class ShoppingCartService : IShoppingCartService
//    {
//        private readonly IRepository<ShoppingCart> _shoppingCartRepository;

//        public ShoppingCartService(IRepository<ShoppingCart> shoppingCartRepository)
//        {
//            _shoppingCartRepository = shoppingCartRepository;
//        }

//        public void DeleteTicketFromShoppingCart(Guid ticketdId)
//        {
//            throw new NotImplementedException();
//        }

//        public ShoppingCart? GetByUserId(string userId)
//        {
//            return _shoppingCartRepository.Get(
//                selector: x => x, 
//                predicate: x => x.OwnerId.Equals(userId
//                ));
//        }

//        public ShoppingCartDTO GetByUserIdWithIncludedTickets(string userId)
//        {
//            var userCart = _shoppingCartRepository.Get(
//                selector: x => x,
//                predicate: x => x.OwnerId.Equals(userId),
//                include: x => x.Include(y => y.TicketInShoppingCarts).ThenInclude(z => z.Ticket)
//                );


//            var allTickets = userCart.TicketInShoppingCarts.ToList();
//            var allTicketsPrice = allTickets.Sum(x => x.Ticket.Price * x.Quantity);



//            var cartDto = new ShoppingCartDTO
//            {
//                Tickets = allTickets,
//                TotalPrice = (double)allTicketsPrice
//            };


//            return cartDto;
//        }

//        public bool OrderTickets(Guid Id)
//        {
//            return true;
//        }
//    }
//}


using Microsoft.EntityFrameworkCore;
using OnlineTheatre.Domain.DomainModels;
using OnlineTheatre.Domain.DTO;
using OnlineTheatre.Repository.Interface;
using OnlineTheatre.Service.Interface;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace OnlineTheatre.Service.Implementation
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IRepository<ShoppingCart> _shoppingCartRepository;
        private readonly IRepository<TicketInShoppingCart> _ticketInShoppingCartRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<TicketInOrder> _ticketInOrderRepository;

        public ShoppingCartService(
            IRepository<ShoppingCart> shoppingCartRepository,
            IRepository<TicketInShoppingCart> ticketInShoppingCartRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Order> orderRepository,
            IRepository<TicketInOrder> ticketInOrderRepository)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _ticketInShoppingCartRepository = ticketInShoppingCartRepository;
            _ticketRepository = ticketRepository;
            _orderRepository = orderRepository;
            _ticketInOrderRepository = ticketInOrderRepository;
        }

        public ShoppingCart? GetByUserId(string userId)
        {
            return _shoppingCartRepository.Get(
                selector: x => x,
                predicate: x => x.OwnerId == userId
            );
        }
        
        public ShoppingCart Insert(ShoppingCart shoppingCart)
        {
            return _shoppingCartRepository.Insert(shoppingCart);
        }

        public ShoppingCartDTO GetByUserIdWithIncludedTickets(string userId)
        {
            var userCart = _shoppingCartRepository.Get(
                selector: x => x,
                predicate: x => x.OwnerId == userId,
                include: x => x.Include(y => y.TicketInShoppingCarts)
                              .ThenInclude(z => z.Ticket)
                              .ThenInclude(t => t.Show)
            );

            var items = userCart?.TicketInShoppingCarts?.ToList() ?? new();
            var total = items.Sum(i => (i.Ticket?.Price ?? 0) * i.Quantity);

            return new ShoppingCartDTO
            {
                Tickets = items,
                TotalPrice = (double)total
            };
        }

        public void DeleteTicketFromShoppingCart(Guid ticketId)
        {
            // најди го cart-item записот што го врзува ticket со cart
            var item = _ticketInShoppingCartRepository.Get(
                selector: x => x,
                predicate: x => x.TicketId == ticketId
            );

            if (item == null) return;

            _ticketInShoppingCartRepository.Delete(item);
        }

        

       

    private static string GenerateToken(int bytes = 16)
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(bytes))
            .Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    public bool OrderTickets(string userId)
        {
            var cart = _shoppingCartRepository.Get(
                selector: x => x,
                predicate: x => x.OwnerId == userId.ToString(),
                include: q => q
                    .Include(c => c.TicketInShoppingCarts)
                        .ThenInclude(tisc => tisc.Ticket)
                            .ThenInclude(t => t.Show)
            );

            if (cart == null) return false;

            var cartItems = cart.TicketInShoppingCarts?.ToList() ?? new List<TicketInShoppingCart>();
            if (!cartItems.Any()) return false;

           
            foreach (var item in cartItems)
            {
                if (item.Ticket == null) return false;

                if (item.Ticket.IsSold) return false;

               
                if (item.Ticket.Show != null && item.Ticket.Show.StartTime <= DateTime.Now)
                    return false;
            }

          

            
            var total = cartItems.Sum(x => x.Ticket!.Price * x.Quantity);

            
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OwnerId = userId.ToString(),
                CreatedAt = DateTime.UtcNow,
                TotalPrice = total,
                TicketsInOrder = new List<TicketInOrder>()
            };

           

            foreach (var item in cartItems)
            {

                var token = GenerateToken();
                var payload = $"ORDER:{order.Id}|TICKET:{item.TicketId}|USER:{order.OwnerId}|TOKEN:{token}";
                order.TicketsInOrder.Add(new TicketInOrder
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    Order = order,
                    TicketId = item.TicketId,
                    OrderedTicket = item.Ticket,
                    Quantity = item.Quantity,
                    QrToken = token,
                    QrPayload = payload

                });

               
                item.Ticket!.IsSold = true;
            }

           

            _orderRepository.Insert(order); 
          
            foreach (var item in cartItems)
                _ticketInShoppingCartRepository.Delete(item);

          
            foreach (var item in cartItems)
                _ticketRepository.Update(item.Ticket!);

            return true;
        }

    }
}

