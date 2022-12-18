namespace NewsAggregator.WebAPI.Models.Responses
{
    /// <summary>
    /// Model for returning success messages from api.
    /// </summary>
    public class SuccessResponseModel
    {
        /// <summary>
        /// Success detail message.
        /// </summary>
        public string? DetailMessage { get; set; }
    }
}