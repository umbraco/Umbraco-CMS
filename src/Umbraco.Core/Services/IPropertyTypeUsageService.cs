namespace Umbraco.Cms.Core.Services;

public interface IPropertyTypeUsageService
{
    /// <summary>
    /// Checks if a property type has any saved property values associated with it.
    /// </summary>
    /// <param name="propertyTypeAlias">The alias of the property type to check.</param>
    /// <returns>True if the property type has any property values, otherwise false.</returns>
    bool HasSavedPropertyValues(string propertyTypeAlias);
}
