namespace NewsAggregator.WebAPI.Models.Requests
{
    /// <summary>
    /// Request model for getting an article.
    /// </summary>
    public class GetArticlesRequestModel
    {
        /// <summary>
        /// Source id.
        /// </summary>
        public Guid? SourceId { get; set; }

        /// <summary>
        /// Available rating for showing articles.
        /// </summary>
        public double? Rate { get; set; }
    }
}