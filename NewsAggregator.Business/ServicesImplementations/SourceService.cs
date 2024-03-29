﻿using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.Data.Abstractions;
using NewsAggregator.Data.CQS.Queries;
using Serilog;

namespace NewsAggregator.Business.ServicesImplementations;

public class SourceService : ISourceService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public SourceService(IMapper mapper, 
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<SourceDto?> GetSourceByIdAsync(Guid? sourceId)
    {
        try
        {
            var sourceEntity = await _mediator.Send(new GetSourceByIdQuery() { Id = sourceId });

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

    public async Task<SourceDto?> GetSourceByNameAsync(string sourceName)
    {
        try
        {
            var sourceEntity = await _unitOfWork.Sources
                .FindBy(source => source.Name.Equals(sourceName))
                .FirstOrDefaultAsync();

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