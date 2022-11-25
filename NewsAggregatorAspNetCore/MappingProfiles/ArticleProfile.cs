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
            CreateMap<Article, ArticleDto>();
            CreateMap<ArticleDto, Article>();

            CreateMap<ArticleDto, ArticleModel>().ReverseMap();


            //CreateMap<Article, ArticleDto>()
            //    .ForMember(dto => dto.Id,
            //        opt => opt.MapFrom(article => article.Id))
            //    .ForMember(dto => dto.Title,
            //        opt => opt.MapFrom(article => article.Title))
            //    .ForMember(dto => dto.Text,
            //        opt => opt.MapFrom(article => article.Text))
            //    .ForMember(dto => dto.ShortSummary,
            //        opt => opt.MapFrom(article => article.ShortSummary))
            //    .ForMember(dto => dto.Category, // Category doesn't exist in Entities
            //        opt => opt.MapFrom(article => "Default"));

            //CreateMap<ArticleDto, Article>()
            //    .ForMember(dto => dto.Text,
            //        opt => opt.MapFrom(article => article.Text))
            //    .ForMember(article => article.ShortSummary,
            //        opt => opt.MapFrom(article => article.ShortSummary))
            //    .ForMember(article => article.SourceId,
            //        opt => opt.MapFrom(article => new Guid("2F5A053B-2344-471B-AD99-5768483DD535")));
        }
    }
}
