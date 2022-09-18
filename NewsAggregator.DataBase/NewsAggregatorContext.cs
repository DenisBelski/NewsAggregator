using Microsoft.EntityFrameworkCore;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.DataBase
{
    public class NewsAggregatorContext : DbContext
    {
        public DbSet<Article> Articles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Source> Sources { get; set; }
        public DbSet<User> Users { get; set; }

        public NewsAggregatorContext(DbContextOptions<NewsAggregatorContext> options)
            : base(options)
        {
        }
    }
} 