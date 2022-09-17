using NewsAggregator.Core;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;

namespace NewsAggregator.Business.ServicesImplementations
{
    public class ArticleService : IArticleService
    {
        //private readonly ArticlesStorage _articlesStorage;

        //public ArticleService(ArticlesStorage articlesStorage)
        //{
        //    _articlesStorage = articlesStorage;
        //}

        public async Task<List<ArticleDto>> GetArticlesByPageNumberAndPageSizeAsync(int pageNumber, int pageSize)
        {
            List<ArticleDto> list;

            list = ArticlesStorage.ArticlesList
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .ToList();

            return list;
        }
        public async Task<List<ArticleDto>> GetNewArticlesFromExternalSourcesAsync()
        {
            var list = new List<ArticleDto>();
            return list;
        }
        public async Task<ArticleDto> GetArticleByIdAsync(Guid id)
        {
            var dto = new ArticleDto();
            //var dto = ArticlesStorage.ArticlesList
            //    .FirstOrDefault(articleDto => articleDto.Id.Equals(id));
            return dto;
        }
    }
}
