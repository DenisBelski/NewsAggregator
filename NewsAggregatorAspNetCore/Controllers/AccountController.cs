using System.Security.Claims;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregatorAspNetCore.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace NewsAggregatorAspNetCore.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper;

        public AccountController(IConfiguration configuration,
            IUserService userService,
            IMapper mapper,
            IRoleService roleService)
        {
            _configuration = configuration;
            _userService = userService;
            _roleService = roleService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Login()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                var isPasswordCorrect = await _userService.CheckUserPassword(model.Email, model.Password);

                if (isPasswordCorrect)
                {
                    await AuthenticateAsync(model.Email);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return BadRequest();
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userRoleId = await _roleService
                        .GetRoleIdByNameAsync(_configuration["UsersRole:DefaultUserRole"]);

                    var userDto = _mapper.Map<UserDto>(model);

                    if (userDto != null && userRoleId != null)
                    {
                        userDto.RoleId = userRoleId.Value;
                        var result = await _userService.RegisterUser(userDto, model.Password);

                        if (result > 0)
                        {
                            await AuthenticateAsync(model.Email);
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync();

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500);
            }
        }

        [HttpGet]
        public IActionResult Reset()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500);
            }
        }

        [HttpPost]
        public IActionResult Reset(LoginModel model)
        {
            try
            {
                if (model.Email != null)
                {
                    return Ok("Oops, this part of the application hasn't been created yet. Just create a new account.");
                }
                else
                {
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500);
            }
        }

        [HttpPost]
        public IActionResult ResetError()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckEmail(string email)
        {
            try
            {
                var isEmailExist = await _userService.IsUserExists(email);

                if (isEmailExist)
                {
                    return Ok(false);
                }

                return Ok(true);
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetUserData()
        {
            try
            {
                var userEmail = User.Identity?.Name;

                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest();
                }

                var user = _mapper.Map<UserDataModel>(_userService.GetUserWithRoleByEmailAsync(userEmail));

                return Ok(user);
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult IsLoggedIn()
        {
            try
            {
                if (User.Identities.Any(identity => identity.IsAuthenticated))
                {
                    return Ok(true);
                }

                return Ok(false);
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500);
            }
        }

        [HttpGet]
        public async Task<IActionResult> UserLoginPreviewAsync()
        {
            if (User.Identities.Any(identity => identity.IsAuthenticated))
            {
                var userEmail = User.Identity?.Name;

                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest();
                }

                var userDto = await _userService.GetUserWithRoleByEmailAsync(userEmail);
                return View(_mapper.Map<UserDataModel>(userDto));
            }

            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> PersonalCabinetForAdmin()
        {
            var userEmail = User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                return BadRequest();
            }

            var userDto = await _userService.GetUserWithRoleByEmailAsync(userEmail);
            return View(_mapper.Map<UserDataModel>(userDto));
        }


        private async Task AuthenticateAsync(string email)
        {
            try
            {
                var userDto = await _userService.GetUserWithRoleByEmailAsync(email);

                var claims = new List<Claim>()
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, userDto.Email),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, userDto.RoleName)
                };

                var identity = new ClaimsIdentity(claims,
                    "ApplicationCookie",
                    ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults
                    .AuthenticationScheme,
                    new ClaimsPrincipal(identity));
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
            }
        }
    }
}
