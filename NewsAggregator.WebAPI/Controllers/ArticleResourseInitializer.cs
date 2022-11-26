using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.WebAPI.Models.Requests;
using NewsAggregator.WebAPI.Models.Responses;

namespace NewsAggregator.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleResourseInitializer : ControllerBase
    {

        private readonly IArticleService _articleService;
        private readonly ISourceService _sourceService;
        private readonly IRssService _rssService;
        private readonly IMapper _mapper;

        public ArticleResourseInitializer(IArticleService articleService,
            ISourceService sourceService,
            IRssService rssService,
            IMapper mapper)
        {
            _articleService = articleService;
            _sourceService = sourceService;
            _rssService = rssService;
            _mapper = mapper;
        }

        /// <summary>
        /// Add Articles
        /// </summary>
        /// <param name="model">Contains Add or update article request model</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddArticles([FromBody] AddOrUpdateArticleRequestModel? model)
        {
            //if (model != null)
            //{
            //    var dto = new ArticleDto()
            //    {
            //        Id = Guid.NewGuid(),
            //        ArticleText = "Some article text",
            //        Category = "Some category",
            //        ShortDescription = "Some description",
            //        Title = "New Title",
            //        PublicationDate = DateTime.Now,
            //    };

            //    await _articleService.CreateArticleAsync(dto);

            //    //return Ok(dto);
            //    return CreatedAtAction(nameof(GetArticleById), new { id = dto.Id }, dto);
            //}

            //return BadRequest();


            try
            {
                var sources = await _sourceService.GetSourcesAsync();

                foreach (var source in sources)
                {
                    await _rssService.GetAllArticleDataFromRssAsync();
                    await _articleService.AddArticleTextToArticlesAsync();
                }

                return Ok();
            }
            catch (Exception ex)
            {

                return StatusCode(500, new ErrorModel { Message = ex.Message });
            }
        }


        //[HttpPost]
        //public async Task<IActionResult> AddArticles()
        //{
        //    try
        //    {
        //        //RecurringJob.AddOrUpdate(() => _articleService.AggregateArticlesFromExternalSourcesAsync(),
        //        //    "5,10,35 10-18 * * Mon-Fri");

        //        //RecurringJob.RemoveIfExists(nameof(_articleService.AggregateArticlesFromExternalSourcesAsync));

        //        //RecurringJob.AddOrUpdate(()=>_articleService.GetAllArticleDataFromRssAsync(),
        //        //    "*/15 * * * *");
        //        //RecurringJob.AddOrUpdate(()=>_articleService.AddArticleTextToArticlesAsync(),
        //        //    "*/30 * * * *");

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new ErrorModel() { Message = ex.Message });
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> RateArticles()
        {
            try
            {
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
