namespace NewsAggregator.WebAPI.Models.Responses
{
    /// <summary>
    /// Response model for displaying article data.
    /// </summary>
    public class ArticleResponseModel
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
        /// Article short description.
        /// </summary>
        public string? ShortDescription { get; set; }

        /// <summary>
        /// Article text.
        /// </summary>
        public string? ArticleText { get; set; }

        /// <summary>
        /// Article publication date.
        /// </summary>
        public DateTime PublicationDate { get; set; }

        /// <summary>
        /// Source id.
        /// </summary>
        public Guid SourceId { get; set; }
    }
}