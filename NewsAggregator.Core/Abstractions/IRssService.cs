using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions
{
    public interface IRssService
    {
        Task GetArticlesDataFromAllAvailableRssSourcesAsync();
        Task GetArticlesDataFromRssSourceWithSpecifiedIdAsync(Guid sourceId);
    }
}
