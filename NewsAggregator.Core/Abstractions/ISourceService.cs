using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions
{
    public interface ISourceService
    {
        Task<int> CreateSourceAsync(SourceDto sourceDto);
        Task<int> CreateSourcesAsync(IEnumerable<SourceDto> sourcesDto);
        Task<SourceDto?> GetSourceByIdAsync(Guid sourceId);
        Task<IEnumerable<SourceDto>?> GetAllSourcesAsync();
        Task<int> UpdateSourceAsync(SourceDto sourceDto);
        Task DeleteSourceByIdAsync(Guid sourceId);
    }
}