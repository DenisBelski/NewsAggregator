using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.Data.Abstractions;
using NewsAggregator.Data.CQS.Commands;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;

namespace NewsAggregator.Business.ServicesImplementations
{
    public class RssService : IRssService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IMediator _mediator;
        private readonly int _limitForUploadArticlesAtOneTime = 10;

        public RssService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IMediator mediator)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _mediator = mediator;
        }

        public async Task GetArticlesDataFromAllAvailableRssSourcesAsync()
        {
            try
            {
                var sourceEntities = await _unitOfWork.Sources.GetAllAsync();

                foreach (var sourceEntity in sourceEntities)
                {
                    await GetArticlesDataFromRssSourceWithSpecifiedIdAsync(sourceEntity.Id);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        public async Task GetArticlesDataFromRssSourceWithSpecifiedIdAsync(Guid sourceId)
        {
            try
            {
                var sourceEntity = await _unitOfWork.Sources.GetByIdAsync(sourceId);
                var listArticleDto = new List<ArticleDto>();

                if (sourceEntity != null && sourceEntity.RssUrl != null)
                {
                    if (sourceEntity.Id == new Guid(_configuration["AvailableRssSources:OnlinerId"]))
                    {
                        listArticleDto = GetArticlesDataFromOnliner(sourceEntity.Id, 
                            sourceEntity.RssUrl, listArticleDto);
                    }
                    else if (sourceEntity.Id == new Guid(_configuration["AvailableRssSources:DevbyId"]))
                    {
                        listArticleDto = GetArticlesDataFromDevby(sourceEntity.Id, 
                            sourceEntity.RssUrl, listArticleDto);
                    }
                    else if (sourceEntity.Id == new Guid(_configuration["AvailableRssSources:ShazooId"]))
                    {
                        listArticleDto = GetArticlesDataFromShazoo(sourceEntity.Id, 
                            sourceEntity.RssUrl, listArticleDto);
                    }

                    var oldArticleUrls = await _unitOfWork.Articles.Get()
                        .Select(article => article.SourceUrl)
                        .Distinct()
                        .ToListAsync();

                    var listArticleEntities = listArticleDto.Where(dto => !oldArticleUrls
                        .Contains(dto.SourceUrl))
                        .Select(dto => _mapper.Map<Article>(dto))
                        .ToList();

                    await _unitOfWork.Articles.AddRangeArticlesAsync(listArticleEntities);
                    await _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message, nameof(sourceId));
            }
        }

        private List<ArticleDto> GetArticlesDataFromOnliner(Guid sourceId, 
            string sourceRssUrl, List<ArticleDto> listArticleDto)
        {
            using (var xmlReader = XmlReader.Create(sourceRssUrl))
            {
                var feed = SyndicationFeed.Load(xmlReader);
                var counter = 0;

                foreach (SyndicationItem item in feed.Items)
                {
                    var textSummary = Regex.Replace(item.Summary.Text, @"<[^>]*>", String.Empty);

                    var articleDto = new ArticleDto()
                    {
                        Id = Guid.NewGuid(),
                        Title = Regex.Replace(item.Title.Text, "&nbsp", String.Empty),
                        PublicationDate = item.PublishDate.UtcDateTime,
                        ShortDescription = Regex.Replace(textSummary, "Читать далее…", String.Empty),
                        Category = item.Categories.FirstOrDefault()?.Name,
                        SourceId = sourceId,
                        SourceUrl = item.Id
                    };

                    listArticleDto.Add(articleDto);
                    counter++;

                    if (counter >= _limitForUploadArticlesAtOneTime)
                    {
                        break;
                    }
                }

                return listArticleDto;
            }
        }

        private List<ArticleDto> GetArticlesDataFromDevby(Guid sourceId,
            string sourceRssUrl, List<ArticleDto> listArticleDto)
        {
            using (var xmlReader = XmlReader.Create(sourceRssUrl))
            {
                var feed = SyndicationFeed.Load(xmlReader);
                var counter = 0;

                foreach (SyndicationItem item in feed.Items)
                {
                    if (item.Summary != null)
                    {
                        var articleDto = new ArticleDto()
                        {
                            Id = Guid.NewGuid(),
                            Title = item.Title.Text,
                            PublicationDate = item.PublishDate.UtcDateTime,
                            ShortDescription = item.Summary.Text,
                            Category = _configuration["AvailableRssSources:AdditionalCategory"],
                            SourceId = sourceId,
                            SourceUrl = item.Id
                        };

                        listArticleDto.Add(articleDto);
                        counter++;

                        if (counter >= _limitForUploadArticlesAtOneTime)
                        {
                            break;
                        }
                    }
                }

                return listArticleDto;
            }
        }

        private List<ArticleDto> GetArticlesDataFromShazoo(Guid sourceId,
            string sourceRssUrl, List<ArticleDto> listArticleDto)
        {
            using (var xmlReader = XmlReader.Create(sourceRssUrl))
            {
                var feed = SyndicationFeed.Load(xmlReader);
                var counter = 0;

                foreach (SyndicationItem item in feed.Items)
                {
                    var articleDto = new ArticleDto()
                    {
                        Id = Guid.NewGuid(),
                        Title = item.Title.Text,
                        PublicationDate = item.PublishDate.UtcDateTime,
                        ShortDescription = item.Title.Text,
                        Category = _configuration["AvailableRssSources:DefaultCategory"],
                        SourceId = sourceId,
                        SourceUrl = item.Id
                    };

                    listArticleDto.Add(articleDto);
                    counter++;

                    if (counter >= _limitForUploadArticlesAtOneTime)
                    {
                        break;
                    }
                }

                return listArticleDto;
            }
        }
    }
}
