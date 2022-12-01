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
        private readonly IMapper _mapper;

        public GetAllArticlesWithoutTextQueryHandler(NewsAggregatorContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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