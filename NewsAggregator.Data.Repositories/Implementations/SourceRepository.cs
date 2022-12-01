using Microsoft.EntityFrameworkCore;
using NewsAggregator.Data.Abstractions.Repositories;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Repositories.Implementations
{
    public class SourceRepository : Repository<Source>, ISourceRepository
    {
        public SourceRepository(NewsAggregatorContext database)
            : base(database)
        {
        }

        public async Task AddRangeSourcesAsync(IEnumerable<Source> sources)
        {
            await DbSet.AddRangeAsync(sources);
        }

        public void RemoveSource(Source source)
        {
            DbSet.Remove(source);
        }
    }
}