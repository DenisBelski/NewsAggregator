using MediatR;
using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Data.CQS.Commands
{
    public class AddArticleDataFromRssFeedCommand : IRequest
    {
        public IEnumerable<ArticleDto> Articles { get; set; }
    }
}