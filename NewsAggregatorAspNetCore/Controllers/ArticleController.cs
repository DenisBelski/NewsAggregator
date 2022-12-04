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

        //public async Task<IActionResult> Index(double? rate)

        public async Task<IActionResult> IndexAsync(int page)
        {
            try
            {
                var articles = await _articleService.GetArticlesByPageNumberAsync(page);

                //var articles = await _articleService.GetArticlesByRateAsync(rate);

                if (articles.Any())
                {
                    return View(articles);
                }
                else
                {
                    throw new ArgumentException(nameof(page));
                }
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
        public async Task<IActionResult> GetArticlesFromSources(SourceModel model)
        {
            try
            {
                //await _rssService.GetArticlesDataFromAllRssSourcesAsync();


                //await _articleService.AddArticleTextToArticlesFromOnlinerAsync();
                //await _articleService.AddArticleTextToArticlesFromDevbyAsync();
                await _articleService.AddArticleTextToArticlesFromShazooAsync();


                //await _articleService.AddRateToArticlesAsync();

                return RedirectToAction("PersonalCabinetForAdmin", "Account");

                //var sourceModel = await _sourceService.GetSourceByIdAsync(id);
                //if (sourceModel != null)
                //{
                //    var sourceDto = _mapper.Map<SourceDto>(sourceModel);
                //}
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
                    model.SourceUrl = "CustomUrl";
                    model.SourceId = new Guid("C0DC8F82-933E-4FFE-A576-0BC9BE4DFC8F");
                    
                    var articleDto = _mapper.Map<ArticleDto>(model);
                    await _articleService.CreateArticleAsync(articleDto);

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
