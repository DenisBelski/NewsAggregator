using AutoMapper;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NewsAggregator.Business.Models;
using NewsAggregator.Core;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.Data.Abstractions;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;
using Newtonsoft.Json;
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

        public ArticleService(IMapper mapper,
            IConfiguration configuration, 
            IUnitOfWork unitOfWork,
            IRssService rssService)
        {
            _mapper = mapper;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _rssService = rssService;
        }




        public async Task<ArticleDto> GetArticleByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.Articles.GetByIdAsync(id);
            var dto = _mapper.Map<ArticleDto>(entity);

            return dto;
        }




        public async Task<int> CreateArticleAsync(ArticleDto dto)
        {
            var entity = _mapper.Map<Article>(dto);

            if (entity != null)
            {
                await _unitOfWork.Articles.AddAsync(entity);
                var addingResult = await _unitOfWork.Commit();
                return addingResult;
            }
            else
            {
                throw new ArgumentException(nameof(dto));
            }
        }




        public async Task<int> UpdateArticleAsync(Guid id, ArticleDto? dto)
        {
            var sourceDto = await GetArticleByIdAsync(id);

            //should be sure that dto property naming is the same with entity property naming
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

            //await _unitOfWork.Articles.PatchAsync(id, patchList);
            return await _unitOfWork.Commit();
        }




        public async Task<List<ArticleDto>> GetArticlesByPageNumberAndPageSizeAsync(int pageNumber, int pageSize)
        {
            try
            {
                var list = await _unitOfWork.Articles
                    .Get()
                    .Skip(pageNumber * pageSize)
                    .Take(pageSize)
                    .Select(article => _mapper.Map<ArticleDto>(article))
                    .ToListAsync();

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }




        // for WebAPI
        public async Task<List<ArticleDto>> GetArticlesByNameAndSourcesAsync(string? name, Guid? sourceId)
        {
            //var list = new List<ArticleDto>();

            var entities = _unitOfWork.Articles.Get();

            if (!string.IsNullOrEmpty(name))
            {
                entities = entities.Where(dto => dto.Title.Contains(name));
            }

            if (sourceId != null && !Guid.Empty.Equals(sourceId))
            {
                entities = entities.Where(dto => dto.SourceId.Equals(sourceId));
            }

            var result = (await entities.ToListAsync())
                .Select(entity => _mapper.Map<ArticleDto>(entity))
                .ToList();

            return result;
        }




        // for WebAPI
        public async Task DeleteArticleAsync(Guid id)
        {

            var entity = await _unitOfWork.Articles.GetByIdAsync(id);

            if (entity != null)
            {
                _unitOfWork.Articles.Remove(entity);

                await _unitOfWork.Commit();
            }
            else
            {
                throw new ArgumentException("Article for removing doesn't exist", nameof(id));
            }
        }




        public async Task AggregateArticlesFromExternalSourcesAsync()
        {
            var sources = await _unitOfWork.Sources.GetAllAsync();

            foreach (var source in sources)
            {
                await _rssService.GetAllArticleDataFromOnlinerRssAsync(source.Id, source.RssUrl);
                await AddArticleTextToArticlesAsync();
            }
        }




        public async Task AddRateToArticlesAsync()
        {
            //get data where Rate == null, and article include Article text
            var articlesWithEmptyRateIds = _unitOfWork.Articles.Get()
                .Where(article => article.Rate == null && !string.IsNullOrEmpty(article.ArticleText))
                .Select(article => article.Id)
                .ToList();

            foreach (var articleId in articlesWithEmptyRateIds)
            {
                await RateArticleAsync(articleId);
            }
        }




        public async Task AddArticleTextToArticlesAsync()
        {
            var articlesWithEmptyTextIds = _unitOfWork.Articles
                .Get()
                .Where(article => string.IsNullOrEmpty(article.ArticleText))
                .Select(article => article.Id)
                .ToList();

            foreach (var articleId in articlesWithEmptyTextIds)
            {
                await AddArticleTextToArticleAsync(articleId);
            }

            //    var articlesWithEmptyTextIds = await _mediator
            //        .Send(new GetAllArticlesWithoutTextIdsQuery());
            //    if (articlesWithEmptyTextIds != null)
            //    {
            //        foreach (var articleId in articlesWithEmptyTextIds)
            //        {
            //            await AddArticleTextToArticleAsync(articleId);
            //        }
            //    }
        }




        //Naming example - "AddArticleTextToOnlinerArticleAsync", will be unique for every source
        private async Task AddArticleTextToArticleAsync(Guid articleId)
        {
            try
            {
                var article = await _unitOfWork.Articles.GetByIdAsync(articleId);
                //var article = await _mediator.Send(new GetArticleByIdQuery { Id = articleId });

                if (article == null)
                {
                    throw new ArgumentException($"Article with id: {articleId} doesn't exists",
                        nameof(articleId));
                }

                var articleSourceUrl = article.SourceUrl;
                var web = new HtmlWeb();
                var htmlDoc = web.Load(articleSourceUrl);

                // get class name "news-text" from web page with news
                var nodes = htmlDoc.DocumentNode.Descendants(0).Where(n => n.HasClass("news-text"));

                if (nodes.Any())
                {
                    var articleText = nodes.FirstOrDefault()?
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
                                        && !node.HasClass("news-entry__speech") //??
                                        && !node.HasClass("news-entry") //??
                                        && !node.HasClass("news-header")
                                        && !node.HasClass("news-media")
                                        && !node.HasClass("news-media__viewport")
                                        && !node.HasClass("news-media__preview")
                                        && !node.HasClass("news-media__inside")
                                        && !node.HasClass("alignnone")
                                        && node.Attributes["style"] == null)
                        .Select(node => node.InnerText)      // or => node.InnerText node.OuterHtml/InnerHtml
                        .Aggregate((i, j) => i + Environment.NewLine + j);

                    await _unitOfWork.Articles.UpdateArticleTextAsync(articleId, articleText);
                    await _unitOfWork.Commit();
                    //await _mediator.Send(new UpdateArticleTextCommand() { Id = articleId, Text = articleText });
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        //private async Task AddArticleTextToArticleAsync(Guid articleId)
        //{
        //    try
        //    {
        //        var article = await _mediator.Send(new GetArticleByIdQuery { Id = articleId });
        //        if (article == null)
        //        {
        //            throw new ArgumentException($"Article with id: {articleId} doesn't exists",
        //                nameof(articleId));
        //        }
        //        var articleSourceUrl = article.SourceUrl;
        //        var web = new HtmlWeb();
        //        var htmlDoc = web.Load(articleSourceUrl);
        //        var nodes =
        //            htmlDoc.DocumentNode.Descendants(0)
        //                .Where(n => n.HasClass("news-text"));
        //        if (nodes.Any())
        //        {
        //            var articleText = nodes.FirstOrDefault()?
        //                .ChildNodes
        //                .Where(node => (node.Name.Equals("p") || node.Name.Equals("div") || node.Name.Equals("h2"))
        //                               && !node.HasClass("news-reference")
        //                               && !node.HasClass("news-banner")
        //                               && !node.HasClass("news-widget")
        //                               && !node.HasClass("news-vote")
        //                               && node.Attributes["style"] == null)
        //                .Select(node => node.OuterHtml)
        //                .Aggregate((i, j) => i + Environment.NewLine + j);
        //            await _mediator.Send(new UpdateArticleTextCommand() { Id = articleId, Text = articleText });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}




        private async Task RateArticleAsync(Guid articleId)
        {
            try
            {
                var article = await _unitOfWork.Articles.GetByIdAsync(articleId);

                if (article == null)
                {
                    throw new ArgumentException($"Article with id: {articleId} doesn't exists",
                        nameof(articleId));
                }



                using (var client = new HttpClient())
                {
                    var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                        new Uri(@"http://api.ispras.ru/texterra/v1/nlp?targetType=lemma&apikey=1e03d79d9f01859fefbc04abe0a9c3f3660e117f"));
                    
                    httpRequest.Headers.Add("Accept", "application/json");
                    httpRequest.Content = JsonContent.Create(
                        new[] { new TextRequestModel() { Text = article.ArticleText } });

                    var httpResponse = await client.SendAsync(httpRequest);
                    var responseStream = await httpResponse.Content.ReadAsStreamAsync();

                    //get lemma from words. Then compare with AFINN-ru.json file, and get average value (Rating)
                    using (var stream = new StreamReader(responseStream))
                    {
                        var data = await stream.ReadToEndAsync();

                        var responseObject = JsonConvert.DeserializeObject<IsprassResponseObject[]>(data);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
