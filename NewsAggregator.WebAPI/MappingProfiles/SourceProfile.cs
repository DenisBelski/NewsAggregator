using AutoMapper;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.DataBase.Entities;
using NewsAggregator.WebAPI.Models.Responses;

namespace NewsAggregator.WebAPI.MappingProfiles
{
    /// <summary>
    /// A profile for copying (mapping) property values of one source to a new source with other properties.
    /// </summary>
    public class SourceProfile : Profile
    {
        /// <summary>
        /// A method that includes mapping implementations.
        /// </summary>
        public SourceProfile()
        {
            CreateMap<Source, SourceDto>().ReverseMap();
            CreateMap<SourceDto, SourceResponseModel>();
        }
    }
}
