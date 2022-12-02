using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions
{
    public interface IArticleService
    {
        Task<int> CreateArticleAsync(ArticleDto articleDto);
        Task<int> CreateArticlesAsync(IEnumerable<ArticleDto> articlesDto);
        Task<ArticleDto?> GetArticleByIdAsync(Guid articleId);
        Task<List<ArticleDto>?> GetArticlesBySourceIdAsync(Guid sourceId);
        Task<List<ArticleDto>> GetArticlesByPageNumberAsync(int pageNumber);
        Task<int> UpdateArticleAsync(ArticleDto articleDto);
        Task<int> PatchArticleAsync(Guid articleId, ArticleDto? patchList);
        Task AddArticleTextToArticlesFromOnlinerAsync();
        Task AggregateArticlesFromExternalSourcesAsync();
        Task AddRateToArticlesAsync();
    }
}
