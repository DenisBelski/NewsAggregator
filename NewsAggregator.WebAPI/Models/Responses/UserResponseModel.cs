namespace NewsAggregator.WebAPI.Models.Responses
{
    /// <summary>
    /// Response model for displaying user data.
    /// </summary>
    public class UserResponseModel
    {
        /// <summary>
        /// User email.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// User role name.
        /// </summary>
        public string? RoleName { get; set; }

        /// <summary>
        /// User role id.
        /// </summary>
        public Guid RoleId { get; set; }
    }
}