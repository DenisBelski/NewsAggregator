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
        public string? Title { get; set; }

        /// <summary>
        /// Article category.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Article sthort descrtiption.
        /// </summary>
        public string? ShortDescription { get; set; }

        /// <summary>
        /// Article text.
        /// </summary>
        public string? ArticleText { get; set; }
    }
}