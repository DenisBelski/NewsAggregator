using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions
{
    public interface ISourceService
    {
        Task<SourceDto?> GetSourceByIdAsync(Guid sourceId);
        Task<IEnumerable<SourceDto>?> GetAllSourcesAsync();
        Task DeleteSourceByIdAsync(Guid sourceId);
    }
}