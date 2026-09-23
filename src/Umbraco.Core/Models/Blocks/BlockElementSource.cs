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
    ///     Gets the block's values as the owner's draft holds them.
    /// </summary>
    public required IReadOnlyList<BlockPropertyValue> DraftValues { get; init; }

    /// <summary>
    ///     Gets the block's values as the owner's published version holds them, or <c>null</c> when it holds
    ///     no such block.
    /// </summary>
    public IReadOnlyList<BlockPropertyValue>? PublishedValues { get; init; }

    /// <summary>
    ///     Gets the cultures the block is currently visible in, which the new element should be published in.
    /// </summary>
    /// <remarks>
    ///     Empty when the block is not live anywhere, in which case the new element is created entirely as a
    ///     draft from <see cref="DraftValues" />.
    /// </remarks>
    public required IReadOnlyList<string?> LiveCultures { get; init; }
}
