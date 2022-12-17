using MediatR;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.CQS.Queries
{
    public class GetArticlesByRateQuery : IRequest<List<Article>?>
    {
        public double? Rate { get; set; }
    }
}