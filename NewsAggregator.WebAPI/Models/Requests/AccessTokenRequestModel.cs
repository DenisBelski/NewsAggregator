namespace NewsAggregator.WebAPI.Models.Requests
{
    /// <summary>
    /// Request model for access token.
    /// </summary>
    public class AccessTokenRequestModel
    {
        /// <summary>
        /// Access token.
        /// </summary>
        public string? AccessToken { get; set; }
    }
}