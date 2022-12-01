using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NewsAggregator.Data.CQS.Commands;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.CQS.Handlers.CommandHandlers
{
    public class AddArticleDataFromRssFeedCommandHandler
        : IRequestHandler<AddArticleDataFromRssFeedCommand, Unit>
    {
        private readonly NewsAggregatorContext _context;
        private readonly IMapper _mapper;

        public AddArticleDataFromRssFeedCommandHandler(NewsAggregatorContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(AddArticleDataFromRssFeedCommand command, CancellationToken cancellationToken)
        {
            var oldArticleUrls = await _context.Articles
                .Select(article => article.SourceUrl)
                .ToArrayAsync(cancellationToken);

            var entities = command.Articles
                .Where(dto => !oldArticleUrls.Contains(dto.SourceUrl))
                .Select(dto => _mapper.Map<Article>(dto))
                .ToArray();

            await _context.Articles.AddRangeAsync(entities);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}