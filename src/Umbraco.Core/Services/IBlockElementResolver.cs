using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Reads a block out of the property of a content item, and works out what the element it would become
///     should look like.
/// </summary>
/// <remarks>
///     The owner can be a document, media item, member or another element: block editors carry no
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
    /// <returns>The block's content, or the reason it could not be read.</returns>
    /// <remarks>
    ///     Which property holds the block is worked out here rather than named by the caller, since a nested
    ///     block sits in the value of the outermost property and a caller looking at it cannot name that.
    /// </remarks>
    Attempt<BlockElementSource, ElementCreateFromBlockOperationStatus> Resolve(Guid ownerKey, Guid blockKey);
}
