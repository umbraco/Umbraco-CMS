using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

public class PropertyTypeUsageService : IPropertyTypeUsageService
{
    private readonly IPropertyTypeUsageRepository _propertyTypeUsageRepository;
    private readonly ICoreScopeProvider _scopeProvider;

    public PropertyTypeUsageService(
        IPropertyTypeUsageRepository propertyTypeUsageRepository,
        ICoreScopeProvider scopeProvider)
    {
        _propertyTypeUsageRepository = propertyTypeUsageRepository;
        _scopeProvider = scopeProvider;
    }

    /// <inheritdoc/>
    public bool HasSavedPropertyValues(string propertyTypeAlias)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return _propertyTypeUsageRepository.HasSavedPropertyValues(propertyTypeAlias);
    }
}
