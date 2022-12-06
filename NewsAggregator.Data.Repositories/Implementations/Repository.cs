using Microsoft.EntityFrameworkCore;
using NewsAggregator.Core;
using NewsAggregator.Data.Abstractions.Repositories;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;
using System.Linq.Expressions;

namespace NewsAggregator.Data.Repositories.Implementations
{
    public class Repository<T> : IRepository<T>
        where T : class, IBaseEntity
    {
        protected readonly NewsAggregatorContext Database;
        protected readonly DbSet<T> DbSet;

        public Repository(NewsAggregatorContext database)
        {
            Database = database;
            DbSet = database.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid? id)
        {
            return await DbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id.Equals(id));
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await DbSet.ToListAsync();
        }

        public virtual IQueryable<T> Get()
        {
            return DbSet.AsQueryable();
        }

        public virtual IQueryable<T> FindBy(Expression<Func<T, bool>> searchExpression,
            params Expression<Func<T, object>>[] includes)
        {
            var result = DbSet.Where(searchExpression);

            if (includes.Any())
            {
                result = includes.Aggregate(result, (current, include) =>
                    current.Include(include));
            }

            return result;
        }

        public virtual async Task AddAsync(T entity)
        {
            await DbSet.AddAsync(entity);
        }

        public virtual void Update(T entity)
        {
            DbSet.Update(entity);
        }
    }
}