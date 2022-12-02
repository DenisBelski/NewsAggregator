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
        private readonly int _pageSize = 20;
        private readonly IArticleService _articleService;
        private readonly IRssService _rssService;
        private readonly IMapper _mapper;

        public ArticleController(IArticleService articleService,
            IRssService rssService,
            IMapper mapper)
        {
            _articleService = articleService;
            _rssService = rssService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(int page)
        {
            try
            {
                var articles = await _articleService.GetArticlesByPageNumberAsync(page);

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
        public async Task<IActionResult> Create()
        {
            try
            {
                //var sources = await _sourceService.GetSourcesAsync();

                //model.Sources = sources
                //    .Select(dto => new SelectListItem(
                //        dto.Name,
                //        dto.Id.ToString("G")))
                //    .ToList();

                //return View(model);

                //await _articleService.CreateArticleAsync(articleDto);
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
        public async Task<IActionResult> Create(ArticleModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.Id = Guid.NewGuid();
                    model.PublicationDate = DateTime.Now;

                    var dto = _mapper.Map<ArticleDto>(model);

                    await _articleService.CreateArticleAsync(dto);

                    return RedirectToAction("Index", "Article");
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
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                if (id != Guid.Empty)
                {
                    var articleDto = await _articleService.GetArticleByIdAsync(id);
                    if (articleDto == null)
                    {
                        return BadRequest();
                    }

                    var editModel = _mapper.Map<ArticleModel>(articleDto);

                    return View(editModel);
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return BadRequest();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(ArticleModel model)
        {
            try
            {
                if (model != null)
                {
                    await _articleService.UpdateArticleAsync(_mapper.Map<ArticleDto>(model));

                    return RedirectToAction("Index", "Article");
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return BadRequest();
            }
        }
    }
}
