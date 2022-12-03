using AutoMapper;
using MediatR;
using NewsAggregator.Data.CQS.Commands;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.CQS.Handlers.CommandHandlers
{
    public class AddRefreshTokenCommandHandler
        : IRequestHandler<AddRefreshTokenCommand, Unit>
    {
        private readonly NewsAggregatorContext _context;
        private readonly IMapper _mapper;

        public AddRefreshTokenCommandHandler(NewsAggregatorContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(AddRefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var refreshToken = new RefreshToken()
            {
                Id = Guid.NewGuid(),
                Token = command.TokenValue,
                UserId = command.UserId
            };

            await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}