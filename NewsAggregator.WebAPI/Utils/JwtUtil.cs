using MediatR;
using Microsoft.IdentityModel.Tokens;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.Data.CQS.Commands;
using NewsAggregator.WebAPI.Models.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NewsAggregator.WebAPI.Utils
{
    /// <summary>
    /// Utility for working with JWT token.
    /// </summary>
    public class JwtUtilSha256 : IJwtUtil
    {
        private readonly IConfiguration _configuration;
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtUtilSha256"/> class.
        /// </summary>
        /// <param name="configuration">Configuration file.</param>
        /// <param name="mediator"></param>
        public JwtUtilSha256(IConfiguration configuration,
            IMediator mediator)
        {
            _configuration = configuration;
            _mediator = mediator;
        }

        /// <summary>
        /// Method for generating JWT token.
        /// </summary>
        /// <param name="userDto">Contains user id, email, password hash, role id and role name.</param>
        /// <returns></returns>
        public async Task<TokenResponseModel> GenerateTokenAsync(UserDto userDto)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Token:JwtSecret"]));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var nowUtc = DateTime.UtcNow;
            var expirationTime = nowUtc.AddMinutes(double.Parse(_configuration["Token:ExpiryMinutes"]))
                .ToUniversalTime();

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, userDto.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("D")), //jwt uniq id from spec
                new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString("D")),
                new Claim(ClaimTypes.Role, userDto.RoleName),
            };

            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Token:Issuer"],
                _configuration["Token:Issuer"],
                claims,
                expires: expirationTime,
                signingCredentials: credentials);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            var refreshTokenValue = Guid.NewGuid();

            await _mediator.Send(new AddRefreshTokenCommand()
            {
                UserId = userDto.Id,
                TokenValue = refreshTokenValue
            });

            return new TokenResponseModel()
            {
                AccessToken = accessToken,
                Role = userDto.RoleName,
                TokenExpiration = jwtSecurityToken.ValidTo,
                UserId = userDto.Id,
                RefreshToken = refreshTokenValue
            };
        }

        /// <summary>
        /// Method for removing refresh token.
        /// </summary>
        /// <param name="requestRefreshToken">Contains refresh token for removing.</param>
        /// <returns></returns>
        public async Task RemoveRefreshTokenAsync(Guid requestRefreshToken)
        {
            await _mediator.Send(new RemoveRefreshTokenCommand()
            {
                TokenValue = requestRefreshToken
            });
        }
    }
}