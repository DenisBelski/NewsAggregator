using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregatorAspNetCore.Models
{
    public class ArticlesListWithUserRoleModel
    {
        // We can use DTO, but it is better to create new model and add mapping profile
        public List<ArticleDto> Articles { get; set; }
        public bool IsAdmin { get; set; }
    }
}