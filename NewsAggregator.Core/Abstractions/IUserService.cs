using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions;

public interface IUserService
{
    Task<bool> IsUserExists(Guid userId);
    Task<bool> IsUserExists(string email);
    Task<bool> CheckUserPassword(string email, string password);
    Task<bool> CheckUserPassword(Guid userId, string password);
    Task<int> RegisterUser(UserDto dto, string password);
    UserDto? GetUserByEmailAsync(string email);

    //Task<IEnumerable<UserDto>> GetAllUsers();
    //Task<UserDto?> GetUserByRefreshTokenAsync(Guid token);

}