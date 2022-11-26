﻿using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.WebAPI.Models.Requests;

namespace NewsAggregator.WebAPI.Controllers
{
    /// <summary>
    /// Controller for work with articles
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private static List<ArticleDto> Articles = new List<ArticleDto>()
        {
            new ArticleDto()
            {
                Id = Guid.NewGuid(),
                ArticleText = "Some text 1",
                Title = "Article #1",
                Category = "Some category 1"
            },
            new ArticleDto()
            {
                Id = Guid.NewGuid(),
                ArticleText = "Some text 2",
                Title = "Article #2",
                Category = "Some category 2"
            }
        };




        /// <summary>
        /// Get article from storage with specified id
        /// </summary>
        /// <param name="id">Id of article</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Nullable), StatusCodes.Status404NotFound)]
        public IActionResult GetArticleById(Guid id)
        {

            var article = Articles.FirstOrDefault(dto => dto.Id.Equals(id));
            if (article == null)
            {
                return NotFound();
            }
            return Ok(article);

        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetArticles([FromQuery] GetArticlesRequestModel? model)
        {
            IEnumerable<ArticleDto> articles = Articles;


            return Ok(articles.ToList());
        }




        [HttpPost]
        public IActionResult AddArticles([FromBody] AddOrUpdateArticleRequestModel? model)
        {

            if (model != null)
            {
                var dto = new ArticleDto()
                {
                    Id = Guid.NewGuid(),
                    ArticleText = "Some text added manually",
                    Category = "Some manually category",
                    ShortDescription = "",
                    Title = "New Article ",
                    PublicationDate = DateTime.Now
                };

                Articles.Add(dto);

                return CreatedAtAction(nameof(GetArticleById), new { id = dto.Id }, dto);
            }

            return BadRequest();
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public IActionResult UpdateArticles(Guid id, [FromBody] AddOrUpdateArticleRequestModel? model)
        {
            if (model != null)
            {
                var oldValue = Articles.FirstOrDefault(dto => dto.Id.Equals(id));

                if (oldValue == null)
                {
                    return NotFound();
                }

                var newValue = new ArticleDto()
                {
                    Id = oldValue.Id,
                    PublicationDate = DateTime.Now,
                    Title = model.Title,
                    ArticleText = model.Text,
                    Category = model.Category,
                    ShortDescription = model.ShortSummary
                };

                Articles.Remove(oldValue);
                Articles.Add(newValue);

                return Ok();
            }

            return BadRequest();
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        public IActionResult UpdateArticles(Guid id, [FromBody] PatchRequestModel? model)
        {
            if (model != null)
            {
                var oldValue = Articles.FirstOrDefault(dto => dto.Id.Equals(id));

                if (oldValue == null)
                {
                    return NotFound();
                }

                //todo add patch implementation(change only fields from request

                return Ok();
            }

            return BadRequest();
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public IActionResult UpdateArticles(Guid id)
        {
            var oldValue = Articles.FirstOrDefault(dto => dto.Id.Equals(id));

            if (oldValue == null)
            {
                return NotFound();
            }

            Articles.Remove(oldValue);

            return Ok();
        }
    }
}
