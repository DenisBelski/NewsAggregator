using AutoMapper;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.DataBase.Entities;
using NewsAggregator.WebAPI.Models.Requests;
using NewsAggregator.WebAPI.Models.Responses;

namespace NewsAggregator.WebAPI.MappingProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dto => dto.RoleName,
                    opt => opt.MapFrom(entity => entity.Role.Name));

            CreateMap<UserDto, User>()
                .ForMember(entity => entity.Id,
                    opt => opt.MapFrom(dto => Guid.NewGuid()))
                .ForMember(entity => entity.RegistrationDate,
                    opt => opt.MapFrom(dto => DateTime.Now));

            CreateMap<RegisterUserRequestModel, UserDto>();
            CreateMap<UserDto, UserResponseModel>();
        }
    }
}
