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
                .ForMember(dto => dto.ArticleText,
                    opt => opt.MapFrom(article => article.ArticleText))
                .ForMember(dto => dto.ShortDescription,
                    opt => opt.MapFrom(article => article.ShortDescription))
                .ForMember(dto => dto.Category, // Category doesn't exist in Entities
                    opt => opt.MapFrom(article => "Default"));

            CreateMap<ArticleDto, Article>()
                .ForMember(dto => dto.ArticleText,
                    opt => opt.MapFrom(article => article.ArticleText))
                .ForMember(article => article.ShortDescription, 
                    opt => opt.MapFrom(article => article.ShortDescription))
                .ForMember(article => article.SourceId, 
                    opt => opt.MapFrom(article => new Guid("2F5A053B-2344-471B-AD99-5768483DD535")));
        }
    }
}
