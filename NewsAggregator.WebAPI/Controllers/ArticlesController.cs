using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.WebAPI.Models.Requests;
using NewsAggregator.WebAPI.Models.Responses;
using Serilog;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticlesController"/> class.
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="configuration"></param>
        /// <param name="articleService"></param>
        public ArticlesController(IMapper mapper,
            IConfiguration configuration,
            IArticleService articleService)
        {
            _mapper = mapper;
            _configuration = configuration;
            _articleService = articleService;
        }

        /// <summary>
        /// Get article from the storage by id.
        /// </summary>
        /// <param name="id">An article unique identifier as a <see cref="Guid"/>.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ArticleResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetArticleById(Guid id)
        {
            try
            {
                var articleDto = await _articleService.GetArticleByIdAsync(id);

                return articleDto != null
                    ? Ok(_mapper.Map<ArticleResponseModel>(articleDto))
                    : NotFound(new ErrorResponseModel
                    {
                        ErrorMessage = $"No articles found with the specified {nameof(id)}"
                    });
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return BadRequest(new ErrorResponseModel { ErrorMessage = "Failed, please check your input." });
            }
        }

        /// <summary>
        /// Get all articles or get articles by rate or source id.
        /// </summary>
        /// <param name="articleModel">Assign articles a minimum rating to display or specify source id.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ArticleResponseModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetArticles([FromQuery] GetArticlesRequestModel articleModel)
        {
            try
            {
                var listArticles = await _articleService.GetArticlesAsync();

                if (!listArticles.Any())
                {
                    return NotFound(new ErrorResponseModel { ErrorMessage = "No articles found in the storage" });
                }

                if (articleModel.Rate.HasValue)
                {
                    var listArticlesWithSpecifiedRate =
                        await _articleService.GetArticlesByRateAsync(articleModel.Rate);


                    return listArticlesWithSpecifiedRate.Any()
                        ? Ok(_mapper.Map<List<ArticleResponseModel>>(listArticlesWithSpecifiedRate))
                        : NotFound(new ErrorResponseModel
                        {
                            ErrorMessage = $"No articles found with the specified {nameof(articleModel.Rate)}."
                        });
                }
                else if (articleModel.SourceId != null)
                {
                    var listArticlesWithSpecifiedSource =
                        await _articleService.GetArticlesBySourceIdAsync(articleModel.SourceId);

                    return listArticlesWithSpecifiedSource.Any()
                        ? Ok(_mapper.Map<List<ArticleResponseModel>>(listArticlesWithSpecifiedSource))
                        : NotFound(new ErrorResponseModel
                        {
                            ErrorMessage = $"No articles found with the specified {nameof(articleModel.SourceId)}."
                        });
                }

                return Ok(_mapper.Map<List<ArticleResponseModel>>(listArticles));
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500, new ErrorResponseModel
                {
                    ErrorMessage = "The server encountered an unexpected situation."
                });
            }
        }

        /// <summary>
        /// Create a new custom article and add it to the storage.
        /// </summary>
        /// <param name="id">Assign a unique article identifier as a <see cref="Guid"/></param>
        /// <param name="articleModel">Assign article name, category, short description and article text.</param>
        /// <returns></returns>
        [HttpPost("{id}")]
        [ProducesResponseType(typeof(ArticleResponseModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCustomArticle(Guid id, [FromQuery] AddOrUpdateArticleRequestModel articleModel)
        {
            try
            {
                if (!string.IsNullOrEmpty(articleModel.Title)
                    && !string.IsNullOrEmpty(articleModel.ArticleText)
                    && !string.IsNullOrEmpty(articleModel.Category)
                    && !string.IsNullOrEmpty(articleModel.ShortDescription))
                {
                    var customArticle = new ArticleDto()
                    {
                        Id = id,
                        PublicationDate = DateTime.UtcNow,
                        SourceId = new Guid(_configuration["CustomSource:SourceId"]),
                        SourceUrl = _configuration["CustomSource:SourceUrl"],
                        Rate = await _articleService.GetArticleRateByArticleTextAsync(articleModel.ArticleText),
                        Title = articleModel.Title,
                        ArticleText = articleModel.ArticleText,
                        Category = articleModel.Category,
                        ShortDescription = articleModel.ShortDescription
                    };

                    await _articleService.CreateArticleAsync(customArticle);

                    return CreatedAtAction(nameof(CreateCustomArticle),
                        new { id = customArticle.Id },
                        _mapper.Map<ArticleResponseModel>(customArticle));
                }

                return BadRequest(new ErrorResponseModel
                {
                    ErrorMessage = "Failed to create article, please check your input"
                });
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500, new ErrorResponseModel
                {
                    ErrorMessage = "The server encountered an unexpected situation."
                });
            }
        }

        /// <summary>
        /// Update all fields in article with specified id.
        /// </summary>
        /// <param name="id">Specify a unique article identifier as a <see cref="Guid"/>.</param>
        /// <param name="articleModel">Optionally, specify article name, category, short description and article text.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ArticleResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateArticle(Guid id, [FromQuery] AddOrUpdateArticleRequestModel articleModel)
        {
            try
            {
                var articleDto = await _articleService.GetArticleByIdAsync(id);

                if (articleDto == null)
                {
                    return NotFound(new ErrorResponseModel
                    {
                        ErrorMessage = $"No articles found with the specified {nameof(id)}"
                    });
                }

                if (!string.IsNullOrEmpty(articleModel.Title)
                    && !string.IsNullOrEmpty(articleModel.ArticleText)
                    && !string.IsNullOrEmpty(articleModel.Category)
                    && !string.IsNullOrEmpty(articleModel.ShortDescription))
                {
                    articleDto = new ArticleDto()
                    {
                        Id = articleDto.Id,
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
                    articleDto = new ArticleDto()
                    {
                        Id = articleDto.Id,
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

                await _articleService.UpdateArticleAsync(articleDto);
                return Ok(_mapper.Map<ArticleResponseModel>(articleDto));
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500, new ErrorResponseModel
                {
                    ErrorMessage = "The server encountered an unexpected situation."
                });
            }
        }

        /// <summary>
        /// Update only necessary field in article with specified id.
        /// </summary>
        /// <param name="id">Specify a unique article identifier as a <see cref="Guid"/>.</param>
        /// <param name="patchRequestModel">Specify the name of the field and its values to change the article.</param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(ArticleResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateArticle(Guid id, [FromQuery] PatchRequestModel patchRequestModel)
        {
            try
            {
                var articleForChanges = await _articleService.GetArticleByIdAsync(id);

                if (articleForChanges == null)
                {
                    return NotFound(new ErrorResponseModel
                    {
                        ErrorMessage = $"No articles found with the specified {nameof(id)}."
                    });
                }

                var result = 0;

                if (!string.IsNullOrEmpty(patchRequestModel.FieldName)
                    && !string.IsNullOrEmpty(patchRequestModel.FieldValue))
                {
                    var patchList = new List<PatchModel>()
                    {
                        new PatchModel()
                        {
                            PropertyName = patchRequestModel.FieldName,
                            PropertyValue = patchRequestModel.FieldValue
                        }
                    };

                    result = await _articleService.UpdateOnlyOnleArticleFieldAsync(articleForChanges.Id, patchList);
                }

                return result > 0
                    ? Ok(new SuccessResponseModel
                    {
                        DetailMessage = $"Article with specified {nameof(id)} successfully modified."
                    })
                    : BadRequest(new ErrorResponseModel
                    {
                        ErrorMessage = "Failed to update article, please check your input."
                    });
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500, new ErrorResponseModel
                {
                    ErrorMessage = "The server encountered an unexpected situation."
                });
            }
        }
    }
}
