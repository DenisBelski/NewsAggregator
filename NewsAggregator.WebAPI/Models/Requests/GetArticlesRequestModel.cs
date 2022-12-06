namespace NewsAggregator.WebAPI.Models.Requests
{
    /// <summary>
    /// This class contains fields for "get article request model".
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