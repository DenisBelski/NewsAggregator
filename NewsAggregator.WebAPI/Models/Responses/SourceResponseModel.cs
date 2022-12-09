namespace NewsAggregator.WebAPI.Models.Responses
{
    /// <summary>
    /// Response model for displaying source data.
    /// </summary>
    public class SourceResponseModel
    {
        /// <summary>
        /// Source name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Source url.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Source id.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Source Rss Url
        /// </summary>
        public string? RssUrl { get; set; }
    }
}