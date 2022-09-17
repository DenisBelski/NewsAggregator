using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core;
using NewsAggregator.Core.Abstractions;

namespace NewsAggregatorAspNetCore.Controllers
{
    public class ArticleController : Controller
    {
        private int _pageSize = 5;
        private readonly IArticleService _articleService;

        public ArticleController(IArticleService articleService)
        {
            _articleService = articleService;
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
                    return View("NoArticles");
                }

            }
            catch (Exception)
            {
                throw new ArgumentException(nameof(page));
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

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            return Ok();
        }

        //[HttpPost]
        //public async Task<IActionResult> Edit(TestModel model)
        //{
        //    return Ok();
        //}

    }
}
