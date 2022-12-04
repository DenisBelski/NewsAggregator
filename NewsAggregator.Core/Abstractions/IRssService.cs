using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions
{
    public interface IRssService
    {
        Task GetArticlesDataFromAllRssSourcesAsync();
        Task GetArticlesDataFromAllRssSourcesAsync(Guid sourceId, string? sourceRssUrl);
        Task GetArticlesDataFromOnlinerRssAsync(Guid sourceId, string? sourceRssUrl);
    }
}
