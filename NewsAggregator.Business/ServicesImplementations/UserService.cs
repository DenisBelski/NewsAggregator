using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.Data.Abstractions;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Business.ServicesImplementations
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IMapper mapper,
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> RegisterUser(UserDto dto, string password)
        {
            var user = _mapper.Map<User>(dto);
            user.PasswordHash = CreateMd5($"{password}.{_configuration["Secret:PasswordSalt"]}");

            await _unitOfWork.Users.AddAsync(user);
            return await _unitOfWork.Commit();
        }

        public async Task<bool> IsUserExists(Guid userId)
        {
            return await _unitOfWork.Users
                .Get()
                .AnyAsync(user => user.Id.Equals(userId));
        }

        public async Task<bool> IsUserExists(string email)
        {
            return await _unitOfWork.Users
                .Get()
                .AnyAsync(user => user.Email.Equals(email));
        }


        // get user as entity, with his role
        //public async Task<UserDto> GetUserByEmailAsync(string email)
        //{
        //    var userWithRole = await _unitOfWork.Users
        //        .FindBy(user => user.Email.Equals(email), user => user.Role)
        //        .FirstOrDefaultAsync();

        //    return _mapper.Map<UserDto>(userWithRole);
        //}

        public UserDto? GetUserByEmailAsync(string email)
        {
            var userWithRole = _unitOfWork.Users
                .FindBy(user => user.Email.Equals(email), user => user.Role)
                .FirstOrDefault();

            if (userWithRole != null)
            {
                return _mapper.Map<UserDto>(userWithRole);
            }

            return null;
        }

        public async Task<bool> CheckUserPassword(string email, string password)
        {
            var dbPasswordHash = (await _unitOfWork.Users.Get()
                .FirstOrDefaultAsync(user => user.Email.Equals(email)))
                ?.PasswordHash;

            if (dbPasswordHash != null 
                && CreateMd5($"{password}.{_configuration["Secret:PasswordSalt"]}")
                .Equals(dbPasswordHash))
            {
                return true;
            }

            return false;
        }

        public async Task<bool> CheckUserPassword(Guid userId, string password)
        {
            var dbPasswordHash = (await _unitOfWork.Users.GetByIdAsync(userId))
                ?.PasswordHash;

            if (dbPasswordHash != null 
                && CreateMd5($"{password}.{_configuration["Secret:PasswordSalt"]}")
                .Equals(dbPasswordHash))
            {
                return true;
            }

            return false;
        }

        private string CreateMd5(string password)
        {
            var passwordSalt = _configuration["Secret:PasswordSalt"];

            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = System.Text.Encoding.UTF8.GetBytes(password + passwordSalt);
                var hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes);
            }
        }
    }
}