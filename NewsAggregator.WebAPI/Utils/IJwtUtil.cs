using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.WebAPI.Models.Responses;

namespace NewsAggregator.WebAPI.Utils
{
    /// <summary>
    /// Utility interface for working with JWT token.
    /// </summary>
    public interface IJwtUtil
    {
        /// <summary>
        /// Method for generating JWT token.
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        Task<TokenResponse> GenerateTokenAsync(UserDto userDto);

        /// <summary>
        /// Method for removing refresh token.
        /// </summary>
        /// <param name="requestRefreshToken"></param>
        /// <returns></returns>
        Task RemoveRefreshTokenAsync(Guid requestRefreshToken);
    }
}