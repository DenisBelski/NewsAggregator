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
        private readonly IMediator _mediator;
        private readonly int _limitForUploadArticlesAtOneTime = 20;

        public RssService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IMediator mediator)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task GetArticlesDataFromAllRssSourcesAsync()
        {
            try
            {
                var sourceEntities = await _unitOfWork.Sources.GetAllAsync();

                //await GetArticlesDataFromDevbyRssAsync(new Guid("FB3A7AB4-4588-4BD8-8CAF-6900D2CAE87D"), sourceRssUrl: @"https://money.onliner.by/feed");

                //await GetArticlesDataFromDevbyRssAsync(new Guid("823FB8C7-66CD-4D05-ACBC-609702A80B33"), sourceRssUrl: @"https://devby.io/rss");

                await GetArticlesDataFromShazooRssAsync(new Guid("12E05080-506F-4B91-80CB-1144A52408E3"), sourceRssUrl: @"https://shazoo.ru/feed/rss");

                //Parallel.ForEach(sourceEntities, (source) => GetArticlesDataFromDevbyRssAsync(source.Id, source.RssUrl).Wait());
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        public async Task GetArticlesDataFromAllRssSourcesAsync(Guid sourceId, string? sourceRssUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(sourceRssUrl))
                {
                    var onlinerId = new Guid("FB3A7AB4-4588-4BD8-8CAF-6900D2CAE87D");
                    var devbyId = new Guid("823FB8C7-66CD-4D05-ACBC-609702A80B33");
                    var shazooId = new Guid("12E05080-506F-4B91-80CB-1144A52408E3");

                    if (sourceId == onlinerId)
                    {
                        await GetArticlesDataFromOnlinerRssAsync(sourceId, sourceRssUrl);
                    }
                    else if (sourceId == devbyId)
                    {
                        await GetArticlesDataFromDevbyRssAsync(sourceId, sourceRssUrl);
                    }
                    else if (sourceId == shazooId)
                    {
                        await GetArticlesDataFromShazooRssAsync(sourceId, sourceRssUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        public async Task GetArticlesDataFromOnlinerRssAsync(Guid sourceId, string? sourceRssUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(sourceRssUrl))
                {
                    var listArticleDto = new List<ArticleDto>();

                    using (var xmlReader = XmlReader.Create(sourceRssUrl))
                    {
                        var feed = SyndicationFeed.Load(xmlReader);

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
                        }
                    }

                    var oldArticleUrls = await _unitOfWork.Articles.Get()
                        .Select(article => article.SourceUrl)
                        .Distinct()
                        .ToListAsync();

                    var listArticleEntities = listArticleDto.Where(dto => !oldArticleUrls.Contains(dto.SourceUrl))
                        .Select(dto => _mapper.Map<Article>(dto))
                        .ToList();

                    await _unitOfWork.Articles.AddRangeArticlesAsync(listArticleEntities);
                    await _unitOfWork.Commit();

                    //await _mediator.Send(new AddArticleDataFromRssFeedCommand() { Articles = list });
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        public async Task GetArticlesDataFromDevbyRssAsync(Guid sourceId, string? sourceRssUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(sourceRssUrl))
                {
                    var listArticleDto = new List<ArticleDto>();

                    using (var xmlReader = XmlReader.Create(sourceRssUrl))
                    {
                        var feed = SyndicationFeed.Load(xmlReader);

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
                                    Category = "IT news",
                                    SourceId = sourceId,
                                    SourceUrl = item.Id
                                };

                                listArticleDto.Add(articleDto);
                            }
                        }
                    }

                    var oldArticleUrls = await _unitOfWork.Articles.Get()
                        .Select(article => article.SourceUrl)
                        .Distinct()
                        .ToListAsync();

                    var listArticleEntities = listArticleDto.Where(dto => !oldArticleUrls.Contains(dto.SourceUrl))
                        .Select(dto => _mapper.Map<Article>(dto))
                        .ToList();

                    await _unitOfWork.Articles.AddRangeArticlesAsync(listArticleEntities);
                    await _unitOfWork.Commit();

                    //await _mediator.Send(new AddArticleDataFromRssFeedCommand() { Articles = list });
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        public async Task GetArticlesDataFromShazooRssAsync(Guid sourceId, string? sourceRssUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(sourceRssUrl))
                {
                    var listArticleDto = new List<ArticleDto>();

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
                                Category = "News mix",
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

                    var oldArticleUrls = await _unitOfWork.Articles.Get()
                        .Select(article => article.SourceUrl)
                        .Distinct()
                        .ToListAsync();

                    var listArticleEntities = listArticleDto.Where(dto => !oldArticleUrls.Contains(dto.SourceUrl))
                        .Select(dto => _mapper.Map<Article>(dto))
                        .ToList();

                    await _unitOfWork.Articles.AddRangeArticlesAsync(listArticleEntities);
                    await _unitOfWork.Commit();

                    //await _mediator.Send(new AddArticleDataFromRssFeedCommand() { Articles = list });
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

    }
}
