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

        [HttpGet]
        public async Task<IActionResult> Index(double rate)
        {
            try
            {
                //var articles = await _articleService.GetArticlesByPageNumberAsync(page);

                if (rate == 0)
                {
                    rate = Convert.ToDouble(_configuration["Rating:AcceptableRating"]);
                }

                var listArticleDto = await _articleService.GetArticlesByRateAsync(rate);

                return listArticleDto.Any() 
                    ? View(_mapper.Map<List<ArticleModel>>(listArticleDto)) 
                    : NotFound();
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return NotFound();
            }
        }

        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var articleDto = await _articleService.GetArticleByIdAsync(id);

                return articleDto != null
                    ? View(_mapper.Map<ArticleModel>(articleDto))
                    : NotFound();
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
                if (!string.IsNullOrEmpty(model.Name))
                {
                    var sourceDto = _sourceService.GetSourceByName(model.Name); 

                    if (sourceDto != null && sourceDto.Name == model.Name)
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
        public async Task<IActionResult> CreateCustomArticle(ArticleCreationModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var articleDto = _mapper.Map<ArticleDto>(model);

                    if (articleDto != null && articleDto.ArticleText != null)
                    {
                        articleDto.Id = Guid.NewGuid();
                        articleDto.PublicationDate = DateTime.Now;
                        articleDto.SourceUrl = _configuration["CustomSource:SourceUrl"];
                        articleDto.SourceId = new Guid(_configuration["CustomSource:SourceId"]);
                        articleDto.Rate = await _articleService.GetArticleRateByArticleTextAsync(articleDto.ArticleText);

                        await _articleService.CreateArticleAsync(articleDto);
                        return RedirectToAction("PersonalCabinetForAdmin", "Account");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return BadRequest();
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult ChangeRateOfArticlesToShow()
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
        public IActionResult ChangeRateOfArticlesToShow(ChangeArticleRateModel model)
        {
            try
            {
                if (model.Rate != Convert.ToDouble(_configuration["Rating:AcceptableRating"]))
                {
                    return RedirectToAction("Index", "Article", new { rate = model.Rate });
                }

                return RedirectToAction("PersonalCabinetForAdmin", "Account");
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return BadRequest();
            }
        }
    }
}
