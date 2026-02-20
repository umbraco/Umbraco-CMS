using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IMemberType"/>.
/// </summary>
public static class MemberTypeExtensions
{
    /// <summary>
    /// Gets all property types that are marked as sensitive.
    /// </summary>
    /// <param name="memberType">The member type.</param>
    /// <returns>An enumerable of sensitive property types.</returns>
    public static IEnumerable<IPropertyType> GetSensitivePropertyTypes(this IMemberType memberType)
        => memberType.CompositionPropertyTypes.Where(p => memberType.IsSensitiveProperty(p.Alias));

    /// <summary>
    /// Gets the aliases of all property types that are marked as sensitive.
    /// </summary>
    /// <param name="memberType">The member type.</param>
    /// <returns>An enumerable of sensitive property type aliases.</returns>
    public static IEnumerable<string> GetSensitivePropertyTypeAliases(this IMemberType memberType)
        => memberType.GetSensitivePropertyTypes().Select(p => p.Alias);
}
