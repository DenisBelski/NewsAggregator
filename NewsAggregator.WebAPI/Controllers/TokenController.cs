﻿using Microsoft.AspNetCore.Mvc;
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
        private readonly IJwtUtil _jwtUtil;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="jwtUtil"></param>
        public TokenController(IUserService userService,
            IJwtUtil jwtUtil)
        {
            _userService = userService;
            _jwtUtil = jwtUtil;
        }

        /// <summary>
        /// Login and generate JWT token.
        /// </summary>
        /// <param name="requestModel">Contains user email and user password.</param>
        /// <returns></returns>
        [Route("Create")]
        [HttpPost]
        [ProducesResponseType(typeof(TokenResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateJwtToken([FromBody] LoginUserRequestModel requestModel)
        {
            try
            {
                if (!string.IsNullOrEmpty(requestModel.Email)
                    && !string.IsNullOrEmpty(requestModel.Password))
                {
                    var userDto = await _userService.GetUserWithRoleByEmailAsync(requestModel.Email);

                    if (userDto == null)
                    {
                        return NotFound(new ErrorResponseModel() { ErrorMessage = "User does't exist." });
                    }

                    var isPassCorrect = await _userService
                        .CheckUserPassword(requestModel.Email, requestModel.Password);

                    return isPassCorrect
                        ? Ok(await _jwtUtil.GenerateTokenAsync(userDto))
                        : BadRequest(new ErrorResponseModel() { ErrorMessage = "Password is incorrect." });
                }

                return BadRequest(new ErrorResponseModel() { ErrorMessage = "Request model isn't valid." });
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500, new ErrorResponseModel
                {
                    ErrorMessage = "The server encountered an unexpected situation."
                });
            }
        }

        /// <summary>
        /// Refresh token.
        /// </summary>
        /// <param name="requestModel">Contains refresh token as a <see cref="Guid"/>.</param>
        /// <returns></returns>
        [Route("Refresh")]
        [HttpPost]
        [ProducesResponseType(typeof(TokenResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestModel requestModel)
        {
            try
            {
                var userDto = await _userService.GetUserByRefreshTokenAsync(requestModel.RefreshToken);

                if (userDto == null)
                {
                    return NotFound(new ErrorResponseModel() 
                    { 
                        ErrorMessage = "User with specified token id does't exist."
                    });
                }

                var tokenResponse = await _jwtUtil.GenerateTokenAsync(userDto);
                await _jwtUtil.RemoveRefreshTokenAsync(requestModel.RefreshToken);

                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500, new ErrorResponseModel
                {
                    ErrorMessage = "The server encountered an unexpected situation."
                });
            }
        }

        /// <summary>
        /// Revoke token.
        /// </summary>
        /// <param name="requestModel">Contains refresh token as a <see cref="Guid"/>.</param>
        /// <returns></returns>
        [Route("Revoke")]
        [HttpPost]
        [ProducesResponseType(typeof(SuccessResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RevokeToken([FromQuery] RefreshTokenRequestModel requestModel)
        {
            try
            {
                var userDto = await _userService.GetUserByRefreshTokenAsync(requestModel.RefreshToken);

                if (userDto == null)
                {
                    return NotFound(new ErrorResponseModel() 
                    { 
                        ErrorMessage = "User with specified token id does't exist." 
                    });
                }

                await _jwtUtil.RemoveRefreshTokenAsync(requestModel.RefreshToken);
                return Ok(new SuccessResponseModel { DetailMessage = "Token revoked successfully." });
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500, new ErrorResponseModel
                {
                    ErrorMessage = "The server encountered an unexpected situation."
                });
            }
        }
    }
}
