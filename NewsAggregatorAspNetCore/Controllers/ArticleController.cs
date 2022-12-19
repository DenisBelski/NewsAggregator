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
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly int _pageSize = 7;

        public ArticleController(IConfiguration configuration,
            IArticleService articleService,
            ISourceService sourceService,
            IUserService userService,
            IMapper mapper)
        {
            _configuration = configuration;
            _articleService = articleService;
            _sourceService = sourceService;
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page)
        {
            try
            {
                var rate = Convert.ToDouble(_configuration["Rating:AcceptableRating"]);
                var listArticleDto = await _articleService.GetArticlesByRateByPageNumberAndPageSizeAsync(rate, page, _pageSize);

                return listArticleDto.Any()
                    ? View(_mapper.Map<List<ArticleModel>>(listArticleDto))
                    : RedirectToAction("CustomError", "Home", new { statusCode = 404 });
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return RedirectToAction("CustomError", "Home", new { statusCode = 500 });
            }
        }

        [Authorize(Roles = "User, Admin")]
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var userEmail = User.Identity?.Name;

                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("CustomError", "Home");
                }

                var userDto = await _userService.GetUserWithRoleByEmailAsync(userEmail);
                var articleDto = await _articleService.GetArticleByIdAsync(id);
                var articleWithRoleModel = _mapper.Map<ArticleWithUserRoleModel>(articleDto);

                if (userDto != null
                    && articleDto != null
                    && userDto.RoleName == _configuration["UserRoles:Admin"])
                {
                    articleWithRoleModel.IsAdmin = true;
                }

                return View(articleWithRoleModel);
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return RedirectToAction("CustomError", "Home", new { statusCode = 500 });
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
                return RedirectToAction("CustomError", "Home", new { statusCode = 500 });
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
                    var sourceDto = await _sourceService.GetSourceByNameAsync(model.Name); 

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
                return RedirectToAction("CustomError", "Home", new { statusCode = 500 });
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
                return RedirectToAction("CustomError", "Home", new { statusCode = 500 });
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
                return RedirectToAction("CustomError", "Home", new { statusCode = 500 });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var articleDto = await _articleService.GetArticleByIdAsync(id);

                return articleDto != null 
                    ? View(_mapper.Map<ArticleModel>(articleDto)) 
                    : RedirectToAction("CustomError", "Home", new { statusCode = 404 });
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return RedirectToAction("CustomError", "Home", new { statusCode = 500 });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(ArticleModel model)
        {
            try
            {
                var newArticleDto = _mapper.Map<ArticleDto>(model);

                if (newArticleDto != null && newArticleDto.ArticleText != null)
                {
                    var oldArticleDto = await _articleService.GetArticleByIdAsync(newArticleDto.Id);

                    newArticleDto.PublicationDate = DateTime.Now;
                    newArticleDto.SourceId = oldArticleDto.SourceId;
                    newArticleDto.SourceUrl = oldArticleDto.SourceUrl;
                    newArticleDto.Rate = await _articleService.GetArticleRateByArticleTextAsync(newArticleDto.ArticleText);

                    await _articleService.UpdateArticleAsync(newArticleDto);
                    return RedirectToAction("PersonalCabinetForAdmin", "Account");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return RedirectToAction("CustomError", "Home", new { statusCode = 500 });
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
                return RedirectToAction("CustomError", "Home", new { statusCode = 500 });
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
                    _configuration["Rating:AcceptableRating"] = model.Rate.ToString();
                    return RedirectToAction("Index", "Article");
                }

                return RedirectToAction("PersonalCabinetForAdmin", "Account");
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return RedirectToAction("CustomError", "Home", new { statusCode = 500 });
            }
        }
    }
}
