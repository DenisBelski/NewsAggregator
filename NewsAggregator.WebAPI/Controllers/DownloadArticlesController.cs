using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.WebAPI.Models.Requests;
using NewsAggregator.WebAPI.Models.Responses;
using Serilog;

namespace NewsAggregator.WebAPI.Controllers
{
    /// <summary>
    /// Controller for getting articles from available sources.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;
        private readonly IRssService _rssService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadArticlesController"/> class.
        /// </summary>
        /// <param name="articleService"></param>
        /// <param name="rssService"></param>
        public DownloadArticlesController(IArticleService articleService,
            IRssService rssService)
        {
            _articleService = articleService;
            _rssService = rssService;
        }

        /// <summary>
        /// Create recurring jobs to add articles from all available sources to the storage.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(SuccessModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public IActionResult AddArticlesFromAllAvailableSourcesToTheStorage()
        {
            try
            {
                RecurringJob.AddOrUpdate(()
                    => _rssService.GetArticlesDataFromAllAvailableRssSourcesAsync(),
                    "30 */1 * * *");

                RecurringJob.AddOrUpdate(()
                    => _articleService.AddArticleTextToArticlesForAllAvailableSourcesAsync(),
                    "35,05 */1 * * *");

                RecurringJob.AddOrUpdate(()
                    => _articleService.AddRateToArticlesAsync(),
                    "40,10 */1 * * *");

                return Ok(new SuccessModel
                {
                    DetailMessage = "Recurring jobs added successfully to Hangfire dashboard"
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, new ErrorModel
                {
                    ErrorMessage = "The server encountered an unexpected situation."
                });
            }
        }
    }
}
