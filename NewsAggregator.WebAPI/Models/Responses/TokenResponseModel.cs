namespace NewsAggregator.WebAPI.Models.Responses
{
    /// <summary>
    /// Response model for returning token data.
    /// </summary>
    public class TokenResponseModel
    {
        /// <summary>
        /// Access token.
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Role.
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// User id.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Token expiration.
        /// </summary>
        public DateTime TokenExpiration { get; set; }

        /// <summary>
        /// Refresh token.
        /// </summary>
        public Guid RefreshToken { get; set; }
    }
}