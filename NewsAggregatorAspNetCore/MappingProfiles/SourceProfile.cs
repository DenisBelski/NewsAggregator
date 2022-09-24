using AutoMapper;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.DataBase.Entities;
using NewsAggregatorAspNetCore.Models;

namespace NewsAggregatorAspNetCore.MappingProfiles
{
    public class SourceProfile : Profile
    {
        public SourceProfile()
        {
            CreateMap<Source, SourceDto>();
            CreateMap<SourceDto, Source>();

            CreateMap<SourceModel, SourceDto>();
            CreateMap<SourceDto, SourceModel>();
        }
    }
}
