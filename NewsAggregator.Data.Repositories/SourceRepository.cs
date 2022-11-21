using Microsoft.EntityFrameworkCore;
using NewsAggregator.Data.Abstractions.Repositories;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Repositories
{
    public class SourceRepository : ISourceRepository 
    {
        private readonly NewsAggregatorContext _database;
        
        public SourceRepository(NewsAggregatorContext database)
        {
            _database = database;
        }

        public async Task<Source?> GetSourceByIdAsync(Guid id)
        {
            return await _database.Sources.FirstOrDefaultAsync(source => source.Id.Equals(id));
        }
        
        //not for regular usage
        public IQueryable<Source> GetSourcesAsQueryable()
        {
            return _database.Sources;
        }

        public async Task<List<Source?>> GetAllSourcesAsync()
        {
            return await _database.Sources.ToListAsync();
        }

        public async Task AddSourceAsync(Source source)
        {
            await _database.Sources.AddAsync(source);
        }

        public async Task AddSourcesAsync(IEnumerable<Source> articles)
        {
            await _database.Sources.AddRangeAsync(articles);
        }

        public async Task RemoveSourceAsync(Source source)
        {
            _database.Sources.Remove(source);
        }

        public async Task UpdateSource(Guid id, Source source)
        {
            var entity = await _database.Sources.FirstOrDefaultAsync(source => source.Id.Equals(id));

            if (entity!=null)
            {
                entity = source;
            }
        }
    }
}