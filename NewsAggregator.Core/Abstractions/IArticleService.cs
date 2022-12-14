using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions
{
    public interface IArticleService
    {
        Task<int> CreateArticleAsync(ArticleDto articleDto);
        Task<List<ArticleDto>> GetArticles();
        Task<ArticleDto?> GetArticleByIdAsync(Guid articleId);
        Task<List<ArticleDto>> GetArticlesByPageNumberAsync(int pageNumber);
        Task<List<ArticleDto>> GetArticlesByRateAsync(double? rate);
        Task<List<ArticleDto>> GetArticlesBySourceIdAsync(Guid? sourceId);
        Task<int> UpdateArticleAsync(ArticleDto articleDto);
        
        Task AggregateArticlesFromAllAvailableSourcesAsync();
        Task AggregateArticlesFromSourceWithSpecifiedIdAsync(Guid sourceId);
        Task AddRateToArticlesAsync();
        Task<double> GetArticleRateByArticleTextAsync(string articleText);
        Task AddArticleTextToArticlesForAllAvailableSourcesAsync();

        Task<int> UpdateOnlyOnleArticleFieldAsync(Guid articleId, List<PatchModel> patchData);
    }
}
