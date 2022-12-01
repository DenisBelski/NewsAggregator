using Microsoft.EntityFrameworkCore;
using NewsAggregator.Core;
using NewsAggregator.Data.Abstractions.Repositories;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Repositories.Implementations
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(NewsAggregatorContext database)
            : base(database)
        {
        }

        public async Task PatchUserAsync(Guid id, List<PatchModel> patchData)
        {
            var model = await DbSet.FirstOrDefaultAsync(entity => entity.Id.Equals(id));

            var nameValuePropertiesPairs = patchData
                .ToDictionary(
                    patchModel => patchModel.PropertyName,
                    patchModel => patchModel.PropertyValue);

            if (model != null)
            {
                var dbEntityEntry = Database.Entry(model);
                dbEntityEntry.CurrentValues.SetValues(nameValuePropertiesPairs);
                dbEntityEntry.State = EntityState.Modified;
            }
        }

        public void RemoveUser(User user)
        {
            DbSet.Remove(user);
        }
    }
}