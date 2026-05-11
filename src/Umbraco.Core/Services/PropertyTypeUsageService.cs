using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc/>
public class PropertyTypeUsageService : IPropertyTypeUsageService
{
    private readonly IPropertyTypeUsageRepository _propertyTypeUsageRepository;
    private readonly ICoreScopeProvider _scopeProvider;

    // TODO (V18): Remove IContentTypeService parameter from constructor.

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyTypeUsageService"/> class.
    /// </summary>
    public PropertyTypeUsageService(
        IPropertyTypeUsageRepository propertyTypeUsageRepository,
#pragma warning disable IDE0060 // Remove unused parameter
        IContentTypeService contentTypeService,
#pragma warning restore IDE0060 // Remove unused parameter
        ICoreScopeProvider scopeProvider)
    {
        _propertyTypeUsageRepository = propertyTypeUsageRepository;
        _scopeProvider = scopeProvider;
    }

    /// <inheritdoc/>
    public async Task<Attempt<bool, PropertyTypeOperationStatus>> HasSavedPropertyValuesAsync(Guid contentTypeKey, string propertyAlias)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        var contentTypeExists = await _propertyTypeUsageRepository.ContentTypeExistAsync(contentTypeKey);

        if (contentTypeExists is false)
        {
            return Attempt.FailWithStatus(PropertyTypeOperationStatus.ContentTypeNotFound, false);
        }


        var hasSavedPropertyValues = await _propertyTypeUsageRepository.HasSavedPropertyValuesAsync(contentTypeKey, propertyAlias);

        return Attempt.SucceedWithStatus(PropertyTypeOperationStatus.Success, hasSavedPropertyValues);
    }
}
