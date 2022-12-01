using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Abstractions.Repositories
{
    public interface IArticleRepository : IRepository<Article>
    {
        Task UpdateArticleTextAsync(Guid id, string text);
    }
}