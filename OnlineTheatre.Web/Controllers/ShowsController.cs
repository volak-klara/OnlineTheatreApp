using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineTheatre.Domain.DomainModels;
using OnlineTheatre.Service.Interface;
using OnlineTheatre.Repository;
using OnlineTheatre.Service.Integration;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace OnlineTheatre.Web.Controllers
{
    public class ShowsController : Controller
    {
        private readonly IShowService _showService;
        private readonly ITicketmasterService _ticketmaster;
        private readonly IShoppingCartService _shoppingCartService;

        public ShowsController(IShowService showService, ITicketmasterService ticketmaster, IShoppingCartService shoppingCartService)
        {
            _showService = showService;
            _ticketmaster = ticketmaster;
            _shoppingCartService = shoppingCartService;
        }

        public async Task<IActionResult> Index(int? year, int? month)
        {
            
            var now = DateTime.Now;
            int y = year ?? now.Year;
            int m = month ?? now.Month;

            var firstDay = new DateTime(y, m, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            
            var shows = _showService
                .GetAll()
                .Where(s => s.StartTime >= DateTime.UtcNow)      
                .Where(s => s.StartTime >= firstDay && s.StartTime <= lastDay)
                .OrderBy(s => s.StartTime)
                .ToList();

           
            ViewBag.Month = m;

            return View(shows);
        }

        // GET: Shows/Details/5
       
        public IActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var show = _showService.GetById(id.Value);
            if (show == null)
            {
                return NotFound();
            }

            var preselected = new List<Guid>();

            // ако е најавен корисник
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                var cartDto = _shoppingCartService.GetByUserIdWithIncludedTickets(userId);

                // ticket IDs во cart што припаѓаат на ова шоу
                preselected = cartDto.Tickets
                    .Where(x => x.Ticket != null && x.Ticket.ShowId == show.Id)
                    .Select(x => x.TicketId)
                    .Distinct()
                    .ToList();
            }

            ViewBag.PreselectedTicketIds = preselected;
            return View(show);
        }

        // GET: Shows/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Shows/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Titile,StartTime,BasePrice")] Show show)  
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var createdShow = _showService.Insert(show);  
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }

            // Show ModelState errors
            ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return View(show);
        }


        // GET: Shows/Edit/5
        public IActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var show = _showService.GetById(id.Value);
            if (show == null)
            {
                return NotFound();
            }
            return View(show);
        }

        // POST: Shows/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, [Bind("Titile,StartTime,BasePrice,Id")] Show show)
        {
            if (id != show.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _showService.Update(show);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShowExists(show.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(show);
        }

        // GET: Shows/Delete/5
        public IActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var show = _showService.GetById(id.Value);
            if (show == null)
            {
                return NotFound();
            }

            return View(show);
        }

        // POST: Shows/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _showService.DeleteById(id);
            return RedirectToAction(nameof(Index));
        }

        private bool ShowExists(Guid id)
        {
            return _showService.GetById(id) != null;
        }
    


        public async Task<IActionResult> ImportFromApi()
        {
            var apiShows = await _ticketmaster.GetTheatreShowsAsync(20);

            int imported = 0;

            foreach (var show in apiShows)
            {
                if (_showService.ExistsByExternalId(show.ExternalId!))
                    continue;

                _showService.Insert(show);
                imported++;
            }

            TempData["Success"] = $"{imported} shows imported successfully.";
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportFromSeatGeek()
        {
            var count = await _showService.ImportFromSeatGeekAsync(10);
            TempData["Success"] = $"Imported {count} shows from SeatGeek.";
            return RedirectToAction(nameof(Index));
        }
    }
}
