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
    /// Controller for work with articles.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IArticleService _articleService;
        private readonly ISourceService _sourceService;
        private readonly IRssService _rssService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticlesController"/> class.
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="configuration"></param>
        /// <param name="articleService"></param>
        /// <param name="sourceService"></param>
        /// <param name="rssService"></param>
        public ArticlesController(IMapper mapper,
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
        /// Get article from the storage by id.
        /// </summary>
        /// <param name="id">An article unique identifier as a <see cref="Guid"/>.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ArticleResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Nullable), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetArticleById(Guid id)
        {
            var articleDto = await _articleService.GetArticleByIdAsync(id);

            return articleDto != null 
                ? Ok(_mapper.Map<ArticleResponseModel>(articleDto)) 
                : NotFound($"No articles found with the specified {nameof(id)}");
        }

        /// <summary>
        /// Get all articles or get articles by rate or source id.
        /// </summary>
        /// <param name="model">Contains article rating and id of the link to the article in source.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ArticleResponseModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Nullable), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetArticles([FromQuery] GetArticlesRequestModel? model)
        {
            var listArticles = await _articleService.GetArticles();

            if (!listArticles.Any())
            {
                return NoContent();
            }

            if (model != null && model.Rate.HasValue)
            {
                var listArticlesWithSpecifiedRate = 
                    await _articleService.GetArticlesByRateAsync(model.Rate);

                return listArticlesWithSpecifiedRate != null 
                    ? Ok(_mapper.Map<List<ArticleResponseModel>>(listArticlesWithSpecifiedRate))
                    : NotFound($"No articles found with the specified {nameof(model.Rate)}");
            }
            else if (model != null && !Guid.Empty.Equals(model.SourceId))
            {
                var listArticlesWithSpecifiedSource = 
                    await _articleService.GetArticlesBySourceIdAsync(model.SourceId);

                return listArticlesWithSpecifiedSource != null
                    ? Ok(_mapper.Map<List<ArticleResponseModel>>(listArticlesWithSpecifiedSource))
                    : NotFound($"No articles found with the specified {nameof(model.SourceId)}");
            }

            return Ok(_mapper.Map<List<ArticleResponseModel>>(listArticles));
        }

        /// <summary>
        /// Update all fields in article with specified id.
        /// </summary>
        /// <param name="id">Contains article id.</param>
        /// <param name="model">Contains article name, category, short description and article text.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ArticleResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Nullable), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateArticle(Guid id, [FromQuery] AddOrUpdateArticleRequestModel? model)
        {
            if (!Guid.Empty.Equals(id))
            {
                var articleForChanges = await _articleService.GetArticleByIdAsync(id);

                if (articleForChanges == null)
                {
                    return NotFound();
                }

                if (model != null
                    && model.Title != null
                    && model.Text != null
                    && model.Category != null
                    && model.ShortDescrtiption != null)
                {
                    articleForChanges = new ArticleDto()
                    {
                        Id = articleForChanges.Id,
                        PublicationDate = DateTime.Now,
                        SourceId = new Guid(_configuration["CustomSource:SourceId"]),
                        SourceUrl = _configuration["CustomSource:SourceUrl"],
                        Rate = await _articleService.GetArticleRateByArticleTextAsync(model.Text),
                        Title = model.Title,
                        ArticleText = model.Text,
                        Category = model.Category,
                        ShortDescription = model.ShortDescrtiption
                    };
                }
                else
                {
                    articleForChanges = new ArticleDto()
                    {
                        Id = articleForChanges.Id,
                        PublicationDate = DateTime.Now,
                        SourceId = new Guid(_configuration["CustomSource:SourceId"]),
                        SourceUrl = _configuration["CustomSource:SourceUrl"],
                        Rate = null,
                        Title = null,
                        ArticleText = null,
                        Category = null,
                        ShortDescription = null
                    };
                }

                await _articleService.UpdateArticleAsync(articleForChanges);
                return Ok(_mapper.Map<ArticleResponseModel>(articleForChanges));
            }

            return BadRequest();
        }

        /// <summary>
        /// Update only one field in article with specified id.
        /// </summary>
        /// <param name="id">Contains article id.</param>
        /// <param name="model">Contains article name substring and source id.</param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(ArticleResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Nullable), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateArticle(Guid id, [FromBody] PatchRequestModel? model)
        {
            if (!Guid.Empty.Equals(id))
            {
                var articleForChanges = await _articleService.GetArticleByIdAsync(id);

                if (articleForChanges == null)
                {
                    return NotFound();
                }

                if (model != null && model.Fields[0] != null && model.Fields[1] != null)
                {
                    var a = model.Fields;

                    //CQS?
                    //articleForChanges = await _articleService.UpdateArticleAsync(articleForChanges.Id, model.Fields);
                    return Ok(_mapper.Map<ArticleResponseModel>(articleForChanges));
                }
            }

            return BadRequest();
        }
    }
}
