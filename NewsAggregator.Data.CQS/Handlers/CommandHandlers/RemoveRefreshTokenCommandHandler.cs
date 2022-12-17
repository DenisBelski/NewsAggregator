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

        public RemoveRefreshTokenCommandHandler(NewsAggregatorContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(RemoveRefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => command.TokenValue.Equals(rt.Token),
                    cancellationToken);

            if (refreshToken != null)
            {
                _context.RefreshTokens.Remove(refreshToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }
    }
}