using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.DataBase;

namespace NewsAggregator.Business.ServicesImplementations
{
    public class ArticleService : IArticleService
    {
        private readonly NewsAggregatorContext _databaseContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public ArticleService(NewsAggregatorContext databaseContext, 
            IMapper mapper,
            IConfiguration configuration)
        {
            _databaseContext = databaseContext;
            _mapper = mapper;
            _configuration = configuration;
        }
        public async Task<List<ArticleDto>> GetArticlesByPageNumberAndPageSizeAsync(int pageNumber, int pageSize)
        {
            try
            {
                //var passwordSalt = _configuration["UserSecrets:PasswordSalt"];
                var list = await _databaseContext.Articles
                    .Skip(pageNumber * pageSize)
                    .Take(pageSize)
                    .Select(article => _mapper.Map<ArticleDto>(article))
                    .ToListAsync();

                return list;
            }
            catch (Exception)
            {
                throw;
            }
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
