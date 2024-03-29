﻿using AutoMapper;
using HtmlAgilityPack;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NewsAggregator.Business.Models;
using NewsAggregator.Core;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.Data.Abstractions;
using NewsAggregator.Data.CQS.Queries;
using NewsAggregator.DataBase.Entities;
using Newtonsoft.Json;
using Serilog;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

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
                var result = await _unitOfWork.Commit();

                return result;
            }

            Log.Warning($"The logic in {nameof(CreateArticleAsync)} method wasn't implemented, " +
                $"check the parameter: {nameof(articleDto)}");

            return -1;
        }

        public async Task<List<ArticleDto>> GetArticlesAsync()
        {
            try
            {
                var articleEntities = await _unitOfWork.Articles.GetAllAsync();
                return _mapper.Map<List<ArticleDto>>(articleEntities);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task<ArticleDto?> GetArticleByIdAsync(Guid articleId)
        {
            try
            {
                var articleEntity = await _unitOfWork.Articles.GetByIdAsync(articleId);
                return _mapper.Map<ArticleDto>(articleEntity);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new ArgumentException(ex.Message, nameof(articleId));
            }
        }

        public async Task<List<ArticleDto>> GetArticlesByRateAsync(double? rate)
        {
            try
            {
                var listArticleEntities = await _mediator.Send(new GetArticlesByRateQuery() { Rate = rate });
                return _mapper.Map<List<ArticleDto>>(listArticleEntities);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new ArgumentException(ex.Message, nameof(rate));
            }
        }

        public async Task<List<ArticleDto>> GetArticlesByRateByPageNumberAndPageSizeAsync(double? rate, int pageNumber, int pageSize)
        {
            try
            {
                var listArticlesDto = await _unitOfWork.Articles
                    .Get()
                    .Where(article => article.Rate != null && article.Rate > rate)
                    .Skip(pageNumber * pageSize)
                    .Take(pageSize)
                    .Select(article => _mapper.Map<ArticleDto>(article))
                    .ToListAsync();

                return listArticlesDto;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new ArgumentException(ex.Message, nameof(rate));
            }
        }

        public async Task<List<ArticleDto>> GetArticlesBySourceIdAsync(Guid? sourceId)
        {
            var listArticleEntities = await _mediator.Send(new GetArticlesBySourceIdQuery() { SourceId = sourceId });

            return listArticleEntities != null
                ? _mapper.Map<List<ArticleDto>>(listArticleEntities)
                : throw new ArgumentException(null, nameof(sourceId));
        }

        public async Task<int> UpdateArticleAsync(ArticleDto articleDto)
        {
            var articleEntity = _mapper.Map<Article>(articleDto);

            if (articleEntity != null)
            {
                _unitOfWork.Articles.Update(articleEntity);
                var result = await _unitOfWork.Commit();

                return result;
            }

            Log.Warning($"The logic in {nameof(UpdateArticleAsync)} method wasn't implemented, " +
                $"check the parameter: {nameof(articleDto)}");

            return -1;
        }

        public async Task<int> UpdateOnlyOnleArticleFieldAsync(Guid articleId, List<PatchModel> patchData)
        {
            if (patchData != null)
            {
                await _unitOfWork.Articles.PatchArticleAsync(articleId, patchData);
                var result = await _unitOfWork.Commit();

                return result;
            }

            Log.Warning($"The logic in {nameof(UpdateOnlyOnleArticleFieldAsync)} method wasn't implemented, " +
                $"check one of the parameters: {nameof(articleId)}, {nameof(patchData)}");

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
                Log.Error(ex.Message);
                throw new InvalidOperationException(ex.Message);
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
                Log.Error(ex.Message);
                throw new InvalidOperationException(ex.Message);
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
                Log.Error(ex.Message);
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

                using var stream = new StreamReader(_configuration["AfinnJson:JsonPath"]);
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
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task<double> GetArticleRateByArticleTextAsync(string articleText)
        {
            using var stream = new StreamReader(_configuration["AfinnJson:JsonPath"]);
            var afinnData = stream.ReadToEnd();

            var afinnDictionary = JsonConvert.DeserializeObject<IReadOnlyDictionary<string, int>>(afinnData);

            if (afinnDictionary != null && !string.IsNullOrEmpty(articleText))
            {
                var result = await GetArticleRateByArticleTextAsync(articleText, afinnDictionary);

                return result;
            }

            Log.Error($"The logic in {nameof(GetArticleRateByArticleTextAsync)} method wasn't implemented, " +
                $"check the parameter: {nameof(articleText)}");

            throw new ArgumentException(null, nameof(articleText));
        }

        public async Task<double> GetArticleRateByArticleTextAsync(string articleText, IReadOnlyDictionary<string, int> afinnDictionary)
        {
            if (!string.IsNullOrEmpty(articleText))
            {
                using var client = new HttpClient();
                var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                    new Uri(_configuration["Ispras:Url"]));

                httpRequest.Headers.Add("Accept", "application/json");
                httpRequest.Content = JsonContent.Create(
                    new[] { new TextRequestModel() { Text = articleText } });

                var httpResponse = await client.SendAsync(httpRequest);
                var responseStream = await httpResponse.Content.ReadAsStreamAsync();

                using var stream = new StreamReader(responseStream);
                var responseData = await stream.ReadToEndAsync();
                var responseObject = JsonConvert.DeserializeObject<IsprassResponseObject[]>(responseData);

                return responseObject != null
                    ? CompareArticleWithAfinnDictionary(responseObject[0].Annotations.Lemma, afinnDictionary)
                    : Convert.ToDouble(_configuration["Rating:DefaultValue"]);
            }

            Log.Warning($"{nameof(articleText)} parametr equals null");
            throw new ArgumentException(null, nameof(articleText));
        }

        public async Task AddArticleTextToArticlesForAllAvailableSourcesAsync()
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
                Log.Error(ex.Message);
                throw new InvalidOperationException(ex.Message);
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
                Log.Error(ex.Message);
                throw new ArgumentException(ex.Message, nameof(sourceId));
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
                        articleText = GetArticleTextFromShazoo(htmlDoc.DocumentNode
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
                        $"check the parametr: {nameof(articleId)}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new InvalidOperationException(ex.Message);
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

            Log.Warning($"The logic in {nameof(GetArticleTextFromOnliner)} method wasn't implemented, " +
                $"check the parameter: {nameof(htmlNodes)}");

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
                                    && node.Attributes["style"] == null)
                    .Select(node => node.InnerText)
                    .Aggregate((i, j) => i + Environment.NewLine + j)
                    .Replace("&nbsp;", " ");
            }

            Log.Warning($"The logic in {nameof(GetArticleTextFromDevby)} method wasn't implemented, " +
                $"check the parameter: {nameof(htmlNodes)}");

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

            Log.Warning($"The logic in {nameof(GetArticleTextFromShazoo)} method wasn't implemented, " +
                $"check the parameter: {nameof(htmlNodes)}");

            return String.Empty;
        }

        private double CompareArticleWithAfinnDictionary(List<Lemma> listLemmas, IReadOnlyDictionary<string, int> afinnDictionary)
        {
            try
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
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new ArgumentException(ex.Message, nameof(listLemmas));
            }
        }
    }
}
