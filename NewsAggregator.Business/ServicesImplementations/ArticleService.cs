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
using Newtonsoft.Json.Converters;
using Serilog;
using System.Net.Http.Json;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;

namespace NewsAggregator.Business.ServicesImplementations
{
    public class ArticleService : IArticleService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IRssService _rssService;
        private readonly IMediator _mediator;

        public ArticleService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IRssService rssService,
            IMediator mediator)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _rssService = rssService;
            _mediator = mediator;
        }

        public async Task<int> CreateArticleAsync(ArticleDto articleDto)
        {
            var articleEntity = _mapper.Map<Article>(articleDto);

            if (articleEntity != null)
            {
                await _unitOfWork.Articles.AddAsync(articleEntity);
                return await _unitOfWork.Commit();
            }

            return -1;
        }

        public async Task<IEnumerable<ArticleDto>> GetArticles()
        {
            var articleEntities = await _unitOfWork.Articles.GetAllAsync();

            return articleEntities != null
                ? _mapper.Map<List<ArticleDto>>(articleEntities)
                : Enumerable.Empty<ArticleDto>();
        }

        public async Task<ArticleDto?> GetArticleByIdAsync(Guid articleId)
        {
            var articleEntity = await _unitOfWork.Articles.GetByIdAsync(articleId);
            //return _mapper.Map<ArticleDto>(await _mediator.Send(new GetArticleByIdQuery() { Id = id }));

            return articleEntity != null
                ? _mapper.Map<ArticleDto>(articleEntity) 
                : null;
        }

        public async Task<IEnumerable<ArticleDto>> GetArticlesByPageNumberAsync(int pageNumber)
        {
            var listArticlesDto = await _unitOfWork.Articles
                .Get()
                .Skip(pageNumber)
                .Select(article => _mapper.Map<ArticleDto>(article))
                .ToListAsync();

            return listArticlesDto != null
                ? _mapper.Map<List<ArticleDto>>(listArticlesDto)
                : Enumerable.Empty<ArticleDto>();
        }

        public async Task<IEnumerable<ArticleDto>> GetArticlesByRateAsync(double? rate)
        {
            if (rate.HasValue)
            {
                var listArticlesDto = await _unitOfWork.Articles
                    .Get()
                    .Where(article => article.Rate != null && article.Rate > rate)
                    .Select(article => _mapper.Map<ArticleDto>(article))
                    .ToListAsync();

                return listArticlesDto;
            }

            return Enumerable.Empty<ArticleDto>();
        }

        public async Task<IEnumerable<ArticleDto>> GetArticlesBySourceIdAsync(Guid? sourceId)
        {
            var articleEntities = _unitOfWork.Articles.Get();

            if (articleEntities != null && !Guid.Empty.Equals(sourceId))
            {
                articleEntities = articleEntities.Where(dto => dto.SourceId.Equals(sourceId));

                return (await articleEntities.ToListAsync())
                    .Select(articleEntity => _mapper.Map<ArticleDto>(articleEntity))
                    .ToList();
            }

            return Enumerable.Empty<ArticleDto>();
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

        public async Task AggregateArticlesFromAllAvailableSourcesAsync()
        {
            try
            {
                await _rssService.GetArticlesDataFromAllAvailableRssSourcesAsync();
                await AddArticleTextToArticlesForAllAvailableSourcesAsync();
                await AddRateToArticlesAsync();
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        public async Task AggregateArticlesFromSourceWithSpecifiedIdAsync(Guid sourceId)
        {
            try
            {
                if (!Guid.Empty.Equals(sourceId))
                {
                    await _rssService.GetArticlesDataFromRssSourceWithSpecifiedIdAsync(sourceId);
                    await AddArticleTextToArticlesForSpecifiedSourceByIdAsync(sourceId);
                    await AddRateToArticlesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        public async Task RateArticleByIdAsync(Guid articleId, IReadOnlyDictionary<string, int> afinnDictionary)
        {
            try
            {
                var article = await _unitOfWork.Articles.GetByIdAsync(articleId);

                if (article != null && !string.IsNullOrEmpty(article.ArticleText))
                {
                    var rateResult = await GetArticleRateByArticleTextAsync(article.ArticleText, afinnDictionary);

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
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message, nameof(articleId));
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

                using (var stream = new StreamReader(_configuration["AfinnJson:JsonPath"]))
                {
                    var afinnData = stream.ReadToEnd();
                    var afinnDictionary = JsonConvert.DeserializeObject<IReadOnlyDictionary<string, int>>(afinnData);

                    if (afinnDictionary != null)
                    {
                        foreach (var articleId in articlesWithEmptyRateIds)
                        {
                            await RateArticleByIdAsync(articleId, afinnDictionary);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        public async Task<double?> GetArticleRateByArticleTextAsync(string articleText)
        {
            try
            {
                using (var stream = new StreamReader(_configuration["AfinnJson:JsonPath"]))
                {
                    var afinnData = stream.ReadToEnd();
                    var afinnDictionary = JsonConvert.DeserializeObject<IReadOnlyDictionary<string, int>>(afinnData);

                    if (string.IsNullOrEmpty(articleText) && afinnDictionary != null)
                    {
                        return await GetArticleRateByArticleTextAsync(articleText, afinnDictionary);
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        public async Task<double?> GetArticleRateByArticleTextAsync(string articleText, IReadOnlyDictionary<string, int> afinnDictionary)
        {
            if (!string.IsNullOrEmpty(articleText))
            {
                using (var client = new HttpClient())
                {
                    var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                        new Uri(_configuration["Ispras:Url"]));

                    httpRequest.Headers.Add("Accept", "application/json");
                    httpRequest.Content = JsonContent.Create(
                        new[] { new TextRequestModel() { Text = articleText } });

                    var httpResponse = await client.SendAsync(httpRequest);
                    var responseStream = await httpResponse.Content.ReadAsStreamAsync();

                    using (var stream = new StreamReader(responseStream))
                    {
                        var responseData = await stream.ReadToEndAsync();
                        var responseObject = JsonConvert.DeserializeObject<IsprassResponseObject[]>(responseData);

                        return responseObject != null
                            ? CompareArticleWithAfinnDictionary(responseObject[0].Annotations.Lemma, afinnDictionary)
                            : null;
                    }
                }
            }
            else
            {
                Log.Warning($"{nameof(articleText)} parametr equals null");
                return null;
            }
        }

        private async Task AddArticleTextToArticlesForAllAvailableSourcesAsync()
        {
            try
            {
                var articlesWithEmptyTextIdsAndSpecifySource = _unitOfWork.Articles
                    .Get()
                    .Where(article => string.IsNullOrEmpty(article.ArticleText))
                    .Select(article => article.Id)
                    .ToList();

                foreach (var articleId in articlesWithEmptyTextIdsAndSpecifySource)
                {
                    await AddArticleTextToArticleBySourceIdAsync(articleId);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        private async Task AddArticleTextToArticlesForSpecifiedSourceByIdAsync(Guid sourceId)
        {
            try
            {
                var articlesWithEmptyTextIdsAndSpecifySource = _unitOfWork.Articles
                    .Get()
                    .Where(article => string.IsNullOrEmpty(article.ArticleText) && article.SourceId == sourceId)
                    .Select(article => article.Id)
                    .ToList();

                foreach (var articleId in articlesWithEmptyTextIdsAndSpecifySource)
                {
                    await AddArticleTextToArticleBySourceIdAsync(articleId);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        private async Task AddArticleTextToArticleBySourceIdAsync(Guid articleId)
        {
            try
            {
                var articleEntity = await _unitOfWork.Articles.GetByIdAsync(articleId);

                if (articleEntity != null)
                {
                    var articleSourceUrl = articleEntity.SourceUrl;
                    var web = new HtmlWeb();
                    var htmlDoc = web.Load(articleSourceUrl);
                    var articleText = String.Empty;

                    if (articleEntity.SourceId == new Guid(_configuration["AvailableRssSources:OnlinerId"]))
                    {
                        articleText = GetArticleTextFromOnliner(htmlDoc.DocumentNode
                            .Descendants(0).Where(n => n.HasClass("news-text")));
                    }
                    else if (articleEntity.SourceId == new Guid(_configuration["AvailableRssSources:DevbyId"]))
                    {
                        articleText = GetArticleTextFromDevby(htmlDoc.DocumentNode
                            .Descendants(0).Where(n => n.HasClass("article__body")));
                    }
                    else if (articleEntity.SourceId == new Guid(_configuration["AvailableRssSources:ShazooId"]))
                    {
                        articleText = GetArticleTextFromOnliner(htmlDoc.DocumentNode
                            .Descendants(0).Where(n => n.HasClass("Entry__content")));

                        var patchList = new List<PatchModel>()
                        {
                            new PatchModel()
                            {
                                PropertyName = nameof(articleEntity.ShortDescription),
                                PropertyValue = articleText
                            }
                        };

                        await _unitOfWork.Articles.PatchArticleAsync(articleId, patchList);
                    }

                    if (!string.IsNullOrEmpty(articleText))
                    {
                        await _unitOfWork.Articles.UpdateArticleTextAsync(articleId, articleText);
                        await _unitOfWork.Commit();
                    }
                }
                else
                {
                    Log.Warning($"The logic in {nameof(AddArticleTextToArticleBySourceIdAsync)} method wasn't implemented, " +
                        $"because {nameof(articleId)} parametr equals null");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        private string? GetArticleTextFromOnliner(IEnumerable<HtmlNode> htmlNodes)
        {
            if (htmlNodes.Any())
            {
                return htmlNodes.FirstOrDefault()?
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
                                    && !node.HasClass("news-entry__speech")
                                    && !node.HasClass("news-entry")
                                    && !node.HasClass("news-header")
                                    && !node.HasClass("news-media")
                                    && !node.HasClass("news-media__viewport")
                                    && !node.HasClass("news-media__preview")
                                    && !node.HasClass("news-media__inside")
                                    && !node.HasClass("alignnone")
                                    && node.Attributes["style"] == null)
                    .Select(node => node.InnerText)
                    .Aggregate((i, j) => i + Environment.NewLine + j);
            }

            return String.Empty;
        }

        private string? GetArticleTextFromDevby(IEnumerable<HtmlNode> htmlNodes)
        {
            if (htmlNodes.Any())
            {
                return htmlNodes.FirstOrDefault()?
                    .ChildNodes
                    .Where(node => (node.Name.Equals("p")
                                    || node.Name.Equals("div")
                                    || node.Name.Equals("li"))
                                    && !node.Name.Equals("a")
                                    && !node.Name.Equals("h2")
                                    && !node.Name.Equals("span")
                                    && !node.Name.Equals("figure")
                                    && !Regex.IsMatch(node.GetAttributeValue("class", ""), @"\s*global-incut\s*")
                                    && !Regex.IsMatch(node.GetAttributeValue("class", ""), @"\s*incut\s*")
                                    && !Regex.IsMatch(node.GetAttributeValue("class", ""), @"\s*article-widget__content\s*")
                                    && !Regex.IsMatch(node.GetAttributeValue("class", ""), @"\s*noopener\s*")
                                    && node.Attributes["style"] == null)
                    .Select(node => node.InnerText)
                    .Aggregate((i, j) => i + Environment.NewLine + j)
                    .Replace("&nbsp;", " ");
            }

            return String.Empty;
        }

        private string? GetArticleTextFromShazoo(IEnumerable<HtmlNode> htmlNodes)
        {
            if (htmlNodes.Any())
            {
                return htmlNodes.FirstOrDefault()?
                     .ChildNodes
                     .Where(node => node.Name.Equals("p")
                                     && !node.Name.Equals("span")
                                     && node.Attributes["style"] == null)
                     .Select(node => node.InnerText)
                     .Aggregate((i, j) => i + Environment.NewLine + j)
                     .Replace("&quot;", " ");
            }

            return String.Empty;
        }

        private double CompareArticleWithAfinnDictionary(List<Lemma> listLemmas, IReadOnlyDictionary<string, int> afinnDictionary)
        {
            int amountOfEvaluatedWords = 0;
            double totalTextScore = 0;


            for (int i = 0; i < listLemmas.Count - 1; i++)
            {
                foreach (var afinnItem in afinnDictionary)
                {
                    if (!string.IsNullOrEmpty(listLemmas[i].Value)
                        && afinnItem.Key == listLemmas[i].Value)
                    {
                        amountOfEvaluatedWords++;
                        totalTextScore += afinnItem.Value;

                        break;
                    }
                }
            }

            return amountOfEvaluatedWords != 0 
                ? Math.Round((totalTextScore / amountOfEvaluatedWords), 2)
                : Convert.ToDouble(_configuration["Rating:DefaultValue"]);
        }
    }
}
