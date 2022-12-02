using NewsAggregator.Data.Abstractions;
using NewsAggregator.Data.Abstractions.Repositories;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly NewsAggregatorContext _database;

        public IArticleRepository Articles { get; }
        public ISourceRepository Sources { get; }
        public IUserRepository Users { get; }
        public IRepository<Role> Roles { get; }

        public UnitOfWork(NewsAggregatorContext database,
            IArticleRepository articleRepository,
            ISourceRepository sourceRepository,
            IUserRepository userRepository,
            IRepository<Role> roles)
        {
            _database = database;
            Articles = articleRepository;
            Sources = sourceRepository;
            Users = userRepository;
            Roles = roles;
        }

        public async Task<int> Commit()
        {
            return await _database.SaveChangesAsync();
        }
    }
}