using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a collection of properties.
/// </summary>
public interface IPropertyCollection : IEnumerable<IProperty>, IDeepCloneable, INotifyCollectionChanged
{
    /// <summary>
    ///     Gets the number of properties in the collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    ///     Gets the property with the specified alias.
    /// </summary>
    IProperty? this[string name] { get; }

    /// <summary>
    ///     Gets the property at the specified index.
    /// </summary>
    IProperty? this[int index] { get; }

    /// <summary>
    ///     Tries to get a property by its alias.
    /// </summary>
    /// <param name="propertyTypeAlias">The alias of the property type.</param>
    /// <param name="property">When this method returns, contains the property if found; otherwise, null.</param>
    /// <returns><c>true</c> if a property with the specified alias was found; otherwise, <c>false</c>.</returns>
    bool TryGetValue(string propertyTypeAlias, [MaybeNullWhen(false)] out IProperty property);

    /// <summary>
    ///     Determines whether the collection contains a property with the specified alias.
    /// </summary>
    /// <param name="key">The alias of the property to locate.</param>
    /// <returns><c>true</c> if the collection contains a property with the specified alias; otherwise, <c>false</c>.</returns>
    bool Contains(string key);

    /// <summary>
    ///     Ensures that the collection contains properties for the specified property types.
    /// </summary>
    void EnsurePropertyTypes(IEnumerable<IPropertyType> propertyTypes);

    /// <summary>
    ///     Ensures that the collection does not contain properties not in the specified property types.
    /// </summary>
    void EnsureCleanPropertyTypes(IEnumerable<IPropertyType> propertyTypes);

    /// <summary>
    ///     Adds or updates a property.
    /// </summary>
    void Add(IProperty property);

    /// <summary>
    ///     Clears all collection changed event handlers.
    /// </summary>
    void ClearCollectionChangedEvents();
}
