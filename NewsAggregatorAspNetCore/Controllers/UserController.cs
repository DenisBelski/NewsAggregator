using Microsoft.AspNetCore.Mvc;
using NewsAggregatorAspNetCore.Models;

namespace NewsAggregatorAspNetCore.Controllers
{
    public class UserController : Controller
    {
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //Встраиваем логику проверки Email на существование такого-же в базе (серверная валидация, в идеале делать в паре с клиентской для перестраховки):
                    //if (model.Email.ToLowerInvariant().Equals("test@email.com"))
                    //{
                    //    ModelState.AddModelError(nameof(model.Email), "Email is already exist");
                    //    return View(model);
                    //}
                    //клиентская валидация
                    //используем атрибут[Remote("CheckEmail", "Account", HttpMethod = WebRequestMethods.Http.Post, ErrorMessage = "Email is already exists")]
                    //и соответствующий метод CheckEmail, см. ниже (валидация на стороне клиента)
                    //для корректной работы Remote необходимы подключенные скрипты jquery-validation...

                    return Ok("Login Successful");
                }
                else
                {
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.Id = Guid.NewGuid();
                    return Ok("New account successfully registered");
                }
                else
                {
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Reset()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Reset(UserModel model)
        {
            try
            {
                if (model.Email != null)
                {
                    return Ok("We have sent an email with instructions to your email.");
                }
                else
                {
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        public IActionResult CheckEmail(string email)
        {
            // в if вставить проверку на уже существующие emals в базе
            if (email.ToLowerInvariant().Equals("test@email.com"))
            {
                return Ok(false);
            }
            return Ok(true);
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
