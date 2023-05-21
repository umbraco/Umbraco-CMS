namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a tagged property on an entity.
/// </summary>
public class TaggedProperty
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TaggedProperty" /> class.
    /// </summary>
    public TaggedProperty(int propertyTypeId, string? propertyTypeAlias, IEnumerable<ITag> tags)
    {
        PropertyTypeId = propertyTypeId;
        PropertyTypeAlias = propertyTypeAlias;
        Tags = tags;
    }

    /// <summary>
    ///     Gets the identifier of the property type.
    /// </summary>
    public int PropertyTypeId { get; }

    /// <summary>
    ///     Gets the alias of the property type.
    /// </summary>
    public string? PropertyTypeAlias { get; }

    /// <summary>
    ///     Gets the tags.
    /// </summary>
    public IEnumerable<ITag> Tags { get; }
}
