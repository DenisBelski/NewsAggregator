using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NewsAggregator.Data.CQS.Commands;
using NewsAggregator.DataBase;

namespace NewsAggregator.Data.CQS.Handlers.CommandHandlers
{
    public class RemoveRefreshTokenCommandHandler
        : IRequestHandler<RemoveRefreshTokenCommand, Unit>
    {
        private readonly NewsAggregatorContext _context;
        private readonly IMapper _mapper;

        public RemoveRefreshTokenCommandHandler(NewsAggregatorContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(RemoveRefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => command.TokenValue.Equals(rt.Token),
                    cancellationToken);

            _context.RefreshTokens.Remove(refreshToken);
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}