namespace NewsAggregator.WebAPI.Models.Requests
{
    /// <summary>
    /// This class contains fields for "add or update article request model".
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