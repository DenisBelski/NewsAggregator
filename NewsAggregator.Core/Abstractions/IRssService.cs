using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions
{
    public interface IRssService
    {
        Task GetAllArticleDataFromRssAsync();
        Task GetAllArticleDataFromRssAsync(Guid sourceId, string? sourceRssUrl);
    }
}
