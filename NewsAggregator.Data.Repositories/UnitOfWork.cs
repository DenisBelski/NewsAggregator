using NewsAggregator.Data.Abstractions;
using NewsAggregator.Data.Abstractions.Repositories;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly NewsAggregatorContext _database;

    public IAdditionalArticleRepository Articles { get; }
    public IGenericRepository<Source> Sources { get; }
    public IGenericRepository<User> Users { get; }
    public IGenericRepository<Role> Roles { get; }

    public UnitOfWork(NewsAggregatorContext database,
        IAdditionalArticleRepository articleRepository,
        IGenericRepository<Source> sourceRepository, 
        IGenericRepository<User> users, 
        IGenericRepository<Role> roles)
    {
        _database = database;
        Articles = articleRepository;
        Sources = sourceRepository;
        Users = users;
        Roles = roles;
    }

    public async Task<int> Commit()
    {
        return await _database.SaveChangesAsync();
    }
}