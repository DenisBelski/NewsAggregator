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
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetArticleById(Guid id)
        {
            var articleDto = await _articleService.GetArticleByIdAsync(id);

            return articleDto != null 
                ? Ok(_mapper.Map<ArticleResponseModel>(articleDto)) 
                : NotFound( new ErrorModel { ErrorMessage = $"No articles found with the specified {nameof(id)}" });
        }

        /// <summary>
        /// Get all articles or get articles by rate or source id.
        /// </summary>
        /// <param name="articleModel">Contains article rating and id of the link to the article in source.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ArticleResponseModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetArticles([FromQuery] GetArticlesRequestModel? articleModel)
        {
            var listArticles = await _articleService.GetArticles();

            if (!listArticles.Any())
            {
                return NotFound();
            }

            if (articleModel != null && articleModel.Rate.HasValue)
            {
                var listArticlesWithSpecifiedRate = 
                    await _articleService.GetArticlesByRateAsync(articleModel.Rate);

                return listArticlesWithSpecifiedRate != null 
                    ? Ok(_mapper.Map<List<ArticleResponseModel>>(listArticlesWithSpecifiedRate))
                    : NotFound(new ErrorModel { ErrorMessage = $"No articles found with the specified {nameof(articleModel.Rate)}" });
            }
            else if (articleModel != null && !Guid.Empty.Equals(articleModel.SourceId))
            {
                var listArticlesWithSpecifiedSource = 
                    await _articleService.GetArticlesBySourceIdAsync(articleModel.SourceId);

                return listArticlesWithSpecifiedSource != null
                    ? Ok(_mapper.Map<List<ArticleResponseModel>>(listArticlesWithSpecifiedSource))
                    : NotFound(new ErrorModel { ErrorMessage = $"No articles found with the specified {nameof(articleModel.SourceId)}" });
            }

            return Ok(_mapper.Map<List<ArticleResponseModel>>(listArticles));
        }

        /// <summary>
        /// Create a new custom article and add it to the storage.
        /// </summary>
        /// <param name="id">Assign a unique article identifier as a <see cref="Guid"/></param>
        /// <param name="articleModel">Assign article name, category, short description and article text.</param>
        /// <returns></returns>
        [HttpPost("{id}")]
        [ProducesResponseType(typeof(ArticleResponseModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddCustomArticle(Guid id, [FromBody] AddOrUpdateArticleRequestModel? articleModel)
        {
            if (articleModel != null
                && articleModel.Title != null
                && articleModel.ArticleText != null
                && articleModel.Category != null
                && articleModel.ShortDescription != null)
            {
                var customArticle = new ArticleDto()
                {
                    Id = id,
                    PublicationDate = DateTime.Now,
                    SourceId = new Guid(_configuration["CustomSource:SourceId"]),
                    SourceUrl = _configuration["CustomSource:SourceUrl"],
                    Rate = await _articleService.GetArticleRateByArticleTextAsync(articleModel.ArticleText),
                    Title = articleModel.Title,
                    ArticleText = articleModel.ArticleText,
                    Category = articleModel.Category,
                    ShortDescription = articleModel.ShortDescription
                };

                await _articleService.CreateArticleAsync(customArticle);

                return CreatedAtAction(nameof(AddCustomArticle),
                    new { id = customArticle.Id },
                    _mapper.Map<ArticleResponseModel>(customArticle));
            }

            return BadRequest();
        }

        /// <summary>
        /// Update all fields in article with specified id.
        /// </summary>
        /// <param name="id">Contains article id.</param>
        /// <param name="articleModel">Contains article name, category, short description and article text.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ArticleResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Nullable), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateArticle(Guid id, [FromQuery] AddOrUpdateArticleRequestModel? articleModel)
        {
            if (!Guid.Empty.Equals(id))
            {
                var articleForChanges = await _articleService.GetArticleByIdAsync(id);

                if (articleForChanges == null)
                {
                    return NotFound();
                }

                if (articleModel != null
                    && articleModel.Title != null
                    && articleModel.ArticleText != null
                    && articleModel.Category != null
                    && articleModel.ShortDescription != null)
                {
                    articleForChanges = new ArticleDto()
                    {
                        Id = articleForChanges.Id,
                        PublicationDate = DateTime.Now,
                        SourceId = new Guid(_configuration["CustomSource:SourceId"]),
                        SourceUrl = _configuration["CustomSource:SourceUrl"],
                        Rate = await _articleService.GetArticleRateByArticleTextAsync(articleModel.ArticleText),
                        Title = articleModel.Title,
                        ArticleText = articleModel.ArticleText,
                        Category = articleModel.Category,
                        ShortDescription = articleModel.ShortDescription
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
        /// <param name="articleModel">Contains article name substring and source id.</param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(ArticleResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Nullable), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateArticle(Guid id, [FromBody] PatchRequestModel? articleModel)
        {
            if (!Guid.Empty.Equals(id))
            {
                var articleForChanges = await _articleService.GetArticleByIdAsync(id);

                if (articleForChanges == null)
                {
                    return NotFound();
                }

                if (articleModel != null && articleModel.Fields[0] != null && articleModel.Fields[1] != null)
                {
                    var a = articleModel.Fields;

                    //CQS?
                    //articleForChanges = await _articleService.UpdateArticleAsync(articleForChanges.Id, model.Fields);
                    return Ok(_mapper.Map<ArticleResponseModel>(articleForChanges));
                }
            }

            return BadRequest();
        }
    }
}
