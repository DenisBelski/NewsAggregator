namespace NewsAggregator.WebAPI.Models.Requests
{
    /// <summary>
    /// Request model for getting a source.
    /// </summary>
    public class GetSourceRequestModel
    {
        /// <summary>
        /// Optional field, source name. Specify a name from one of the available sources: Onliner, Devby, Shazoo, Custom.
        /// </summary>
        public string? Name { get; set; }
    }
}