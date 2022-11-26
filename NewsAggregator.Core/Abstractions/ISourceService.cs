using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions
{
    public interface ISourceService
    {
        Task<List<SourceDto>> GetSourcesAsync();
        Task<SourceDto> GetSourcesByIdAsync(Guid id);
        Task<int> CreateSourcesAsync(SourceDto dto);
    }
}