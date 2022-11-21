using NewsAggregator.Data.Abstractions.Repositories;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Abstractions;

public interface IUnitOfWork
{
    IAdditionalArticleRepository Articles { get; }
    IRepository<Source> Sources { get; }
    IRepository<User> Users { get; }
    IRepository<Role> Roles { get; }

    Task<int> Commit();
}