using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.Data.Abstractions;
using NewsAggregator.DataBase.Entities;
using Serilog;

namespace NewsAggregator.Business.ServicesImplementations;

public class SourceService : ISourceService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public SourceService(IMapper mapper, 
        IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<SourceDto?> GetSourceByIdAsync(Guid? sourceId)
    {
        try
        {
            var sourceEntity = await _unitOfWork.Sources.GetByIdAsync(sourceId);

            return sourceEntity != null
                ? _mapper.Map<SourceDto>(sourceEntity)
                : null;
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            throw new ArgumentException(ex.Message, nameof(sourceId));
        }
    }

    public SourceDto? GetSourceByName(string sourceName)
    {
        try
        {
            var sourceEntity = _unitOfWork.Sources
                .FindBy(source => source.Name.Equals(sourceName))
                .FirstOrDefault();

            return sourceEntity != null
                ? _mapper.Map<SourceDto>(sourceEntity)
                : null;
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            throw new ArgumentException(ex.Message, nameof(sourceName));
        }
    }

    public async Task<List<SourceDto>> GetAllSourcesAsync()
    {
        try
        {
            var sourceEntities = await _unitOfWork.Sources.GetAllAsync();

            return sourceEntities != null
                ? _mapper.Map<List<SourceDto>>(sourceEntities)
                : new List<SourceDto>();
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            throw new InvalidOperationException(ex.Message);
        }
    }

    public async Task DeleteSourceByIdAsync(Guid sourceId)
    {
        try
        {
            var sourceEntity = await _unitOfWork.Sources.GetByIdAsync(sourceId);

            if (sourceEntity != null)
            {
                _unitOfWork.Sources.RemoveSource(sourceEntity);
                await _unitOfWork.Commit();
            }
            else
            {
                Log.Warning($"The logic in {nameof(DeleteSourceByIdAsync)} method wasn't implemented, " +
                    $"because {nameof(sourceId)} parametr equals null");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            throw new ArgumentException(ex.Message, nameof(sourceId));
        }
    }
}