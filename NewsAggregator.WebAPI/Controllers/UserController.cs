using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.WebAPI.Models.Requests;
using NewsAggregator.WebAPI.Utils;
using Serilog;

namespace NewsAggregator.WebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IJwtUtil _jwtUtil;
        private readonly IMapper _mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="roleService"></param>
        /// <param name="mapper"></param>
        /// <param name="jwtUtil"></param>
        public UserController(IUserService userService,
            IRoleService roleService,
            IMapper mapper,
            IJwtUtil jwtUtil)
        {
            _userService = userService;
            _roleService = roleService;
            _mapper = mapper;
            _jwtUtil = jwtUtil;
        }

        /// <summary>
        /// Get all register users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Register user.
        /// </summary>
        /// <param name="request">Contains user email, password and password confirmation.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegisterUserRequestModel request)
        {
            try
            {
                var userRoleId = await _roleService.GetRoleIdByNameAsync("User");
                var userDto = _mapper.Map<UserDto>(request);
                var userWIthSameEmailExists = await _userService.IsUserExists(request.Email);

                if (userDto != null 
                    && userRoleId != null
                    && !userWIthSameEmailExists
                    && request.Password.Equals(request.PasswordConfirmation))
                {
                    userDto.RoleId = userRoleId.Value;
                    var result = await _userService.RegisterUser(userDto, request.Password);

                    if (result > 0)
                    {
                        var userInDbDto = await _userService.GetUserWithRoleByEmailAsync(userDto.Email);

                        var response = await _jwtUtil.GenerateTokenAsync(userInDbDto);
                        return Ok(response);
                    }
                }

                return BadRequest();
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return StatusCode(500);
            }
        }
    }
}
