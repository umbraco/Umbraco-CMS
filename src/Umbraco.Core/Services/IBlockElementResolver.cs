using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Reads a block out of the property of a content item, and works out what the element it would become
///     should look like.
/// </summary>
/// <remarks>
///     The owner can be a document, a media item, a member or another element: block editors carry no
///     restriction on which tree they are used in, and a Library element holds block properties like any
///     other content. Everything is read from what the owner has stored; nothing comes from the caller.
/// </remarks>
internal interface IBlockElementResolver
{
    /// <summary>
    ///     Finds a block and describes the element it would make.
    /// </summary>
    /// <param name="ownerKey">The content item holding the property the block sits in.</param>
    /// <param name="blockKey">The block itself, which may be nested inside other blocks.</param>
    /// <returns>The block's content and live cultures, or the reason it could not be read.</returns>
    /// <remarks>
    ///     Which property holds the block is worked out here rather than named by the caller, since a nested
    ///     block sits in the value of the outermost property and a caller looking at it cannot name that.
    /// </remarks>
    Attempt<BlockElementSource, ElementCreateFromBlockOperationStatus> Resolve(Guid ownerKey, Guid blockKey);

    /// <summary>
    ///     Narrows the cultures a block is live in down to those that can actually be published on the element
    ///     built from it.
    /// </summary>
    /// <param name="element">The element built from the block's published values.</param>
    /// <param name="elementType">The element's content type.</param>
    /// <param name="liveCultures">The cultures the block is live in.</param>
    /// <remarks>
    ///     Publishing validates every culture together, so a culture that would fail takes the rest down with
    ///     it unless it is filtered out first. A subset that leaves a mandatory language uncovered would
    ///     unpublish the whole element, so that is treated as nothing being publishable at all.
    /// </remarks>
    Task<IReadOnlyCollection<string?>> ResolvePublishableCulturesAsync(
        IElement element,
        IContentType elementType,
        IEnumerable<string?> liveCultures);
}
