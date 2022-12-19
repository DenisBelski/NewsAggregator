using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.WebAPI.Models.Requests;
using NewsAggregator.WebAPI.Models.Responses;
using Serilog;

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
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSourceById(Guid id)
        {
            try
            {
                var sourceDto = await _sourceService.GetSourceByIdAsync(id);

                return sourceDto != null
                    ? Ok(_mapper.Map<SourceResponseModel>(sourceDto))
                    : NotFound(new ErrorResponseModel
                    {
                        ErrorMessage = $"No sources found with the specified {nameof(id)}."
                    });
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500, new ErrorResponseModel
                {
                    ErrorMessage = "The server encountered an unexpected situation."
                });
            }
        }

        /// <summary>
        /// Get all sources or get source by name.
        /// </summary>
        /// <param name="sourceModel">Contains source name.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(SourceResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(List<SourceResponseModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSources([FromQuery] GetSourceRequestModel sourceModel)
        {
            try
            {
                var listSources = await _sourceService.GetAllSourcesAsync();

                if (!listSources.Any())
                {
                    return NotFound(new ErrorResponseModel { ErrorMessage = "Sources not found." });
                }

                if (!string.IsNullOrEmpty(sourceModel.Name))
                {
                    var sourceWithSpecifiedName = _sourceService.GetSourceByNameAsync(sourceModel.Name);

                    return sourceWithSpecifiedName != null
                        ? Ok(_mapper.Map<SourceResponseModel>(sourceWithSpecifiedName))
                        : BadRequest(new ErrorResponseModel
                        {
                            ErrorMessage = $"Source with specified '{nameof(sourceModel.Name)}' doesn't exist."
                        });
                }

                return Ok(_mapper.Map<List<SourceResponseModel>>(listSources));
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500, new ErrorResponseModel
                {
                    ErrorMessage = "The server encountered an unexpected situation."
                });
            }
        }

        /// <summary>
        /// Delete source from storage by id.
        /// </summary>
        /// <param name="id">A source unique identifier as a <see cref="Guid"/>.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(typeof(SuccessResponseModel), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteSource(Guid id)
        {
            try
            {
                var sourceDto = await _sourceService.GetSourceByIdAsync(id);

                if (sourceDto == null)
                {
                    return NotFound(new ErrorResponseModel
                    {
                        ErrorMessage = $"No sources found with the specified {nameof(id)}"
                    });
                }

                await _sourceService.DeleteSourceByIdAsync(sourceDto.Id);
                return StatusCode(204, new SuccessResponseModel { DetailMessage = "Source deleted successfully." });
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}. {Environment.NewLine} {ex.StackTrace}");
                return StatusCode(500, new ErrorResponseModel
                {
                    ErrorMessage = "The server encountered an unexpected situation."
                });
            }
        }
    }
}
