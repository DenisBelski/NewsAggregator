namespace NewsAggregatorAspNetCore.Models
{
    public class ArticleModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public DateTime PublicationDate { get; set; }
    }
}
