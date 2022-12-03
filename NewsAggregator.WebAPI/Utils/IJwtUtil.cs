using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.WebAPI.Models.Responses;

namespace NewsAggregator.WebAPI.Utils
{
    public interface IJwtUtil
    {
        Task<TokenResponse> GenerateTokenAsync(UserDto dto);
        Task RemoveRefreshTokenAsync(Guid requestRefreshToken);
    }
}