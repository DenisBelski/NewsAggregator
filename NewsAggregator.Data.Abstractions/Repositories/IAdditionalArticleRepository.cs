using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Abstractions.Repositories
{
    public interface IAdditionalArticleRepository : IGenericRepository<Article>
    {
        Task UpdateArticleTextAsync(Guid id, string text);
    }
}