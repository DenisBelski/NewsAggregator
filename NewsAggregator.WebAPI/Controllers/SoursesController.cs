using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
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
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSourceById(Guid id)
        {
            try
            {
                if (!Guid.Empty.Equals(id))
                {
                    var sourceDto = await _sourceService.GetSourceByIdAsync(id);

                    return sourceDto != null
                        ? Ok(_mapper.Map<SourceResponseModel>(sourceDto))
                        : NotFound(new ErrorModel
                        {
                            ErrorMessage = $"No sources found with the specified {nameof(id)}."
                        });
                }

                return BadRequest(new ErrorModel { ErrorMessage = "Failed, please check your input." });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, new ErrorModel
                {
                    ErrorMessage = "The server encountered an unexpected situation."
                });
            }
        }

        /// <summary>
        /// Get all sources or get source by name.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(SourceResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(List<SourceResponseModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSources([FromQuery] GetSourceRequestModel sourceModel)
        {
            try
            {
                var listSources = await _sourceService.GetAllSourcesAsync();

                if (!listSources.Any())
                {
                    return NotFound(new ErrorModel { ErrorMessage = "Sources not found." });
                }

                if (!string.IsNullOrEmpty(sourceModel.Name))
                {
                    var sourceWithSpecifiedName = _sourceService.GetSourceByName(sourceModel.Name);

                    return sourceWithSpecifiedName != null
                        ? Ok(_mapper.Map<SourceResponseModel>(sourceWithSpecifiedName))
                        : BadRequest(new ErrorModel
                        {
                            ErrorMessage = $"Source with specified name '{nameof(sourceModel.Name)}' doesn't exist."
                        });
                }

                return Ok(_mapper.Map<List<SourceResponseModel>>(listSources));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, new ErrorModel
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
        [ProducesResponseType(typeof(SuccessModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteSource(Guid id)
        {
            try
            {
                if (!Guid.Empty.Equals(id))
                {
                    var sourceForRemove = await _sourceService.GetSourceByIdAsync(id);

                    if (sourceForRemove == null)
                    {
                        return NotFound(new ErrorModel
                        {
                            ErrorMessage = $"No sources found with the specified {nameof(id)}"
                        });
                    }

                    await _sourceService.DeleteSourceByIdAsync(sourceForRemove.Id);
                    return Ok(new SuccessModel { DetailMessage = "Source deleted successfully." });
                }

                return BadRequest(new ErrorModel { ErrorMessage = "Failed, please check your input" });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, new ErrorModel
                {
                    ErrorMessage = "The server encountered an unexpected situation."
                });
            }
        }
    }
}
