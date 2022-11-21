using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Abstractions.Repositories
{
    public interface IAdditionalArticleRepository : IRepository<Article>
    {
        void DoCustomMethod();
    }
}