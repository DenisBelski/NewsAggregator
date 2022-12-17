using MediatR;

namespace NewsAggregator.Data.CQS.Commands
{
    public class AddRefreshTokenCommand : IRequest
    {
        public Guid TokenValue { get; set; }
        public Guid UserId { get; set; }
    }
}