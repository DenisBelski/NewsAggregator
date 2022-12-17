using MediatR;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.CQS.Queries
{
    public class GetSourceByIdQuery : IRequest<Source?>
    {
        public Guid? Id { get; set; }
    }
}