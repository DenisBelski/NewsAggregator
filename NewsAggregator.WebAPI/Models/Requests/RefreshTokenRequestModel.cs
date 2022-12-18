using System.ComponentModel.DataAnnotations;

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
        [Required]
        public Guid RefreshToken { get; set; }
    }
}