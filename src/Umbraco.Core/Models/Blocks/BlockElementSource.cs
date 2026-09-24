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

    /// <summary>
    ///     Gets the variations the block has been created for.
    /// </summary>
    /// <remarks>
    ///     Which variations a block exists in is what it is exposed in, which is not the same as what it holds
    ///     values for - a block created for a culture and left empty is still created for it.
    /// </remarks>
    public required IReadOnlyList<BlockItemVariation> Variations { get; init; }
}
