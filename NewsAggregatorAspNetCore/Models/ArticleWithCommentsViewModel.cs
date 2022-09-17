using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregatorAspNetCore.Models
{
    public class ArticleWithCommentsViewModel
    {
        public ArticleDto Article { get; set; }
        public List<string> Comments { get; set; }
    }
}
