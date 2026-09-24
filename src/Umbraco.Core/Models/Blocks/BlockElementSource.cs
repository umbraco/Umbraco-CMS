namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Everything a block held in a content item's property says about the element it would become.
/// </summary>
internal sealed class BlockElementSource
{
    /// <summary>
    ///     Gets the key of the block's content type, which the new element takes.
    /// </summary>
    public required Guid ContentTypeKey { get; init; }

    /// <summary>
    ///     Gets the block's values as the owner has them stored.
    /// </summary>
    public required IReadOnlyList<BlockPropertyValue> Values { get; init; }
}
