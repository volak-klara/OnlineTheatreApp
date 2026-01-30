using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTheatre.Service.Interface;
using OnlineTheatre.Domain.DTO;
using System;
using System.Security.Claims;
using OnlineTheatre.Repository;

namespace OnlineTheatre.Web.Controllers
{

    
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        // List tickets for a given show (all, red ones = sold in view)
        public IActionResult ByShow(Guid showId)
        {
            var tickets = _ticketService
                .GetAll()
                .Where(t => t.ShowId == showId)
                .ToList();

            return View(tickets);   // Views/Ticket/ByShow.cshtml
        }

        // GET: /Ticket/AddToCart/{id}
        // Exactly like "GetSelectedShoppingCartProduct" example in slides,
        // it prepares the DTO for the AddToCart form.
        //public IActionResult AddToCart(Guid id)
        //{
        //    var ticket = _ticketService.GetById(id);
        //    if (ticket == null) return NotFound();

        //    var model = new AddToCartDTO
        //    {
        //        SelectedTicketId = ticket.Id,
        //        SelectedTicketSeatLabel = ticket.SeatLabel,
        //        Quantity = 1
        //    };

        //    return View(model); // Views/Ticket/AddToCart.cshtml
        //}

        //// POST: /Ticket/AddToCart
        //// This is the equivalent of "AddProductToCart" in ProductController
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult AddToCart(AddToCartDTO model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    // Get logged-in user id (like slides: user ID from principal)
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    // Controller only forwards ticketId + userId + quantity to service
        //    _ticketService.AddTicketToShoppingCart(model.SelectedTicketId, userId, model.Quantity);

        //    return RedirectToAction("Index", "ShoppingCart");
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize]
        public IActionResult AddSelectedToCart(AddToCartDTO model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please select at least one seat.";
                return RedirectToAction("Details", "Shows", new { id = model.ShowId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            _ticketService.SyncSelectedSeats(model, userId);

            return RedirectToAction("Index", "ShoppingCarts");
        }
    }
}
