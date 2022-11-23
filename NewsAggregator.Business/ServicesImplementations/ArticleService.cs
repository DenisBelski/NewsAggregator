using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.Data.Abstractions;
using NewsAggregator.DataBase;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Business.ServicesImplementations
{
    public class ArticleService : IArticleService
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public ArticleService(IMapper mapper,
            IConfiguration configuration, 
            IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }
        public async Task<List<ArticleDto>> GetArticlesByPageNumberAndPageSizeAsync(int pageNumber, int pageSize)
        {
            try
            {
                var list = await _unitOfWork.Articles
                    .Get()
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
            var entity = await _unitOfWork.Articles.GetByIdAsync(id);
            var dto = _mapper.Map<ArticleDto>(entity);

            return dto;
        }

        public async Task Do()
        {
            await _unitOfWork.Articles.AddAsync(new Article());
            await _unitOfWork.Sources.AddAsync(new Source());

            await _unitOfWork.Commit();
        }
    }
}
