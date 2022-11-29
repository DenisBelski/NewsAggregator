using Microsoft.AspNetCore.Mvc;
using NewsAggregatorAspNetCore.Filters;
using NewsAggregatorAspNetCore.Models;
using System.Diagnostics;

namespace NewsAggregatorAspNetCore.Controllers
{
    [InternetExplorerBlockerFilter]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IActionResult Privacy()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
                throw;
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}