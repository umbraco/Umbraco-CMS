namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a tagged entity.
/// </summary>
/// <remarks>
///     Note that it is the properties of an entity (like Content, Media, Members, etc.) that are tagged,
///     which is why this class is composed of a list of tagged properties and the identifier the actual entity.
/// </remarks>
public class TaggedEntity
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TaggedEntity" /> class.
    /// </summary>
    public TaggedEntity(int entityId, IEnumerable<TaggedProperty> taggedProperties)
    {
        EntityId = entityId;
        TaggedProperties = taggedProperties;
    }

    /// <summary>
    ///     Gets the identifier of the entity.
    /// </summary>
    public int EntityId { get; }

    /// <summary>
    ///     Gets the tagged properties.
    /// </summary>
    public IEnumerable<TaggedProperty> TaggedProperties { get; }
}
