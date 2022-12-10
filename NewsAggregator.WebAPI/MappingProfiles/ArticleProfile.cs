using AutoMapper;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.DataBase.Entities;
using NewsAggregator.WebAPI.Models.Responses;

namespace NewsAggregator.WebAPI.MappingProfiles
{
    /// <summary>
    /// A profile for copying (mapping) property values of one article to a new article with other properties.
    /// </summary>
    public class ArticleProfile : Profile
    {
        /// <summary>
        /// A method that includes mapping implementations.
        /// </summary>
        public ArticleProfile()
        {
            CreateMap<Article, ArticleDto>().ReverseMap();
            CreateMap<ArticleDto, ArticleResponseModel>();
        }
    }
}
