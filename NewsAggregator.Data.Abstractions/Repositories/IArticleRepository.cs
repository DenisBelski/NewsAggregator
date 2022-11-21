using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Abstractions.Repositories
{
    public interface IArticleRepository
    {
        public Task<Article?> GetArticleByIdAsync(Guid id);

        public IQueryable<Article> GetArticlesAsQueryable();

        public Task<List<Article?>> GetAllArticlesAsync();

        public Task AddArticleAsync(Article article);

        public Task AddArticlesAsync(IEnumerable<Article> articles);

        public Task RemoveArticleAsync(Article article);

        public Task UpdateArticle(Guid id, Article article);
    }
}