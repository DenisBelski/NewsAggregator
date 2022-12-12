using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NewsAggregator.Data.CQS.Queries;
using NewsAggregator.DataBase;

namespace NewsAggregator.Data.CQS.Handlers.QueryHandlers
{
    public class GetAllArticlesWithoutTextQueryHandler : IRequestHandler<GetAllArticlesWithoutTextIdsQuery, Guid[]?>
    {
        private readonly NewsAggregatorContext _context;

        public GetAllArticlesWithoutTextQueryHandler(NewsAggregatorContext context)
        {
            _context = context;
        }

        public async Task<Guid[]?> Handle(GetAllArticlesWithoutTextIdsQuery request,
            CancellationToken cancellationToken)
        {
            var articlesWithEmptyTextIds = await _context.Articles
                .AsNoTracking()
                .Where(article => string.IsNullOrEmpty(article.ArticleText))
                .Select(article => article.Id)
                .ToArrayAsync(cancellationToken);

            return articlesWithEmptyTextIds;
        }
    }
}