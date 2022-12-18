using System.ComponentModel.DataAnnotations;

namespace NewsAggregator.WebAPI.Models.Requests
{
    /// <summary>
    /// Request model for user login.
    /// </summary>
    public class LoginUserRequestModel
    {
        /// <summary>
        /// User email.
        /// </summary>
        [Required]
        public string? Email { get; set; }

        /// <summary>
        /// User password.
        /// </summary>
        [Required]
        public string? Password { get; set; }
    }
}