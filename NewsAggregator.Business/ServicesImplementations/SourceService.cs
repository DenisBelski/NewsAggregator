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

    public async Task<SourceDto?> GetSourceByIdAsync(Guid sourceId)
    {
        var sourceEntity = await _unitOfWork.Sources.GetByIdAsync(sourceId);

        if (sourceEntity != null)
        {
            return _mapper.Map<SourceDto>(sourceEntity);
        }

        return null;
    }

    public SourceDto? GetSourceByName(string sourceName)
    {
        var sourceEntity = _unitOfWork.Sources
            .FindBy(source => source.Name.Equals(sourceName))
            .FirstOrDefault();

        if (sourceEntity != null)
        {
            return _mapper.Map<SourceDto>(sourceEntity);
        }

        return null;
    }

    public async Task<IEnumerable<SourceDto>?> GetAllSourcesAsync()
    {
        var sourceEntities = await _unitOfWork.Sources.GetAllAsync();

        if (sourceEntities != null)
        {
            return _mapper.Map<IEnumerable<SourceDto>>(sourceEntities).ToList();
        }

        return null;
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
            throw new ArgumentException(ex.Message, nameof(sourceId));
        }
    }
}