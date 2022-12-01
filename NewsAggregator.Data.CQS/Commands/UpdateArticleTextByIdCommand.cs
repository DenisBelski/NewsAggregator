using MediatR;

namespace NewsAggregator.Data.CQS.Commands
{
    public class UpdateArticleTextCommand : IRequest
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
    }
}