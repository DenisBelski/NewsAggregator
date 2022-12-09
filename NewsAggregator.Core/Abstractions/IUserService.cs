using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions;

public interface IUserService
{
    Task<int> RegisterUser(UserDto userDto, string password);
    Task<UserDto?> GetUserWithRoleByEmailAsync(string email);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<int> UpdateUserAsync(UserDto userDto);
    Task DeleteUserByIdAsync(Guid userId);
    Task<bool> IsUserExists(Guid userId);
    Task<bool> IsUserExists(string email);
    Task<bool> CheckUserPassword(Guid userId, string password);
    Task<bool> CheckUserPassword(string email, string password);
    Task<UserDto?> GetUserByRefreshTokenAsync(Guid token);
}