namespace NewsAggregator.WebAPI.Models.Requests
{
    /// <summary>
    /// Request model for user registration.
    /// </summary>
    public class RegisterUserRequestModel
    {
        /// <summary>
        /// User email.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// User password.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// User password confirmation.
        /// </summary>
        public string? PasswordConfirmation { get; set; }
    }
}