using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NewsAggregator.Data.CQS.Queries;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.CQS.Handlers.QueryHandlers
{
    public class GetArticlesBySourceIdQueryHandler : IRequestHandler<GetArticlesBySourceIdQuery, List<Article>?>
    {
        private readonly NewsAggregatorContext _context;

        public GetArticlesBySourceIdQueryHandler(NewsAggregatorContext context)
        {
            _context = context;
        }

        public async Task<List<Article>?> Handle(GetArticlesBySourceIdQuery request,
            CancellationToken cancellationToken)
        {
            var articleListEntities = await _context.Articles
                .Where(article => article.SourceId.Equals(request.SourceId))
                .ToListAsync(cancellationToken);

            return articleListEntities;
        }
    }
}