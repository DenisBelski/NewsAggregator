namespace NewsAggregator.WebAPI.Models.Requests
{
    public class GetArticlesRequestModel
    {
        public string? Name { get; set; }
        public Guid SourceId { get; set; }
    }
}