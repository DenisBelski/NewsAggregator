using AutoMapper;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.DataBase.Entities;
using NewsAggregatorAspNetCore.Models;

namespace NewsAggregatorAspNetCore.MappingProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<UserDto, Source>();

            CreateMap<UserModel, UserDto>();
            CreateMap<UserDto, UserModel>();
        }
    }
}
