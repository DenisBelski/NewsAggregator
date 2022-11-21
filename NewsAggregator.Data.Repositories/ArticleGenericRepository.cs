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

        public void DoCustomMethod()
        {
            throw new NotImplementedException();
        }
    }
}