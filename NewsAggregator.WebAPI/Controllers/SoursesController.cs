using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.WebAPI.Models.Requests;
using NewsAggregator.WebAPI.Models.Responses;

namespace NewsAggregator.WebAPI.Controllers
{
    /// <summary>
    /// Controller for work with sources.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SourcesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ISourceService _sourceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourcesController"/> class.
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="sourceService"></param>
        public SourcesController(IMapper mapper,
            ISourceService sourceService)
        {
            _mapper = mapper;
            _sourceService = sourceService;
        }

        /// <summary>
        /// Get source from the storage by source id.
        /// </summary>
        /// <param name="id">A source unique identifier as a <see cref="Guid"/></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SourceResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Nullable), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSourceById(Guid id)
        {
            var sourceDto = await _sourceService.GetSourceByIdAsync(id);

            return sourceDto != null
                ? Ok(_mapper.Map<SourceResponseModel>(sourceDto))
                : NotFound(new ErrorModel { ErrorMessage = $"No sources found with the specified {nameof(id)}" });
        }

        /// <summary>
        /// Get all sources or get source by name.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<SourceResponseModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSources([FromQuery] GetSourceRequestModel? sourceModel)
        {
            var listSources = await _sourceService.GetAllSourcesAsync();

            if (!listSources.Any())
            {
                return NotFound(new ErrorModel { ErrorMessage = "Sources not found" });
            }

            if (sourceModel != null && !string.IsNullOrEmpty(sourceModel.Name))
            {
                var sourceWithSpecifiedName = _sourceService.GetSourceByName(sourceModel.Name);

                return sourceWithSpecifiedName != null
                    ? Ok(_mapper.Map<SourceResponseModel>(sourceWithSpecifiedName))
                    : NotFound(new ErrorModel { ErrorMessage = $"No sources found with the specified {nameof(sourceModel.Name)}" });
            }

            return Ok(_mapper.Map<List<SourceResponseModel>>(listSources));
        }

        /// <summary>
        /// Delete source from storage by id.
        /// </summary>
        /// <param name="id">Contains source id.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesDefaultResponseType ]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Nullable), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteSource(Guid id)
        {
            if (!Guid.Empty.Equals(id))
            {
                var sourceForRemove = await _sourceService.GetSourceByIdAsync(id);

                if (sourceForRemove == null)
                {
                    return NotFound(new ErrorModel { ErrorMessage = $"No sources found with the specified {nameof(id)}" });
                }

                await _sourceService.DeleteSourceByIdAsync(sourceForRemove.Id);
                return Ok();
            }

            return StatusCode(500);
        }
    }
}
