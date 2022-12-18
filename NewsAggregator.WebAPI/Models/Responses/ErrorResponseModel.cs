namespace NewsAggregator.WebAPI.Models.Responses
{
    /// <summary>
    /// Model for returning errors from api.
    /// </summary>
    public class ErrorResponseModel
    {
        /// <summary>
        /// Error message.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}