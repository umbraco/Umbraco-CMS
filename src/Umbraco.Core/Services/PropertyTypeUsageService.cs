using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public class PropertyTypeUsageService : IPropertyTypeUsageService
{
    private readonly IPropertyTypeUsageRepository _propertyTypeUsageRepository;
    private readonly IContentTypeService _contentTypeService;
    private readonly ICoreScopeProvider _scopeProvider;

    [Obsolete("Use non-obsolete constructor. This will be removed in Umbraco 15.")]
    public PropertyTypeUsageService(
        IPropertyTypeUsageRepository propertyTypeUsageRepository,
        ICoreScopeProvider scopeProvider): this(propertyTypeUsageRepository, StaticServiceProvider.Instance.GetRequiredService<IContentTypeService>(), scopeProvider)
    {

    }

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
    [Obsolete("Please use HasSavedPropertyValuesAsync. Scheduled for removable in Umbraco 15.")]
    public bool HasSavedPropertyValues(string propertyTypeAlias)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        return _propertyTypeUsageRepository.HasSavedPropertyValues(propertyTypeAlias);
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
