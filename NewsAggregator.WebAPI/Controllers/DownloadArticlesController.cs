using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.WebAPI.Models.Requests;
using NewsAggregator.WebAPI.Models.Responses;

namespace NewsAggregator.WebAPI.Controllers
{
    /// <summary>
    /// Controller for getting articles from resources.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadArticlesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IArticleService _articleService;
        private readonly ISourceService _sourceService;
        private readonly IRssService _rssService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadArticlesController"/> class.
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="configuration"></param>
        /// <param name="articleService"></param>
        /// <param name="sourceService"></param>
        /// <param name="rssService"></param>
        public DownloadArticlesController(IMapper mapper,
            IConfiguration configuration,
            IArticleService articleService,
            ISourceService sourceService,
            IRssService rssService)
        {
            _mapper = mapper;
            _configuration = configuration;
            _articleService = articleService;
            _sourceService = sourceService;
            _rssService = rssService;
        }

        /// <summary>
        /// Add a new custom article to the storage.
        /// </summary>
        /// <param name="id">An article unique identifier as a <see cref="Guid"/></param>
        /// <param name="model">Contains article name, category, short description and article text.</param>
        /// <returns></returns>
        [HttpPost("{id}")]
        public async Task<IActionResult> AddCustomArticle(Guid id, [FromBody] AddOrUpdateArticleRequestModel? model)
        {
            if (model != null
                && model.Title != null
                && model.Text != null
                && model.Category != null
                && model.ShortDescrtiption != null)
            {
                var customArticle = new ArticleDto()
                {
                    Id = Guid.NewGuid(),
                    PublicationDate = DateTime.Now,
                    SourceId = new Guid(_configuration["CustomSource:SourceId"]),
                    SourceUrl = _configuration["CustomSource:SourceUrl"],
                    Rate = await _articleService.GetArticleRateByArticleTextAsync(model.Text),
                    Title = model.Title,
                    ArticleText = model.Text,
                    Category = model.Category,
                    ShortDescription = model.ShortDescrtiption
                };

                await _articleService.CreateArticleAsync(customArticle);


                return CreatedAtAction(nameof(AddCustomArticle), 
                    new { id = customArticle.Id }, 
                    _mapper.Map<ArticleResponseModel>(customArticle));
            }

            return BadRequest();
        }

        /// <summary>
        /// Add articles from available sources to the storage.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddArticlesFromAvailableSources()
        {
            try
            {
                //var sources = await _sourceService.GetSourcesAsync();

                //foreach (var source in sources)
                //{
                //    await _rssService.GetAllArticleDataFromRssAsync();
                //    await _articleService.AddArticleTextToArticlesFromOnlinerAsync();
                //}


                //RecurringJob.AddOrUpdate(() => _articleService.AggregateArticlesFromExternalSourcesAsync(), "5,10,35 10-18 * * Mon-Fri");

                //Remove created jobs
                //RecurringJob.RemoveIfExists(nameof(_articleService.AggregateArticlesFromExternalSourcesAsync));

                RecurringJob.RemoveIfExists(nameof(_articleService.AggregateArticlesFromAllAvailableSourcesAsync));
                RecurringJob.AddOrUpdate(() => _rssService.GetArticlesDataFromAllAvailableRssSourcesAsync(), "15 */12 * * *");


                return Ok();
            }
            catch (Exception ex)
            {

                return StatusCode(500, new ErrorModel { Message = ex.Message });
            }
        }

        /// <summary>
        /// Add rating articles to the storage.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> AddRateToArticles()
        {
            try
            {
                // Do for rate article separate RecurringJob
                // Think algorithm "take no more than 10 data at a time, for minute"
                //await _articleService.AddRateToArticlesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorModel() { Message = ex.Message });
            }
        }
    }
}
