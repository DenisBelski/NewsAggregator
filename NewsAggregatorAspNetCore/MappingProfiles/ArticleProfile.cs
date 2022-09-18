using AutoMapper;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregatorAspNetCore.MappingProfiles
{
    public class ArticleProfile : Profile
    {
        public ArticleProfile()
        {
            CreateMap<Article, ArticleDto>()
                .ForMember(dto => dto.Id,
                    opt => opt.MapFrom(article => article.Id))
                .ForMember(dto => dto.Title,
                    opt => opt.MapFrom(article => article.Title))
                .ForMember(dto => dto.ShortDescription,
                    opt => opt.MapFrom(article => article.ShortDescription))
                .ForMember(dto => dto.Category, // Category doesn't exist in Entities
                    opt => opt.MapFrom(article => "Default"));
        }
    }
}
