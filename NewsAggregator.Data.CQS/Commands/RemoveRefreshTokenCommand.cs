using MediatR;

namespace NewsAggregator.Data.CQS.Commands
{
    public class RemoveRefreshTokenCommand : IRequest
    {
        public Guid TokenValue { get; set; }
    }
}