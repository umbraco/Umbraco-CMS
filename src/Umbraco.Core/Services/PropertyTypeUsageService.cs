using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public class PropertyTypeUsageService : IPropertyTypeUsageService
{
    private readonly IPropertyTypeUsageRepository _propertyTypeUsageRepository;
    private readonly IContentTypeService _contentTypeService;
    private readonly ICoreScopeProvider _scopeProvider;

    public PropertyTypeUsageService(
        IPropertyTypeUsageRepository propertyTypeUsageRepository,
        IContentTypeService contentTypeService,
        ICoreScopeProvider scopeProvider)
    {
        _propertyTypeUsageRepository = propertyTypeUsageRepository;
        _contentTypeService = contentTypeService;
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
