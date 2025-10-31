namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
/// Defines repository methods for querying property type usage.
/// </summary>
public interface IPropertyTypeUsageRepository
{
    /// <summary>
    /// Determines whether there are any saved property values for the specified content type and property alias.
    /// </summary>
    Task<bool> HasSavedPropertyValuesAsync(Guid contentTypeKey, string propertyAlias);

    /// <summary>
    /// Determines whether a content type with the specified unique identifier exists.
    /// </summary>
    Task<bool> ContentTypeExistAsync(Guid contentTypeKey);
}
