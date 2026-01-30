using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTheatre.Service.Interface;
using System.Security.Claims;

[Authorize]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public IActionResult MyTickets()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var tickets = _orderService.GetActiveTickets(userId); // ќе го напишеме во сервис
        return View(tickets);
    }

    public IActionResult History()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tickets = _orderService.GetPastTickets(userId);
        return View(tickets);
    }
}
