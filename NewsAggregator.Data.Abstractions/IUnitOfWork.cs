using NewsAggregator.Data.Abstractions.Repositories;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Abstractions;

public interface IUnitOfWork
{
    IAdditionalArticleRepository Articles { get; }
    IGenericRepository<Source> Sources { get; }
    IGenericRepository<User> Users { get; }
    IGenericRepository<Role> Roles { get; }

    Task<int> Commit();
}