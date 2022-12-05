using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions
{
    public interface ISourceService
    {
        Task<SourceDto?> GetSourceByIdAsync(Guid sourceId);
        SourceDto? GetSourceByName(string sourceName);
        Task<IEnumerable<SourceDto>?> GetAllSourcesAsync();
        Task DeleteSourceByIdAsync(Guid sourceId);
    }
}