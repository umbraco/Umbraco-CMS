namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IPropertyTypeUsageRepository
{
    Task<bool> HasSavedPropertyValuesAsync(Guid contentTypeKey, string propertyAlias);
    Task<bool> ContentTypeExistAsync(Guid contentTypeKey);
}
