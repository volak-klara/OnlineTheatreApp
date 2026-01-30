using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTheatre.Service.Interface;
using System;
using System.Security.Claims;

namespace OnlineTheatre.Web.Controllers
{
    //[Authorize] // user must be logged in, like in the slides
    public class ShoppingCartsController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;

        public ShoppingCartsController(IShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        // GET: /ShoppingCart
        // "Index" action that shows the cart – uses ShoppingCartDTO from the service
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cartDto = _shoppingCartService.GetByUserIdWithIncludedTickets(userId);

            return View(cartDto);   // Views/ShoppingCart/Index.cshtml
        }

        // POST: /ShoppingCart/DeleteTicket/{ticketId}
        // Thin action that just forwards the ticket id to the service
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteTicket(Guid ticketId)
        {
            _shoppingCartService.DeleteTicketFromShoppingCart(ticketId);

            return RedirectToAction(nameof(Index));
        }

        // POST: /ShoppingCart/Order
        // Equivalent to "Order" or "Place order" from the slides
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Order()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = _shoppingCartService.OrderTickets(userId);

            if (!result)
            {
                // Optional: show error message if ordering failed
                TempData["Error"] = "Ordering tickets failed.";
                return RedirectToAction(nameof(Index));
            }

            return View("Success"); // Views/ShoppingCart/Success.cshtml
        }

    }


}
