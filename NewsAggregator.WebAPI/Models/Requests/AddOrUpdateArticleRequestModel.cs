using System.ComponentModel.DataAnnotations;

namespace NewsAggregator.WebAPI.Models.Requests
{
    /// <summary>
    /// Request model for adding or updating an article.
    /// </summary>
    public class AddOrUpdateArticleRequestModel
    {
        /// <summary>
        /// Article title.
        /// </summary>
        [Required]
        public string? Title { get; set; }

        /// <summary>
        /// Article category.
        /// </summary>
        [Required]
        public string? Category { get; set; }

        /// <summary>
        /// Article sthort descrtiption.
        /// </summary>
        [Required]
        public string? ShortDescription { get; set; }

        /// <summary>
        /// Article text.
        /// </summary>
        [Required]
        public string? ArticleText { get; set; }
    }
}