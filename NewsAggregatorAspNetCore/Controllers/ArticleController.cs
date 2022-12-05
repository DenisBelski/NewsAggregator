using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregatorAspNetCore.Models;
using Serilog;
using Serilog.Events;

namespace NewsAggregatorAspNetCore.Controllers
{
    //get filter which allow to watch articles only Users
    //[Authorize(Roles = "User")]
    public class ArticleController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IArticleService _articleService;
        private readonly ISourceService _sourceService;
        private readonly IRssService _rssService;
        private readonly IMapper _mapper;
        private readonly int _pageSize = 20;

        public ArticleController(IConfiguration configuration,
            IArticleService articleService,
            ISourceService sourceService,
            IRssService rssService,
            IMapper mapper)
        {
            _configuration = configuration;
            _articleService = articleService;
            _sourceService = sourceService;
            _rssService = rssService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                //var articles = await _articleService.GetArticlesByPageNumberAsync(page);

                var articles = await _articleService.GetArticlesByRateAsync();

                if (articles.Any())
                {
                    return View(articles);
                }

                return View();
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return BadRequest();
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var articleDto = await _articleService.GetArticleByIdAsync(id);

                if (articleDto != null)
                {
                    return View(_mapper.Map<ArticleModel>(articleDto));
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return RedirectToAction("Error", "Internal");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetArticlesFromSources()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return BadRequest();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> GetArticlesFromSources(string name)
        {
            try
            {
                if (!string.IsNullOrEmpty(name))
                {
                    var sourceDto = _sourceService.GetSourceByName(name); 

                    if (sourceDto != null && sourceDto.Name == name)
                    {
                        await _articleService.AggregateArticlesFromSourceWithSpecifiedIdAsync(sourceDto.Id);
                    }
                }
                else
                {
                    await _articleService.AggregateArticlesFromAllAvailableSourcesAsync();
                }

                return RedirectToAction("PersonalCabinetForAdmin", "Account");
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return BadRequest();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult CreateCustomArticle()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return BadRequest();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateCustomArticle(ArticleModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.Id = Guid.NewGuid();
                    model.PublicationDate = DateTime.Now;
                    model.SourceUrl = _configuration["CustomSource:SourceUrl"];
                    model.SourceId = new Guid(_configuration["CustomSource:SourceId"]);
                    
                    var articleDto = _mapper.Map<ArticleDto>(model);
                    await _articleService.CreateArticleAsync(articleDto);
                    await _articleService.RateArticleAsync(articleDto.Id);

                    return RedirectToAction("PersonalCabinetForAdmin", "Account");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return BadRequest();
            }
        }
    }
}
