using AutoMapper;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.DataBase.Entities;
using NewsAggregatorAspNetCore.Models;

namespace NewsAggregatorAspNetCore.MappingProfiles
{
    public class ArticleProfile : Profile
    {
        public ArticleProfile()
        {
            CreateMap<Article, ArticleDto>().ReverseMap();
            CreateMap<ArticleDto, ArticleModel>().ReverseMap();
            CreateMap<ArticleCreationModel, ArticleDto>();
        }
    }
}
