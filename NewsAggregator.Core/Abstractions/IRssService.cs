using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions
{
    public interface IRssService
    {
        Task GetAllArticleDataFromOnlinerRssAsync();
        Task GetAllArticleDataFromOnlinerRssAsync(Guid sourceId, string? sourceRssUrl);
    }
}
