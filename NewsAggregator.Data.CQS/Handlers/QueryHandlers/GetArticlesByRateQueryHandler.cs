using MediatR;
using Microsoft.EntityFrameworkCore;
using NewsAggregator.Data.CQS.Queries;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;
using System.Linq;

namespace NewsAggregator.Data.CQS.Handlers.QueryHandlers
{
    public class GetArticlesByRateQueryHandler : IRequestHandler<GetArticlesByRateQuery, List<Article>?>
    {
        private readonly NewsAggregatorContext _context;

        public GetArticlesByRateQueryHandler(NewsAggregatorContext context)
        {
            _context = context;
        }

        public async Task<List<Article>?> Handle(GetArticlesByRateQuery request,
            CancellationToken cancellationToken)
        {
            var listArticlesWithSpecifiedRate = await _context.Articles
                .Where(article => article.Rate != null && article.Rate > request.Rate)
                .ToListAsync(cancellationToken);

            return listArticlesWithSpecifiedRate;
        }
    }
}