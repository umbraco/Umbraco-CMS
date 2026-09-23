namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a request to create an Element Library element from a block held in a content item's
///     property, reproducing the cultures the block is currently live in.
/// </summary>
/// <remarks>
///     Everything about the element itself is read from the owner's stored property values; only the name of
///     the new Library element is supplied by the caller.
/// </remarks>
public class CreateElementFromBlockModel
{
    /// <summary>
    ///     Gets or sets the key of the content item (document, media item or member) that owns the property the
    ///     element is nested in.
    /// </summary>
    public required Guid OwnerKey { get; set; }

    /// <summary>
    ///     Gets or sets the key of the block to create the element from.
    /// </summary>
    public required Guid BlockKey { get; set; }

    /// <summary>
    ///     Gets or sets the key to give the new element, or <c>null</c> to have one assigned.
    /// </summary>
    public Guid? Key { get; set; }

    /// <summary>
    ///     Gets or sets the key of the Library folder to create the new element in, or <c>null</c> for the root.
    /// </summary>
    public Guid? ParentKey { get; set; }

    /// <summary>
    ///     Gets or sets the name to give the new element.
    /// </summary>
    public required string Name { get; set; }
}
