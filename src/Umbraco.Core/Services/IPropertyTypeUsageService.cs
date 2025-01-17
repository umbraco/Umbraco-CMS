using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IPropertyTypeUsageService
{
    /// <summary>
    /// Checks if a property type has any saved property values associated with it.
    /// </summary>
    /// <param name="propertyTypeAlias">The alias of the property type to check.</param>
    /// <returns>True if the property type has any property values, otherwise false.</returns>
    [Obsolete("Please use HasSavedPropertyValuesAsync. Scheduled for removable in Umbraco 15.")]
    bool HasSavedPropertyValues(string propertyTypeAlias);

    /// <summary>
    /// Checks if a property type has any saved property values associated with it.
    /// </summary>
    /// <param name="contentTypeKey">The key of the content type to check.</param>
    /// <param name="propertyAlias">The alias of the property to check.</param>
    /// <returns>An attempt with status and result if the property type has any property values, otherwise false.</returns>
    Task<Attempt<bool, PropertyTypeOperationStatus>> HasSavedPropertyValuesAsync(Guid contentTypeKey, string propertyAlias);
}
