using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Abstractions.Repositories
{
    public interface ISourceRepository
    {
        public Task<Source?> GetSourceByIdAsync(Guid id);

        public IQueryable<Source> GetSourcesAsQueryable();

        public Task<List<Source?>> GetAllSourcesAsync();

        public Task AddSourceAsync(Source article);

        public Task AddSourcesAsync(IEnumerable<Source> articles);

        public Task RemoveSourceAsync(Source article);

        public Task UpdateSource(Guid id, Source article);
    }
}