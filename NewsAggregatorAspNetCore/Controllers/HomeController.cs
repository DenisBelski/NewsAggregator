using Microsoft.AspNetCore.Mvc;
using NewsAggregatorAspNetCore.Filters;
using NewsAggregatorAspNetCore.Models;
using Serilog;
using System.Diagnostics;

namespace NewsAggregatorAspNetCore.Controllers
{
    [InternetExplorerBlockerFilter]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return RedirectToAction("CustomError", "Home", new { statusCode = 500 });
            }
        }

        public IActionResult Privacy()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return RedirectToAction("CustomError", "Home", new { statusCode = 500 });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult CustomError(int statusCode)
        {
            if (statusCode == 400)
            {
                ViewBag.ErrorMessage = "The request was malformed.";
            }
            else if (statusCode == 404)
            {
                ViewBag.ErrorMessage = "The server cannot find the requested resource.";
            }
            else if (statusCode == 500)
            {
                ViewBag.ErrorMessage = "The server encountered an unexpected condition that prevented it from fulfilling the request.";
            }
            else
            {
                ViewBag.ErrorMessage = "Unexpected error. The development team is already solving your problem.";
            }

            ViewBag.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            ViewBag.ShowRequestId = !string.IsNullOrEmpty(ViewBag.RequestId);
            ViewBag.ErrorStatusCode = statusCode;

            return View();
        }
    }
}