
namespace NewsAggregator.DataBase.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime RegistrationDate { get; set; }
        public List<Comment> Comments { get; set; }
        public UserType UserType { get; set; }
    }
}
