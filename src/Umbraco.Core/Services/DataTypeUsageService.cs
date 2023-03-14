using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

public class DataTypeUsageService : IDataTypeUsageService
{
    private readonly IDataTypeUsageRepository _dataTypeUsageRepository;
    private readonly ICoreScopeProvider _scopeProvider;

    public DataTypeUsageService(
        IDataTypeUsageRepository dataTypeUsageRepository,
        ICoreScopeProvider scopeProvider)
    {
        _dataTypeUsageRepository = dataTypeUsageRepository;
        _scopeProvider = scopeProvider;
    }

    /// <inheritdoc/>
    public bool HasSavedValues(int dataTypeId)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return _dataTypeUsageRepository.HasSavedValues(dataTypeId);
    }
}
