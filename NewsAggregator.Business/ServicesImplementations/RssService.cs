using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.Data.Abstractions;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;
using System.ServiceModel.Syndication;
using System.Xml;

namespace NewsAggregator.Business.ServicesImplementations
{
    public class RssService : IRssService
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public RssService(IMapper mapper,
            IConfiguration configuration, 
            IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }


        //use Parallel for each sourceRssUrl, get data from sources effectively
        public async Task GetAllArticleDataFromRssAsync()
        {
            var sources = await _unitOfWork.Sources.GetAllAsync();
            Parallel.ForEach(sources, (source) => GetAllArticleDataFromRssAsync(source.Id, source.RssUrl).Wait());
        }



        private async Task GetAllArticleDataFromRssAsync(Guid sourceId, string? sourceRssUrl)
        {
            if (!string.IsNullOrEmpty(sourceRssUrl))
            {
                var list = new List<ArticleDto>();

                using (var reader = XmlReader.Create(sourceRssUrl))
                {
                    var feed = SyndicationFeed.Load(reader);

                    // get information about available fields from rss data or from feed/items

                    //should be checked for different rss sources
                    //list.AddRange(feed.Items.Select(item => new ArticleDto()
                    //{
                    //    Id = Guid.NewGuid(),
                    //    Title = item.Title.Text,
                    //    PublicationDate = item.PublishDate.UtcDateTime,
                    //    ShortSummary = item.Summary.Text,
                    //    Category = item.Categories.FirstOrDefault()?.Name,
                    //    SourceId = sourceId,
                    //    SourceUrl = item.Id
                    //}));

                    foreach (SyndicationItem item in feed.Items)
                    {
                        var articleDto = new ArticleDto()
                        {
                            Id = Guid.NewGuid(),
                            Title = item.Title.Text,
                            PublicationDate = item.PublishDate.UtcDateTime,
                            ShortDescription = item.Summary.Text,
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
                    .ToListAsync(); // or ToArrayAsync

                var entities = list.Where(dto => !oldArticleUrls.Contains(dto.SourceUrl))
                    .Select(dto => _mapper.Map<Article>(dto))
                    .ToList(); // or ToArray

                //var entities = list.Select(dto => _mapper.Map<Article>(dto));
                await _unitOfWork.Articles.AddRangeAsync(entities);
                await _unitOfWork.Commit();

                //await _mediator.Send(new AddArticleDataFromRssFeedCommand()
                //{ Articles = list });
            }
        }




        //public async Task AggregateArticlesFromExternalSourcesAsync()
        //{
        //    var sources = await _unitOfWork.Sources.GetAllAsync();

        //    foreach (var source in sources)
        //    {
        //        await GetAllArticleDataFromRssAsync(source.Id, source.RssUrl);
        //        await AddArticleTextToArticlesAsync();
        //    }
        //}



        //public async Task AddRateToArticlesAsync()
        //{
        //    var articlesWithEmptyRateIds = _unitOfWork.Articles.Get()
        //        .Where(article => article.Rate == null && !string.IsNullOrEmpty(article.Text))
        //        .Select(article => article.Id)
        //        .ToList();

        //    foreach (var articleId in articlesWithEmptyRateIds)
        //    {
        //        await RateArticleAsync(articleId);
        //    }
        //}


        //private async Task RateArticleAsync(Guid articleId)
        //{
        //    try
        //    {
        //        var article = await _unitOfWork.Articles.GetByIdAsync(articleId);

        //        if (article == null)
        //        {
        //            throw new ArgumentException($"Article with id: {articleId} doesn't exists",
        //                nameof(articleId));
        //        }

        //        using (var client = new HttpClient())
        //        {
        //            var httpRequest = new HttpRequestMessage(HttpMethod.Post,
        //                new Uri(@"https://api.ispras.ru/texterra/v1/nlp?targetType=lemma&apikey=YOUR_KEY"));
        //            httpRequest.Headers.Add("Accept", "application/json");

        //            httpRequest.Content = JsonContent.Create(new[] { new TextRequestModel() { Text = article.Text } });

        //            var response = await client.SendAsync(httpRequest);
        //            var responseStr = await response.Content.ReadAsStreamAsync();

        //            using (var sr = new StreamReader(responseStr))
        //            {
        //                var data = await sr.ReadToEndAsync();

        //                var resp = JsonConvert.DeserializeObject<IsprassResponseObject[]>(data);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}
    }
}
