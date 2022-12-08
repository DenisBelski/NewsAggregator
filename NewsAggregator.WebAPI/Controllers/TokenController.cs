using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.WebAPI.Models.Requests;
using NewsAggregator.WebAPI.Models.Responses;
using NewsAggregator.WebAPI.Utils;
using Serilog;

namespace NewsAggregator.WebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IJwtUtil _jwtUtil;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="roleService"></param>
        /// <param name="mapper"></param>
        /// <param name="jwtUtil"></param>
        public TokenController(IUserService userService,
            IRoleService roleService, IMapper mapper,
            IJwtUtil jwtUtil)
        {
            _userService = userService;
            _mapper = mapper;
            _jwtUtil = jwtUtil;
        }

        /// <summary>
        /// Register user
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateJwtToken([FromBody] LoginUserRequestModel requestModel)
        {
            try
            {
                if (requestModel != null 
                    && requestModel.Email != null
                    && requestModel.Password != null)
                {
                    var userDto = await _userService.GetUserWithRoleByEmailAsync(requestModel.Email);

                    if (userDto == null)
                    {
                        return BadRequest(new ErrorModel() { ErrorMessage = "User does't exist" });
                    }

                    var isPassCorrect = await _userService.CheckUserPassword(requestModel.Email, requestModel.Password);

                    if (!isPassCorrect)
                    {
                        return BadRequest(new ErrorModel() { ErrorMessage = "Password is incorrect" });
                    }

                    var response = await _jwtUtil.GenerateTokenAsync(userDto);
                    return Ok(response);
                }

                return BadRequest(new ErrorModel() { ErrorMessage = "Request model isn't valid" });
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Register user
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [Route("Refresh")]
        [HttpPost]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestModel requestModel)
        {
            try
            {
                var userDto = await _userService.GetUserByRefreshTokenAsync(requestModel.RefreshToken);

                if (userDto == null)
                {
                    return BadRequest(new ErrorModel() { ErrorMessage = "User does't exist" });
                }

                var tokenResponse = await _jwtUtil.GenerateTokenAsync(userDto);
                await _jwtUtil.RemoveRefreshTokenAsync(requestModel.RefreshToken);

                return Ok(tokenResponse);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Method for revoke refresh token.
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [Route("Revoke")]
        [HttpPost]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequestModel requestModel)
        {
            try
            {
                await _jwtUtil.RemoveRefreshTokenAsync(requestModel.RefreshToken);

                return Ok();
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return StatusCode(500);
            }
        }
    }
}
