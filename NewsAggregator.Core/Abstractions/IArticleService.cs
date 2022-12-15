﻿using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Core.Abstractions
{
    public interface IArticleService
    {
        Task<int> CreateArticleAsync(ArticleDto articleDto);
        Task<List<ArticleDto>> GetArticles();
        Task<ArticleDto?> GetArticleByIdAsync(Guid articleId);
        Task<List<ArticleDto>> GetArticlesByRateByPageNumberAndPageSizeAsync(double? rate, int pageNumber, int pageSize);
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
