﻿using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.WebAPI.Models.Requests;
using NewsAggregator.WebAPI.Models.Responses;

namespace NewsAggregator.WebAPI.Controllers
{
    /// <summary>
    /// Controller for getting articles from resources
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiverNewsFromSources : ControllerBase
    {

        private readonly IArticleService _articleService;
        private readonly ISourceService _sourceService;
        private readonly IRssService _rssService;
        private readonly IMapper _mapper;


        public ReceiverNewsFromSources(IArticleService articleService,
            ISourceService sourceService,
            IRssService rssService,
            IMapper mapper)
        {
            _articleService = articleService;
            _sourceService = sourceService;
            _rssService = rssService;
            _mapper = mapper;
        }

        /// <summary>
        /// Add texts of articles from sources to the storage
        /// </summary>
        /// <param name="model">Contains article title, category, short summary and text</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddArticles([FromBody] AddOrUpdateArticleRequestModel? model)
        {
            try
            {
                var sources = await _sourceService.GetSourcesAsync();

                foreach (var source in sources)
                {
                    await _rssService.GetAllArticleDataFromRssAsync();
                    await _articleService.AddArticleTextToArticlesFromOnlinerAsync();
                }

                return Ok();

                //RecurringJob.AddOrUpdate(() => _articleService.AggregateArticlesFromExternalSourcesAsync(), "5,10,35 10-18 * * Mon-Fri");
                //Remove created jobs
                //RecurringJob.RemoveIfExists(nameof(_articleService.AggregateArticlesFromExternalSourcesAsync));
                //RecurringJob.AddOrUpdate(()=>_articleService.GetAllArticleDataFromRssAsync(), "*/15 * * * *");
                //RecurringJob.AddOrUpdate(()=>_articleService.AddArticleTextToArticlesAsync(), "*/30 * * * *");
            }
            catch (Exception ex)
            {

                return StatusCode(500, new ErrorModel { Message = ex.Message });
            }
        }

        /// <summary>
        /// Get articles with rate
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> RateArticles()
        {
            try
            {
                // Do for rate article separate RecurringJob
                // Think algorithm "take no more than 10 data at a time, for minute"
                await _articleService.AddRateToArticlesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorModel() { Message = ex.Message });
            }
        }
    }
}
