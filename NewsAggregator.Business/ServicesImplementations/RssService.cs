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

        public RssService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IMediator mediator)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task GetAllArticleDataFromRssAsync()
        {
            try
            {
                var sourceEntities = await _unitOfWork.Sources.GetAllAsync();
                Parallel.ForEach(sourceEntities, (source) => GetAllArticleDataFromRssAsync(source.Id, source.RssUrl).Wait());
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        public async Task GetAllArticleDataFromRssAsync(Guid sourceId, string? sourceRssUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(sourceRssUrl))
                {
                    var list = new List<ArticleDto>();

                    using (var xmlReader = XmlReader.Create(sourceRssUrl))
                    {
                        var feed = SyndicationFeed.Load(xmlReader);

                        // get information about available fields from rss data or from feed/items
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

                            list.Add(articleDto);
                        }
                    }

                    var oldArticleUrls = await _unitOfWork.Articles.Get()
                        .Select(article => article.SourceUrl)
                        .Distinct()
                        .ToListAsync();

                    var entities = list.Where(dto => !oldArticleUrls.Contains(dto.SourceUrl))
                        .Select(dto => _mapper.Map<Article>(dto))
                        .ToList();

                    await _unitOfWork.Articles.AddRangeArticlesAsync(entities);
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
