using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions
{
    public interface IArticleService
    {
        Task<int> CreateArticleAsync(ArticleDto articleDto);
        Task<ArticleDto?> GetArticleByIdAsync(Guid articleId);
        Task<List<ArticleDto>> GetArticlesByPageNumberAsync(int pageNumber);
        Task<List<ArticleDto>> GetArticlesByRateAsync();
        Task<List<ArticleDto>?> GetArticlesBySourceIdAsync(Guid sourceId);
        Task<int> UpdateArticleAsync(ArticleDto articleDto);
        Task AggregateArticlesFromAllAvailableSourcesAsync();
        Task AggregateArticlesFromSourceWithSpecifiedIdAsync(Guid sourceId);
        Task RateArticleAsync(Guid articleId);
    }
}
