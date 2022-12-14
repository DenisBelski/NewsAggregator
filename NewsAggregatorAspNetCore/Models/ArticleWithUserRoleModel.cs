namespace NewsAggregatorAspNetCore.Models
{
    public class ArticleWithUserRoleModel
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? ShortDescription { get; set; }
        public string? ArticleText { get; set; }
        public string? Category { get; set; }
        public DateTime PublicationDate { get; set; }
        public bool IsAdmin { get; set; }
    }
}