using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregatorAspNetCore.Models;
using System.Security.Claims;

namespace NewsAggregatorAspNetCore.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper;

        public AccountController(IUserService userService,
            IMapper mapper,
            IRoleService roleService)
        {
            _userService = userService;
            _mapper = mapper;
            _roleService = roleService;
        }


        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            return Ok("Logged in");
        }

        [HttpPost]
        public async Task<IActionResult> Login(RegisterModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //server validation (optional)
                    //check email for the existence of the same in the database:
                    //if (model.Email.ToLowerInvariant().Equals("test@email.com"))
                    //{
                    //    ModelState.AddModelError(nameof(model.Email), "Email is already exist");
                    //    return View(model);
                    //}
                    //client validation
                    //use attribute [Remote("CheckEmail", "Account", HttpMethod = WebRequestMethods.Http.Post, ErrorMessage = "Email is already exists")]
                    //and method CheckEmail (see below)
                    //for attribute Remote it is necessary to add scripts jquery-validation... (see views)

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
        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                //add "User" constant to the config
                var userRoleId = await _roleService.GetRoleIdByNameAsync("User");
                var userDto = _mapper.Map<UserDto>(model);

                if (userDto != null && userRoleId != null)
                {
                    userDto.RoleId = userRoleId.Value;
                    var result = await _userService.RegisterUser(userDto);

                    if (result > 0)
                    {
                        await Authenticate(model.Email);
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            return View(model);

            //try
            //{
            //    if (ModelState.IsValid)
            //    {
            //        model.Id = Guid.NewGuid();
            //        return Ok("New account successfully registered");
            //    }
            //    else
            //    {
            //        return View(model);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    return StatusCode(500);
            //}
        }

        [HttpGet]
        public async Task<IActionResult> Reset()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Reset(RegisterModel model)
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
            //check email for the existence of the same in the database:
            if (email.ToLowerInvariant().Equals("test@email.com"))
            {
                return Ok(false);
            }
            return Ok(true);
        }

        private async Task Authenticate(string email)
        {
            var userDto = await _userService.GetUserByEmailAsync(email);

            var claims = new List<Claim>()
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userDto.Email),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, userDto.RoleName)
            };

            //create instance which describe user
            //создаем экземпляр класса, which describes the user based on the claims that are created
            var identity = new ClaimsIdentity(claims,
                "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            //claimsPrincipal - defines an object, which will put to theHTTPContext and to the session
            //will assign an ID and give it to the client to write in a cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults
                .AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }

        //[HttpGet]
        //public IActionResult Register()
        //{
        //    return Ok("Registred");
        //}
    }
}
