namespace NewsAggregator.WebAPI.Models.Responses
{
    /// <summary>
    /// This class contains fields for "source response model".
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
    }
}