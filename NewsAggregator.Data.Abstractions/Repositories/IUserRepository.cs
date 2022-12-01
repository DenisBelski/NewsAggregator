using NewsAggregator.Core;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Abstractions.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        void RemoveUser(User user);
        Task PatchUserAsync(Guid id, List<PatchModel> patchData);
    }
}