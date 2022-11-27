using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions
{
    public interface IArticleService
    {
        Task<ArticleDto> GetArticleByIdAsync(Guid id);
        Task<int> CreateArticleAsync(ArticleDto dto);
        Task<int> UpdateArticleAsync(Guid id, ArticleDto? patchList);
        Task<List<ArticleDto>> GetArticlesByPageNumberAndPageSizeAsync(int pageNumber, int pageSize);


        Task AddArticleTextToArticlesAsync();

        // for WebAPI
        Task<List<ArticleDto>> GetArticlesByNameAndSourcesAsync(string? name, Guid? category);
        Task DeleteArticleAsync(Guid id);
        Task AggregateArticlesFromExternalSourcesAsync();
        Task AddRateToArticlesAsync();
    }
}
