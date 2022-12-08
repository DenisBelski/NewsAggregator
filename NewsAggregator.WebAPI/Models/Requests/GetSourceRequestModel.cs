namespace NewsAggregator.WebAPI.Models.Requests
{
    /// <summary>
    /// Request model for getting a source.
    /// </summary>
    public class GetSourceRequestModel
    {
        /// <summary>
        /// Source name.
        /// </summary>
        public string? Name { get; set; }
    }
}