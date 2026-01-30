using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OnlineTheatre.Domain;
using OnlineTheatre.Service.Interface;

namespace OnlineTheatre.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IShowService _showService;

        public HomeController(ILogger<HomeController> logger, IShowService showService)
        {
            _logger = logger;
            _showService = showService;
        }

        public IActionResult Index(int? year, int? month)
        {
            var now = DateTime.Now;
            int y = year ?? now.Year;
            int m = month ?? now.Month;

            var firstDay = new DateTime(y, m, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            // ???? ???? ?????????
            var shows = _showService
                .GetAll()
                .Where(s => s.StartTime >= DateTime.Now)
                .Where(s => s.StartTime >= firstDay && s.StartTime <= lastDay)
                .OrderBy(s => s.StartTime)
                .ToList();

            ViewBag.Year = y;
            ViewBag.Month = m;

            return View(shows);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
