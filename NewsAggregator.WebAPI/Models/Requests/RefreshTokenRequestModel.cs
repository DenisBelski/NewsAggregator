namespace NewsAggregator.WebAPI.Models.Requests
{
    /// <summary>
    /// Request model for refresh token.
    /// </summary>
    public class RefreshTokenRequestModel
    {
        /// <summary>
        /// Refresh token.
        /// </summary>
        public Guid RefreshToken { get; set; }
    }
}