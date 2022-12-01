using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions
{
    public interface ISourceService
    {
        Task<List<SourceDto>> GetSourcesAsync();
        Task<SourceDto> GetSourceByIdAsync(Guid id);
        Task<int> CreateSourceAsync(SourceDto dto);
        Task DeleteSourceAsync(Guid id);
        Task<int> CreateSourcesAsync(IEnumerable<SourceDto> sourcesDto);
    }
}