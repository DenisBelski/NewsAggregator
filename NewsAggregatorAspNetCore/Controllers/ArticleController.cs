using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core;

namespace NewsAggregatorAspNetCore.Controllers
{
    public class ArticleController : Controller
    {
        private int _pageSize = 5;
        public IActionResult Index(int page)
        {
            var articles = ArticlesStorage.ArticlesList
                .Skip(page * _pageSize)
                .Take(_pageSize)
                .ToList();

            return View(articles);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            return Ok();
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
