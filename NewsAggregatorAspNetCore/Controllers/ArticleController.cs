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
        private int _pageSize = 5;
        private readonly IArticleService _articleService;
        private readonly IRssService _rssService;
        private readonly IMapper _mapper;

        public ArticleController(IArticleService articleService,
            IRssService rssService,
            IMapper mapper)
        {
            _articleService = articleService;
            _mapper = mapper;
        }
        public async Task<IActionResult> Index(int page)
        {
            try
            {
                var articles = await _articleService
                    .GetArticlesByPageNumberAndPageSizeAsync(page, _pageSize);

                if (articles.Any())
                {
                    return View(articles);
                }
                else
                {
                    throw new ArgumentException(nameof(page));
                }
            }
            catch (Exception e)
            {
                Log.Error($"{e.Message}. {Environment.NewLine} {e.StackTrace}");
                return BadRequest();
            }
        }
        public async Task<IActionResult> Details(Guid id)
        {
            var dto = await _articleService.GetArticleByIdAsync(id);

            if (dto != null)
            {
                return View(dto);
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            //var model = new CreateArticleModel();

            //var sources = await _sourceService.GetSourcesAsync();

            //model.Sources = sources
            //    .Select(dto => new SelectListItem(
            //        dto.Name,
            //        dto.Id.ToString("G")))
            //    .ToList();

            //return View(model);

            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(ArticleModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.Title.ToUpperInvariant().Contains("123"))
                    {
                        ModelState.AddModelError("Title", "Article contains 123");
                        return View(model);
                    }

                    model.Id = Guid.NewGuid();
                    model.PublicationDate = DateTime.Now;

                    var dto = _mapper.Map<ArticleDto>(model);

                    await _articleService.CreateArticleAsync(dto);

                    return RedirectToAction("Index", "Article");
                }

                else
                {
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return StatusCode(500);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(ArticleModel model)
        {
            try
            {
                if (model != null)
                {
                    var dto = _mapper.Map<ArticleDto>(model);

                    await _articleService.UpdateArticleAsync(model.Id, dto);

                    //await _articleService.CreateArticleAsync(dto);

                    return RedirectToAction("Index", "Article");
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return StatusCode(500);
            }
        }
    }
}
