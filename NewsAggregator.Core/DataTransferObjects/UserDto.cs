namespace NewsAggregator.Core.DataTransferObjects
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
