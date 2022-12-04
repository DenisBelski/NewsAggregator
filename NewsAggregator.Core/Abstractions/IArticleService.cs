using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions
{
    public interface IArticleService
    {
        Task<int> CreateArticleAsync(ArticleDto articleDto);
        Task<ArticleDto?> GetArticleByIdAsync(Guid articleId);
        Task<List<ArticleDto>> GetArticlesByPageNumberAsync(int pageNumber);
        Task<List<ArticleDto>> GetArticlesByRateAsync(double? rate);
        Task<List<ArticleDto>?> GetArticlesBySourceIdAsync(Guid sourceId);
        Task<int> UpdateArticleAsync(ArticleDto articleDto);
        Task<int> UpdateOnlyNecessaryDataInArticleAsync(Guid articleId, ArticleDto? patchList);
        Task AggregateArticlesFromAllAvailableSourcesAsync();
        Task AggregateArticlesFromOnlinerAsync(Guid sourceId, string? sourceRssUrl);
        Task AddArticleTextToArticlesFromOnlinerAsync();
        Task AddRateToArticlesAsync();
    }
}
