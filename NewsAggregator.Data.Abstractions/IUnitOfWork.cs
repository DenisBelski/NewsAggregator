using NewsAggregator.Data.Abstractions.Repositories;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Abstractions;

public interface IUnitOfWork
{
    IArticleRepository Articles { get; }
    ISourceRepository Sources { get; }
    IUserRepository Users { get; }
    IRepository<Role> Roles { get; }
    Task<int> Commit();
}