using Microsoft.EntityFrameworkCore;
using OnlineTheatre.Domain.DomainModels;
using OnlineTheatre.Domain.DTO;
using OnlineTheatre.Repository.Interface;
using OnlineTheatre.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineTheatre.Service.Implementation
{
    public class TicketService : ITicketService
    {
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<TicketInShoppingCart> _ticketInShoppingCartRepository;
        private readonly IShoppingCartService _shoppingCartService;
        

        public TicketService(
            IRepository<Ticket> ticketRepository,
            IRepository<TicketInShoppingCart> ticketInShoppingCartRepository,
            IShoppingCartService shoppingCartService)
        {
            _ticketRepository = ticketRepository;
            _ticketInShoppingCartRepository = ticketInShoppingCartRepository;
            _shoppingCartService = shoppingCartService;
        }

        public List<Ticket> GetAll()
        {
            return _ticketRepository.GetAll(selector: x => x).ToList();
        }

        public Ticket? GetById(Guid id)
        {
            return _ticketRepository.Get(selector: x => x, predicate: x => x.Id == id);
        }


        public void SyncSelectedSeats(AddToCartDTO model, string userId)
        {
            // 1) земи или креирај cart
            
            var cart = _shoppingCartService.GetByUserId(userId);
            

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    Id = Guid.NewGuid(),
                    OwnerId = userId
                };

              _shoppingCartService.Insert(cart);
            }

            var selectedIds = (model.SelectedTicketIds ?? new List<Guid>()).Distinct().ToHashSet();

            // 2) земи сите items од cart што се за ова шоу (include Ticket за ShowId)
            var currentItemsForShow = _ticketInShoppingCartRepository.GetAll(
                selector: x => x,
                predicate: x => x.ShoppingCartId == cart.Id,
                include: q => q.Include(x => x.Ticket)
            )
            .Where(x => x.Ticket != null && x.Ticket.ShowId == model.ShowId)
            .ToList();

            var currentTicketIds = currentItemsForShow.Select(x => x.TicketId).ToHashSet();

            // 3) DELETE: тие што се во cart а НЕ се селектирани
            var toRemove = currentItemsForShow.Where(x => !selectedIds.Contains(x.TicketId)).ToList();
            foreach (var item in toRemove)
                _ticketInShoppingCartRepository.Delete(item);

            // 4) ADD: тие што се селектирани а НЕ се во cart
            var toAdd = selectedIds.Where(id => !currentTicketIds.Contains(id)).ToList();
            foreach (var ticketId in toAdd)
            {
                var ticket = _ticketRepository.Get(selector: t => t, predicate: t => t.Id == ticketId);

                if (ticket == null) continue;
                if (ticket.IsSold) continue;
                if (ticket.ShowId != model.ShowId) continue; // safety

                _ticketInShoppingCartRepository.Insert(new TicketInShoppingCart
                {
                    Id = Guid.NewGuid(),
                    ShoppingCartId = cart.Id,
                    TicketId = ticketId,
                    Quantity = 1
                });
            }
        }


        public void AddTicketsToShoppingCart(AddToCartDTO model, string userId)
        {
            if (model.SelectedTicketIds == null || model.SelectedTicketIds.Count == 0)
                throw new Exception("No seats selected.");

            var shoppingCart = _shoppingCartService.GetByUserId(userId);
            if (shoppingCart == null)
                throw new Exception("No shopping cart found.");

            foreach (var ticketId in model.SelectedTicketIds.Distinct())
            {
                var ticket = GetById(ticketId);
                if (ticket == null)
                    throw new Exception("Ticket not found.");

                // ако е sold, не смее да се додаде
                if (ticket.IsSold)
                    throw new Exception($"Seat {ticket.SeatLabel} is already sold.");

                // seat-based => quantity = 1
                AddOrIgnoreCartItem(ticket, shoppingCart);
            }
        }



        private void AddOrIgnoreCartItem(Ticket ticket, ShoppingCart shoppingCart)
        {
            var existing = _ticketInShoppingCartRepository.Get(
                selector: x => x,
                predicate: x => x.TicketId == ticket.Id && x.ShoppingCartId == shoppingCart.Id
            );

            if (existing != null)
            {
                // За seat-based, НЕ ја зголемуваме количината.
                // Само игнорираме (веќе е во cart).
                return;
            }

            var newItem = new TicketInShoppingCart
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                Ticket = ticket,
                ShoppingCartId = shoppingCart.Id,
                ShoppingCart = shoppingCart,
                Quantity = 1
            };

            _ticketInShoppingCartRepository.Insert(newItem);
        }
    }
}
