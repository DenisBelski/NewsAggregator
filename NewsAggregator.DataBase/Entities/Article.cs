
namespace NewsAggregator.DataBase.Entities
{
    public class Article
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string ArticleText { get; set; }
        public DateTime PublicationDate { get; set; }
        public Source Source { get; set; }
        public Guid SourceId { get; set; }
        public List<Comment> Comments { get; set; }
        public ArticleEvaluation ArticleEvaluation { get; set; }
    }
    public class Source
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public List <Article> Articles { get; set; }
        public SourceType SourceType { get; set; }
    }
    public class Comment
    {
        public Guid Id { get; set; }
        public string CommentText { get; set; }
        public DateTime PublicationDate { get; set; }
        public Article Article { get; set; }
        public Guid ArticleId { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
    }
    public enum ArticleEvaluation
    {
        BadNews,
        NeutralNews,
        GoodNews
    }
    public enum SourceType
    {
        Api,
        Rss
    }

    public enum UserType
    {
        Administrator,
        RegisteredUser,
        AnonymousUser
    }

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
