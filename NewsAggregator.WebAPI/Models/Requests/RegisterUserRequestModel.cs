using SendGrid.Helpers.Mail;
using System.ComponentModel.DataAnnotations;

namespace NewsAggregator.WebAPI.Models.Requests
{
    /// <summary>
    /// Request model for user registration.
    /// </summary>
    public class RegisterUserRequestModel
    {
        /// <summary>
        /// User email. Type: <see cref="DataType.EmailAddress"/>.
        /// </summary>
        [Required]
        [MinLength(5)]
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// User password. Type: <see cref="DataType.Password"/>. MinLength: 8.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string? Password { get; set; }

        /// <summary>
        /// User password confirmation. Must be equal to the password field.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        [Compare("Password")]
        public string? PasswordConfirmation { get; set; }
    }
}