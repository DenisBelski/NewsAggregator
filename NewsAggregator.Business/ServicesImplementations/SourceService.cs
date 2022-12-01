using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Core.DataTransferObjects;
using NewsAggregator.Data.Abstractions;
using NewsAggregator.DataBase.Entities;

namespace NewsAggregator.Business.ServicesImplementations;

public class SourceService : ISourceService
{
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;

    public SourceService(IMapper mapper, 
        IConfiguration configuration,
        IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<SourceDto>> GetSourcesAsync()
    {
        return await _unitOfWork.Sources.Get()
            .Select(source => _mapper.Map<SourceDto>(source))
            .ToListAsync();
    }

    public async Task<SourceDto> GetSourceByIdAsync(Guid id)
    {
        return _mapper.Map<SourceDto>(await _unitOfWork.Sources.GetByIdAsync(id));
    }

    public async Task<int> CreateSourceAsync(SourceDto dto)
    {
        var entity = _mapper.Map<Source>(dto);
        await _unitOfWork.Sources.AddAsync(entity);

        return await _unitOfWork.Commit();
    }

    public async Task<int> CreateSourcesAsync(IEnumerable<SourceDto> sourcesDto)
    {

        await _unitOfWork.Sources.AddRangeSourcesAsync(_mapper.Map<IEnumerable<Source>>(sourcesDto));
        return await _unitOfWork.Commit();

        //var listEntities = new List<Source>();

        //foreach (var sourceDto in sourcesDto)
        //{
        //    listEntities.Add(_mapper.Map<Source>(sourceDto));
        //}
        //await _unitOfWork.Sources.AddRangeSourcesAsync(listEntities);
    }

    public async Task DeleteSourceAsync(Guid id)
    {
        var source = await _unitOfWork.Sources.GetByIdAsync(id);

        if (source != null)
        {
            _unitOfWork.Sources.RemoveSource(source);

            await _unitOfWork.Commit();
        }
        else
        {
            throw new ArgumentException("Source for removing doesn't exist", nameof(id));
        }
    }

}