using MediatR;

namespace NewsAggregator.Data.CQS.Queries
{
    public class GetAllArticlesWithoutTextIdsQuery : IRequest<Guid[]?>
    {

    }
}