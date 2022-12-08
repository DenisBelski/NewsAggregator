using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions
{
    public interface IArticleService
    {
        Task<int> CreateArticleAsync(ArticleDto articleDto);
        Task<IEnumerable<ArticleDto>> GetArticles();
        Task<ArticleDto?> GetArticleByIdAsync(Guid articleId);
        Task<IEnumerable<ArticleDto>> GetArticlesByPageNumberAsync(int pageNumber);
        Task<IEnumerable<ArticleDto>> GetArticlesByRateAsync(double? rate);
        Task<IEnumerable<ArticleDto>> GetArticlesBySourceIdAsync(Guid? sourceId);
        Task<int> UpdateArticleAsync(ArticleDto articleDto);
        Task AggregateArticlesFromAllAvailableSourcesAsync();
        Task AggregateArticlesFromSourceWithSpecifiedIdAsync(Guid sourceId);
        Task AddRateToArticlesAsync();
        Task<double> GetArticleRateByArticleTextAsync(string articleText);
        Task AddArticleTextToArticlesForAllAvailableSourcesAsync();
    }
}
