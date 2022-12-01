using MediatR;
using Microsoft.EntityFrameworkCore;
using NewsAggregator.Data.CQS.Queries;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.CQS.Handlers.QueryHandlers
{
    public class GetArticleByIdQueryHandler : IRequestHandler<GetArticleByIdQuery, Article?>
    {
        private readonly NewsAggregatorContext _context;

        public GetArticleByIdQueryHandler(NewsAggregatorContext context)
        {
            _context = context;
        }

        public async Task<Article?> Handle(GetArticleByIdQuery request,
            CancellationToken cancellationToken)
        {
            var article = await _context.Articles
                .AsNoTracking()
                .FirstOrDefaultAsync(currentArticle => currentArticle.Id.Equals(request.Id), cancellationToken);

            return article;
        }
    }
}