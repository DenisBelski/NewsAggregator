using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Abstractions.Repositories
{
    public interface IAdditionalArticleRepository : IRepository<Article>
    {
        Task UpdateArticleTextAsync(Guid id, string text);
    }
}