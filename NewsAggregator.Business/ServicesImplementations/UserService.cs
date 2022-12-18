using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.Data.Abstractions;
using NewsAggregator.Data.CQS.Queries;
using NewsAggregator.DataBase.Entities;
using Serilog;

namespace NewsAggregator.Business.ServicesImplementations
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IMediator _mediator;

        public UserService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IMediator mediator)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _mediator = mediator;
        }

        public async Task<int> RegisterUser(UserDto userDto, string password)
        {
            var userEntity = _mapper.Map<User>(userDto);

            if (userEntity != null)
            {
                userEntity.PasswordHash = CreateMd5($"{password}.{_configuration["Secret:PasswordSalt"]}");

                await _unitOfWork.Users.AddAsync(userEntity);
                return await _unitOfWork.Commit();
            }

            Log.Warning($"The logic in {nameof(RegisterUser)} method wasn't implemented, " +
                $"check the parameter: {nameof(userDto)}");

            return -1;
        }

        public async Task<UserDto?> GetUserWithRoleByEmailAsync(string email)
        {
            var userWithRoleEntity = await _unitOfWork.Users
                .FindBy(user => user.Email.Equals(email), user => user.Role)
                .FirstOrDefaultAsync();

            return userWithRoleEntity != null
                ? _mapper.Map<UserDto>(userWithRoleEntity)
                : null;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            try
            {
                var userEntities = await _unitOfWork.Users.GetAllAsync();
                return _mapper.Map<List<UserDto>>(userEntities);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task<int> UpdateUserAsync(UserDto userDto)
        {
            var userEntity = _mapper.Map<User>(userDto);

            if (userEntity != null)
            {
                _unitOfWork.Users.Update(userEntity);
                return await _unitOfWork.Commit();
            }

            return -1;
        }

        public async Task DeleteUserByIdAsync(Guid userId)
        {
            try
            {
                var userEntity = await _unitOfWork.Users.GetByIdAsync(userId);

                if (userEntity != null)
                {
                    _unitOfWork.Users.RemoveUser(userEntity);
                    await _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new ArgumentException(ex.Message, nameof(userId));
            }
        }

        public async Task<bool> IsUserExists(Guid userId)
        {
            if (!Guid.Empty.Equals(userId))
            {
                return await _unitOfWork.Users
                    .Get()
                    .AnyAsync(user => user.Id.Equals(userId));
            }

            return false;
        }

        public async Task<bool> IsUserExists(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                return await _unitOfWork.Users
                    .Get()
                    .AnyAsync(user => user.Email.Equals(email));
            }

            return false;
        }

        public async Task<bool> CheckUserPassword(Guid userId, string password)
        {
            try
            {
                var dbPasswordHash = (await _unitOfWork.Users.GetByIdAsync(userId))?.PasswordHash;

                return
                    dbPasswordHash != null
                    && CreateMd5($"{password}.{_configuration["Secret:PasswordSalt"]}").Equals(dbPasswordHash);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task<bool> CheckUserPassword(string email, string password)
        {
            try
            {
                var dbPasswordHash = (await _unitOfWork.Users.Get().FirstOrDefaultAsync(
                    user => user.Email.Equals(email)))?.PasswordHash;

                return
                    dbPasswordHash != null
                    && CreateMd5($"{password}.{_configuration["Secret:PasswordSalt"]}").Equals(dbPasswordHash);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new InvalidOperationException(ex.Message);
            }
        }

        private string CreateMd5(string password)
        {
            try
            {
                var passwordSalt = _configuration["Secret:PasswordSalt"];

                using System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
                var inputBytes = System.Text.Encoding.UTF8.GetBytes(password + passwordSalt);
                var hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task<UserDto?> GetUserByRefreshTokenAsync(Guid refreshToken)
        {
            var userDto = await _mediator.Send(new GetUserByRefreshTokenQuery() { RefreshToken = refreshToken });

            return userDto;
        }
    }
}