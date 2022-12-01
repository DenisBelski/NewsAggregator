using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Abstractions.Repositories
{
    public interface ISourceRepository : IRepository<Source>
    {
        Task AddRangeSourcesAsync(IEnumerable<Source> sources);
        void RemoveSource(Source source);
    }
}