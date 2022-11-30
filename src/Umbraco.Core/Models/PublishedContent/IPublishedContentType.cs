namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Represents an <see cref="IPublishedElement" /> type.
/// </summary>
/// <remarks>
///     Instances implementing the <see cref="IPublishedContentType" /> interface should be
///     immutable, ie if the content type changes, then a new instance needs to be created.
/// </remarks>
public interface IPublishedContentType
{
    /// <summary>
    ///     Gets the unique key for the content type.
    /// </summary>
    Guid Key { get; }

    /// <summary>
    ///     Gets the content type identifier.
    /// </summary>
    int Id { get; }

    /// <summary>
    ///     Gets the content type alias.
    /// </summary>
    string Alias { get; }

    /// <summary>
    ///     Gets the content item type.
    /// </summary>
    PublishedItemType ItemType { get; }

    /// <summary>
    ///     Gets the aliases of the content types participating in the composition.
    /// </summary>
    HashSet<string> CompositionAliases { get; }

    /// <summary>
    ///     Gets the content variations of the content type.
    /// </summary>
    ContentVariation Variations { get; }

    /// <summary>
    ///     Gets a value indicating whether this content type is for an element.
    /// </summary>
    bool IsElement { get; }

    /// <summary>
    ///     Gets the content type properties.
    /// </summary>
    IEnumerable<IPublishedPropertyType> PropertyTypes { get; }

    /// <summary>
    ///     Gets a property type index.
    /// </summary>
    /// <remarks>The alias is case-insensitive. This is the only place where alias strings are compared.</remarks>
    int GetPropertyIndex(string alias);

    /// <summary>
    ///     Gets a property type.
    /// </summary>
    IPublishedPropertyType? GetPropertyType(string alias);

    /// <summary>
    ///     Gets a property type.
    /// </summary>
    IPublishedPropertyType? GetPropertyType(int index);
}
