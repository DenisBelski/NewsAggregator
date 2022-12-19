using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions
{
    public interface ISourceService
    {
        Task<SourceDto?> GetSourceByIdAsync(Guid? sourceId);
        Task<SourceDto?> GetSourceByNameAsync(string sourceName);
        Task<List<SourceDto>> GetAllSourcesAsync();
        Task DeleteSourceByIdAsync(Guid sourceId);
    }
}