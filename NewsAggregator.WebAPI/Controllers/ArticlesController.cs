﻿using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.WebAPI.Models.Requests;
using NewsAggregator.WebAPI.Models.Responses;

namespace NewsAggregator.WebAPI.Controllers
{
    /// <summary>
    /// Controller for work with articles
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;
        private readonly ISourceService _sourceService;
        private readonly IRssService _rssService;
        private readonly IMapper _mapper;

        public ArticlesController(IArticleService articleService,
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
        /// Get article from storage with specified id
        /// </summary>
        /// <param name="id">Id of article</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Nullable), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetArticleById(Guid id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);

            if (article == null)
            {
                return NotFound();
            }

            return Ok(article);
        }

        /// <summary>
        /// Get articles by article name substring and source id
        /// </summary>
        /// <param name="model">Contains article name substring and source id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetArticles([FromQuery] GetArticlesRequestModel? model)
        {
            IEnumerable<ArticleDto> articles = await _articleService
                .GetArticlesByNameAndSourcesAsync(model?.Name, model?.SourceId);

            return Ok(articles.ToList());
            //return Ok();
        }

        /// <summary>
        /// Update all fields in article from storage with specified id
        /// </summary>
        /// <param name="id">Id of article</param>
        /// <param name="model">Contains article name substring and source id</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public IActionResult UpdateArticles(Guid id, [FromBody] AddOrUpdateArticleRequestModel? model)
        {
            if (model != null)
            {
                //var oldValue = Articles.FirstOrDefault(dto => dto.Id.Equals(id));

                //if (oldValue == null)
                //{
                //    return NotFound();
                //}

                //var newValue = new ArticleDto()
                //{
                //    Id = oldValue.Id,
                //    PublicationDate = DateTime.Now,
                //    Title = model.Title,
                //    ArticleText = model.Text,
                //    Category = model.Category,
                //    ShortDescription = model.ShortSummary
                //};

                //Articles.Remove(oldValue);
                //Articles.Add(newValue);

                return Ok();
            }

            return BadRequest();
        }


        /// <summary>
        /// Update only some of the fields in the article from storage with specified id
        /// </summary>
        /// <param name="id">Id of article</param>
        /// <param name="model">Contains article name substring and source id</param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        public IActionResult UpdateArticles(Guid id, [FromBody] PatchRequestModel? model)
        {
            //if (model != null)
            //{
            //    var oldValue = Articles.FirstOrDefault(dto => dto.Id.Equals(id));

            //    if (oldValue == null)
            //    {
            //        return NotFound();
            //    }

            //    //!!!!!! todo add patch implementation(change only fields from request)

            //    return Ok();
            //}

            return BadRequest();
        }

        /// <summary>
        /// Get article from storage with specified id
        /// </summary>
        /// <param name="id">Id of article</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Nullable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteArticle(Guid id)
        {
            // Simple Example
            //    var oldValue = Articles.FirstOrDefault(dto => dto.Id.Equals(id));
            //    if (oldValue == null)
            //    {
            //        return NotFound();
            //    }
            //    Articles.Remove(oldValue);
            //    return Ok();

            try
            {
                await _articleService.DeleteArticleAsync(id);

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorModel { Message = ex.Message });
            }
        }
    }
}
