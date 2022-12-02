using AutoMapper;
using HtmlAgilityPack;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NewsAggregator.Business.Models;
using NewsAggregator.Core;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.Data.Abstractions;
using NewsAggregator.Data.CQS.Commands;
using NewsAggregator.Data.CQS.Queries;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;
using Newtonsoft.Json;
using Serilog;
using System.Net.Http.Json;
using System.ServiceModel.Syndication;
using System.Xml;

namespace NewsAggregator.Business.ServicesImplementations
{
    public class ArticleService : IArticleService
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRssService _rssService;
        private readonly IMediator _mediator;

        public ArticleService(IMapper mapper,
            IConfiguration configuration, 
            IUnitOfWork unitOfWork,
            IRssService rssService,
            IMediator mediator)
        {
            _mapper = mapper;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _rssService = rssService;
            _mediator = mediator;
        }

        public async Task<int> CreateArticleAsync(ArticleDto articleDto)
        {
            var articleEntity = _mapper.Map<Article>(articleDto);

            if (articleEntity != null)
            {
                await _unitOfWork.Articles.AddAsync(articleEntity);
                var addingResult = await _unitOfWork.Commit();

                return addingResult;
            }

            return -1;
        }

        public async Task<int> CreateArticlesAsync(IEnumerable<ArticleDto> articlesDto)
        {
            var articleEntities = _mapper.Map<IEnumerable<Article>>(articlesDto);

            if (articleEntities != null)
            {
                await _unitOfWork.Articles.AddRangeArticlesAsync(articleEntities);
                return await _unitOfWork.Commit();
            }

            return -1;
        }

        public async Task<ArticleDto?> GetArticleByIdAsync(Guid articleId)
        {
            var articleEntity = await _unitOfWork.Articles.GetByIdAsync(articleId);

            if (articleEntity != null)
            {
                //return _mapper.Map<ArticleDto>(await _mediator.Send(new GetArticleByIdQuery() { Id = id }));

                return _mapper.Map<ArticleDto>(articleEntity);
            }

            return null;
        }

        public async Task<int> UpdateArticleAsync(ArticleDto articleDto)
        {
            var articleEntity = _mapper.Map<Article>(articleDto);

            if (articleEntity != null)
            {
                _unitOfWork.Articles.Update(articleEntity);
                return await _unitOfWork.Commit();
            }

            return -1;
        }

        public async Task<int> PatchArticleAsync(Guid articleId, ArticleDto? patchList)
        {
            //var articleDto = await GetArticleByIdAsync(id);
            //await _unitOfWork.Articles.PatchArticleAsync(id, patchList);

            //should be sure that dto property naming is the same with articleEntity property naming
            //var patchList = new List<PatchModel>();
            //if (dto != null)
            //{
            //    if (!dto.Title.Equals(sourceDto.Title))
            //    {
            //        patchList.Add(new PatchModel()
            //        {
            //            PropertyName = nameof(dto.Title),
            //            PropertyValue = dto.Title
            //        });
            //    }
            //}

            return await _unitOfWork.Commit();
        }

        public async Task<List<ArticleDto>> GetArticlesByPageNumberAsync(int pageNumber)
        {
            var listArticlesDto = await _unitOfWork.Articles
                .Get()
                .Skip(pageNumber)
                .Select(article => _mapper.Map<ArticleDto>(article))
                .ToListAsync();

            return listArticlesDto;
        }

        public async Task<List<ArticleDto>?> GetArticlesBySourceIdAsync(Guid sourceId)
        {
            var articleEntities = _unitOfWork.Articles.Get();

            if (articleEntities != null
                && !Guid.Empty.Equals(sourceId))
            {
                articleEntities = articleEntities.Where(dto => dto.SourceId.Equals(sourceId));

                return (await articleEntities.ToListAsync())
                    .Select(articleEntity => _mapper.Map<ArticleDto>(articleEntity))
                    .ToList();
            }

            return null;
        }

        public async Task AggregateArticlesFromExternalSourcesAsync()
        {
            try
            {
                var sourceEntities = await _unitOfWork.Sources.GetAllAsync();

                foreach (var sourceEntity in sourceEntities)
                {
                    await _rssService.GetAllArticleDataFromRssAsync(sourceEntity.Id, sourceEntity.RssUrl);
                    await AddArticleTextToArticlesFromOnlinerAsync();
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        public async Task AddRateToArticlesAsync()
        {
            try
            {
                var articlesWithEmptyRateIds = _unitOfWork.Articles.Get()
                    .Where(article => article.Rate == null && !string.IsNullOrEmpty(article.ArticleText))
                    .Select(article => article.Id)
                    .ToList();

                foreach (var articleId in articlesWithEmptyRateIds)
                {
                    await RateArticleAsync(articleId);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        public async Task AddArticleTextToArticlesFromOnlinerAsync()
        {
            try
            {
                //var articlesWithEmptyTextIds = _unitOfWork.Articles
                //    .Get()
                //    .Where(article => string.IsNullOrEmpty(article.ArticleText))
                //    .Select(article => article.Id)
                //    .ToList();
                //foreach (var articleId in articlesWithEmptyTextIds)
                //{
                //    await AddArticleTextToArticleFromOnlinerAsync(articleId);
                //}

                var articlesWithEmptyTextIds = await _mediator.Send(new GetAllArticlesWithoutTextIdsQuery());

                foreach (var articleId in articlesWithEmptyTextIds)
                {
                    await AddArticleTextToArticleFromOnlinerAsync(articleId);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        private async Task AddArticleTextToArticleFromOnlinerAsync(Guid articleId)
        {
            try
            {
                //var article = await _unitOfWork.Articles.GetByIdAsync(articleId);

                var article = await _mediator.Send(new GetArticleByIdQuery { Id = articleId });

                if (article != null)
                {
                    var articleSourceUrl = article.SourceUrl;
                    var web = new HtmlWeb();
                    var htmlDoc = web.Load(articleSourceUrl);

                    // get class name "news-text" from web page with news
                    var htmlNodes = htmlDoc.DocumentNode.Descendants(0).Where(n => n.HasClass("news-text"));

                    if (htmlNodes.Any())
                    {
                        var articleText = htmlNodes.FirstOrDefault()?
                            .ChildNodes
                            .Where(node => (node.Name.Equals("p")
                                            || node.Name.Equals("div")
                                            || node.Name.Equals("h2"))
                                            && !node.HasClass("news-reference")
                                            && !node.HasClass("news-banner")
                                            && !node.HasClass("news-widget")
                                            && !node.HasClass("news-vote")
                                            && !node.HasClass("news-incut")
                                            && !node.HasClass("button-style")
                                            && !node.HasClass("news-entry__speech")    //??
                                            && !node.HasClass("news-entry")            //??
                                            && !node.HasClass("news-header")
                                            && !node.HasClass("news-media")
                                            && !node.HasClass("news-media__viewport")
                                            && !node.HasClass("news-media__preview")
                                            && !node.HasClass("news-media__inside")
                                            && !node.HasClass("alignnone")
                                            && node.Attributes["style"] == null)
                            .Select(node => node.InnerText)                            // or => node.InnerText node.OuterHtml/InnerHtml
                            .Aggregate((i, j) => i + Environment.NewLine + j);

                        //await _unitOfWork.Articles.UpdateArticleTextAsync(articleId, articleText);
                        //await _unitOfWork.Commit();

                        await _mediator.Send(new UpdateArticleTextCommand() { Id = articleId, Text = articleText });
                    }
                }
                else
                {
                    Log.Warning($"The logic in {nameof(AddArticleTextToArticleFromOnlinerAsync)} method wasn't implemented, " +
                        $"because {nameof(articleId)} parametr equals null");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        private async Task RateArticleAsync(Guid articleId)
        {
            try
            {
                var article = await _unitOfWork.Articles.GetByIdAsync(articleId);

                if (article != null)
                {
                    using (var client = new HttpClient())
                    {
                        var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                            new Uri(@"http://api.ispras.ru/texterra/v1/nlp?targetType=lemma&apikey=1e03d79d9f01859fefbc04abe0a9c3f3660e117f"));

                        httpRequest.Headers.Add("Accept", "application/json");
                        httpRequest.Content = JsonContent.Create(
                            new[] { new TextRequestModel() { Text = article.ArticleText } });

                        var httpResponse = await client.SendAsync(httpRequest);
                        var responseStream = await httpResponse.Content.ReadAsStreamAsync();

                        using (var stream = new StreamReader(responseStream))
                        {
                            var responseData = await stream.ReadToEndAsync();
                            var responseObject = JsonConvert.DeserializeObject<IsprassResponseObject[]>(responseData);

                            if (responseObject != null)
                            {
                                double? rateResult = CompareArticleWithAfinnDictionary(responseObject[0].Annotations.Lemma);

                                var patchList = new List<PatchModel>()
                                {
                                    new PatchModel()
                                    {
                                        PropertyName = nameof(article.Rate),
                                        PropertyValue = rateResult
                                    }
                                };

                                await _unitOfWork.Articles.PatchArticleAsync(articleId, patchList);
                                await _unitOfWork.Commit();
                            }
                        }
                    }
                }
                else
                {
                    Log.Warning($"The logic in {nameof(RateArticleAsync)} method wasn't implemented, " +
                        $"because {nameof(articleId)} parametr equals null");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        private double? CompareArticleWithAfinnDictionary(List<Lemma> listLemmas)
        {
            using (var stream = new StreamReader(@"D:\IT\GitHub_Projects\NewsAggregator\NewsAggregator.Business\ServicesImplementations\AFINN-ru.json"))
            {
                var afinnData = stream.ReadToEnd();
                var afinnDictionary = JsonConvert.DeserializeObject<Dictionary<string, int>>(afinnData);

                int amountOfEvaluatedWords = 0;
                double totalTextScore = 0;

                if (afinnDictionary != null)
                {
                    foreach (var afinnItem in afinnDictionary)
                    {
                        for (int i = 0; i < listLemmas.Count - 1; i++)
                        {
                            if (!string.IsNullOrEmpty(listLemmas[i].Value)
                                && afinnItem.Key == listLemmas[i].Value)
                            {
                                amountOfEvaluatedWords++;
                                totalTextScore += afinnItem.Value;
                            }
                        }
                    }
                }

                if (amountOfEvaluatedWords != 0)
                {
                    return Math.Round((totalTextScore / amountOfEvaluatedWords), 2);
                }

                return 0;
            }
        }
    }
}
