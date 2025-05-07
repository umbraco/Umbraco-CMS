using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public class DataTypeUsageService : IDataTypeUsageService
{
    private readonly IDataTypeUsageRepository _dataTypeUsageRepository;
    private readonly IDataTypeService _dataTypeService;
    private readonly ICoreScopeProvider _scopeProvider;

    public DataTypeUsageService(
        IDataTypeUsageRepository dataTypeUsageRepository,
        IDataTypeService dataTypeService,
        ICoreScopeProvider scopeProvider)
    {
        _dataTypeUsageRepository = dataTypeUsageRepository;
        _dataTypeService = dataTypeService;
        _scopeProvider = scopeProvider;
    }

    /// <inheritdoc/>
    public async Task<Attempt<bool, DataTypeOperationStatus>> HasSavedValuesAsync(Guid dataTypeKey)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        IDataType? dataType = await _dataTypeService.GetAsync(dataTypeKey);
        if (dataType is null)
        {
            return Attempt.FailWithStatus(DataTypeOperationStatus.NotFound, false);
        }

        var hasSavedValues = await _dataTypeUsageRepository.HasSavedValuesAsync(dataTypeKey);

        return Attempt.SucceedWithStatus(DataTypeOperationStatus.Success, hasSavedValues);
    }
}
