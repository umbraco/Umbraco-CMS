namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IPropertyTypeUsageRepository
{
    [Obsolete("Please use HasSavedPropertyValuesAsync. Scheduled for removable in Umbraco 15.")]
    bool HasSavedPropertyValues(string propertyTypeAlias);
    Task<bool> HasSavedPropertyValuesAsync(Guid contentTypeKey, string propertyAlias);
    Task<bool> ContentTypeExistAsync(Guid contentTypeKey);
}
