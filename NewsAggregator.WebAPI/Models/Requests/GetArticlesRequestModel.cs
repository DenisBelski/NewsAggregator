namespace NewsAggregator.WebAPI.Models.Requests
{
    /// <summary>
    /// Request model for getting an article.
    /// </summary>
    public class GetArticlesRequestModel
    {
        /// <summary>
        /// Optional field, source id.
        /// </summary>
        public Guid? SourceId { get; set; }

        /// <summary>
        /// Optional field, available rating for showing articles.
        /// </summary>
        public double? Rate { get; set; }
    }
}