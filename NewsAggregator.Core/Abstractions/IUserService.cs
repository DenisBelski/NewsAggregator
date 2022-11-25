using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions;

public interface IUserService
{
    Task<int> RegisterUser(UserDto dto, string password);
    Task<bool> IsUserExists(Guid userId);
    Task<UserDto> GetUserByEmailAsync(string email);
    Task<bool> CheckUserPassword(string email, string password);
    Task<bool> CheckUserPassword(Guid userId, string password);
}