using NewsAggregator.Core;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Abstractions.Repositories
{
    public interface IArticleRepository : IRepository<Article>
    {
        Task AddRangeArticlesAsync(IEnumerable<Article> articles);
        Task PatchArticleAsync(Guid id, List<PatchModel> patchData);
        Task UpdateArticleTextAsync(Guid id, string text);
    }
}