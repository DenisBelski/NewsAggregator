using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions;

public interface IUserService
{
    Task<bool> IsUserExists(Guid userId);
    Task<bool> CheckUserPassword(string email, string password);
    Task<bool> CheckUserPassword(Guid userId, string password);
    Task<int> RegisterUser(UserDto dto);
    Task<UserDto> GetUserByEmailAsync(string email);
}