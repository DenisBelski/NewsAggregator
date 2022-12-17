using MediatR;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.CQS.Queries
{
    public class GetArticlesBySourceIdQuery : IRequest<List<Article>?>
    {
        public Guid? SourceId { get; set; }
    }
}