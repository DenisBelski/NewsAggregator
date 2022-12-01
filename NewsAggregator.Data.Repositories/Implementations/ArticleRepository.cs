﻿using Microsoft.EntityFrameworkCore;
using NewsAggregator.Core;
using NewsAggregator.Data.Abstractions.Repositories;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Data.Repositories.Implementations
{
    public class ArticleRepository : Repository<Article>, IArticleRepository
    {
        public ArticleRepository(NewsAggregatorContext database)
            : base(database)
        {
        }

        public async Task AddRangeArticlesAsync(IEnumerable<Article> articles)
        {
            await DbSet.AddRangeAsync(articles);
        }

        public async Task PatchArticleAsync(Guid id, List<PatchModel> patchData)
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

        public async Task UpdateArticleTextAsync(Guid id, string text)
        {
            var article = await DbSet
                .FirstOrDefaultAsync(currentArticle => currentArticle.Id.Equals(id));

            if (article != null)
            {
                article.ArticleText = text;
            }
        }
    }
}