using MediatR;
using Microsoft.EntityFrameworkCore;
using NewsAggregator.Data.CQS.Queries;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.CQS.Handlers.QueryHandlers
{
    public class GetSourceByIdQueryHandler : IRequestHandler<GetSourceByIdQuery, Source?>
    {
        private readonly NewsAggregatorContext _context;

        public GetSourceByIdQueryHandler(NewsAggregatorContext context)
        {
            _context = context;
        }

        public async Task<Source?> Handle(GetSourceByIdQuery request,
            CancellationToken cancellationToken)
        {
            var source = await _context.Sources
                .AsNoTracking()
                .FirstOrDefaultAsync(source => source.Id.Equals(request.Id), cancellationToken);

            return source;
        }
    }
}