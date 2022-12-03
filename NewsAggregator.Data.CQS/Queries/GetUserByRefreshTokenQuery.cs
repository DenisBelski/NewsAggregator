using MediatR;
using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Data.CQS.Queries
{
    public class GetUserByRefreshTokenQuery : IRequest<UserDto?>
    {
        public Guid RefreshToken { get; set; }
    }
}