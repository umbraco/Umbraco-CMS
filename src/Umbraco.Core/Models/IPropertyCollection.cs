using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.Models;

public interface IPropertyCollection : IEnumerable<IProperty>, IDeepCloneable, INotifyCollectionChanged
{
    int Count { get; }

    /// <summary>
    ///     Gets the property with the specified alias.
    /// </summary>
    IProperty? this[string name] { get; }

    /// <summary>
    ///     Gets the property at the specified index.
    /// </summary>
    IProperty? this[int index] { get; }

    bool TryGetValue(string propertyTypeAlias, [MaybeNullWhen(false)] out IProperty property);

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

    void ClearCollectionChangedEvents();
}
