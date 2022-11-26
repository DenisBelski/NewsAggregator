using Microsoft.EntityFrameworkCore;
using NewsAggregator.Data.Abstractions.Repositories;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Repositories
{
    public class ArticleGenericRepository : Repository<Article>, IAdditionalArticleRepository
    {
        public ArticleGenericRepository(NewsAggregatorContext database)
            : base(database)
        {
        }

        public async Task UpdateArticleTextAsync(Guid id, string text)
        {
            var article = await DbSet.FirstOrDefaultAsync(a => a.Id.Equals(id));
            if (article != null)
            {
                article.ArticleText = text;
            }
        }
    }
}