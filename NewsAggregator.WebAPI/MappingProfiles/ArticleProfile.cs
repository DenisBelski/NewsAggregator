using AutoMapper;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.WebAPI.MappingProfiles
{
    public class ArticleProfile : Profile
    {
        public ArticleProfile()
        {
            CreateMap<Article, ArticleDto>()
                .ForMember(dto => dto.Id,
                    opt => opt.MapFrom(article => article.Id))
                .ForMember(dto => dto.ArticleText,
                    opt => opt.MapFrom(article => article.ArticleText))
                .ForMember(dto => dto.ShortDescription,
                    opt => opt.MapFrom(article => article.ShortDescription));

            CreateMap<ArticleDto, Article>()
                .ForMember(dto => dto.ArticleText,
                    opt => opt.MapFrom(article => article.ArticleText))
                .ForMember(article => article.ShortDescription, opt
                    => opt.MapFrom(article => article.ShortDescription));
        }
    }
}
