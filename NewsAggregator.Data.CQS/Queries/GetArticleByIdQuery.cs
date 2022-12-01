using MediatR;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.CQS.Queries
{
    public class GetArticleByIdQuery : IRequest<Article?>
    {
        public Guid Id { get; set; }
    }
}