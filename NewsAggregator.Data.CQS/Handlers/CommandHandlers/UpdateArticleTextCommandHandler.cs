using MediatR;
using Microsoft.EntityFrameworkCore;
using NewsAggregator.Data.CQS.Commands;
using NewsAggregator.DataBase;

namespace NewsAggregator.Data.CQS.Handlers.CommandHandlers
{
    public class UpdateArticleTextCommandHandler
        : IRequestHandler<UpdateArticleTextCommand, Unit>
    {
        private readonly NewsAggregatorContext _context;

        public UpdateArticleTextCommandHandler(NewsAggregatorContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(UpdateArticleTextCommand byIdCommand, CancellationToken cancellationToken)
        {
            var article = await _context.Articles
                .FirstOrDefaultAsync(currentArticle => currentArticle.Id.Equals(byIdCommand.Id), cancellationToken);

            if (article != null)
            {
                article.ArticleText = byIdCommand.Text;
                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}