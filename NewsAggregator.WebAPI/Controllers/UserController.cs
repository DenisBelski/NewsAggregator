using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.WebAPI.Models.Requests;
using NewsAggregator.WebAPI.Models.Responses;
using NewsAggregator.WebAPI.Utils;
using Serilog;

namespace NewsAggregator.WebAPI.Controllers
{
    /// <summary>
    /// Controller for work with users.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IJwtUtil _jwtUtil;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="configuration"></param>
        /// <param name="userService"></param>
        /// <param name="roleService"></param>
        /// <param name="jwtUtil"></param>
        public UserController(IMapper mapper,
            IConfiguration configuration,
            IUserService userService,
            IRoleService roleService,
            IJwtUtil jwtUtil)
        {
            _mapper = mapper;
            _configuration = configuration;
            _userService = userService;
            _roleService = roleService;
            _jwtUtil = jwtUtil;
        }

        /// <summary>
        /// Get all register users.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(UserResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var listUsers = await _userService.GetAllUsersAsync();

                if (listUsers.Any())
                {
                    foreach (var user in listUsers)
                    {
                        user.RoleName = await _roleService.GetRoleNameByIdAsync(user.RoleId);
                    }

                    return Ok(_mapper.Map<List<UserResponseModel>>(listUsers));
                }

                return NotFound(new ErrorModel { ErrorMessage = "No users found in the storage" });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, new ErrorModel
                {
                    ErrorMessage = "The server encountered an unexpected situation."
                });
            }
        }

        /// <summary>
        /// Register new user.
        /// </summary>
        /// <param name="userModel">Contains user email, password and password confirmation.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] RegisterUserRequestModel userModel)
        {
            try
            {
                if (!string.IsNullOrEmpty(userModel.Email)
                    && !string.IsNullOrEmpty(userModel.Password)
                    && !string.IsNullOrEmpty(userModel.PasswordConfirmation))
                {
                    var userRoleId = await _roleService.GetRoleIdByNameAsync(_configuration["UserRoles:Default"]);
                    var userDto = _mapper.Map<UserDto>(userModel);
                    var userWithSameEmailExists = await _userService.IsUserExists(userModel.Email);

                    if (userRoleId != null
                        && userDto != null
                        && !userWithSameEmailExists
                        && userModel.Password.Equals(userModel.PasswordConfirmation))
                    {
                        userDto.RoleId = userRoleId.Value;
                        var result = await _userService.RegisterUser(userDto, userModel.Password);

                        if (result <= 0)
                        {
                            return Conflict(new ErrorModel
                            {
                                ErrorMessage = $"User with specify {nameof(userModel.Email)} already exists"
                            });
                        }

                        var userInDbDto = await _userService.GetUserWithRoleByEmailAsync(userDto.Email);

                        return userInDbDto != null
                            ? Ok(await _jwtUtil.GenerateTokenAsync(userInDbDto))
                            : StatusCode(503, new ErrorModel
                            {
                                ErrorMessage = "The server is not ready to handle the request."
                            });
                    }
                }

                return BadRequest(new ErrorModel
                {
                    ErrorMessage = "Failed to register a user, please check your input"
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, new ErrorModel
                {
                    ErrorMessage = "The server encountered an unexpected situation."
                });
            }
        }
    }
}
