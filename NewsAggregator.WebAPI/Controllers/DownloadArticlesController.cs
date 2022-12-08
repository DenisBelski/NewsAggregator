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
        /// Create recurring jobs to add articles from all available sources to the storage.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(SuccessModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public IActionResult AddArticlesFromAvailableSources()
        {
            try
            {
                RecurringJob.AddOrUpdate(() => _rssService.GetArticlesDataFromAllAvailableRssSourcesAsync(), "30 */1 * * *");
                RecurringJob.AddOrUpdate(() => _articleService.AddArticleTextToArticlesForAllAvailableSourcesAsync(), "35,05 */1 * * *");
                RecurringJob.AddOrUpdate(() => _articleService.AddRateToArticlesAsync(), "40,10 */1 * * *");

                return Ok(new SuccessModel { DetailMessage = "Recurring jobs added successfully to Hangfire dashboard" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorModel { ErrorMessage = ex.Message });
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
                return StatusCode(500, new ErrorModel() { ErrorMessage = ex.Message });
            }
        }
    }
}
