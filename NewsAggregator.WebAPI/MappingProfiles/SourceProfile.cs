using AutoMapper;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.WebAPI.MappingProfiles
{
    public class SourceProfile : Profile
    {
        public SourceProfile()
        {
            CreateMap<Source, SourceDto>();
            CreateMap<SourceDto, Source>();
        }
    }
}
