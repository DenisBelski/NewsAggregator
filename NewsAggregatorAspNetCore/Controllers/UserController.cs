using Microsoft.AspNetCore.Mvc;
using NewsAggregatorAspNetCore.Models;

namespace NewsAggregatorAspNetCore.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserModel model)
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Reset()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Reset(UserModel model)
        {
            return View();
        }

        //[HttpGet]
        //public IActionResult Register()
        //{
        //    return Ok("Registred");
        //}

        //[HttpPost]
        //public IActionResult Login(string email, string password)
        //{
        //    return Ok("Logged in");
        //}
    }
}
