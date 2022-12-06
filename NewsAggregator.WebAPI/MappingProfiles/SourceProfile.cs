using AutoMapper;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.DataBase.Entities;
using NewsAggregator.WebAPI.Models.Responses;

namespace NewsAggregator.WebAPI.MappingProfiles
{
    public class SourceProfile : Profile
    {
        public SourceProfile()
        {
            CreateMap<Source, SourceDto>().ReverseMap();
            CreateMap<SourceDto, SourceResponseModel>();
        }
    }
}
