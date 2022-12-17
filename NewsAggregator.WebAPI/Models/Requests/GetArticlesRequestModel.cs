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
        /// Recommended values: from "-2" to "2"
        /// </summary>
        public double? Rate { get; set; }
    }
}