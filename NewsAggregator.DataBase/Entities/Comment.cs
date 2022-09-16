
namespace NewsAggregator.DataBase.Entities
{
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

}
