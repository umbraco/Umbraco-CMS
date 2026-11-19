using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
/// A service for converting <see cref="BlockItemData"/> into <see cref="IPublishedElement"/>.
/// </summary>
public interface IBlockElementService
{
    /// <summary>
    /// Creates an <see cref="IPublishedElement"/> instance from <see cref="BlockItemData"/>.
    /// </summary>
    /// <param name="owner">The <see cref="IPublishedElement"/> that contains the block property which is the origin to the <see cref="BlockItemData"/>.</param>
    /// <param name="blockItemData">The <see cref="BlockItemData"/> containing the data to convert into an <see cref="IPublishedElement"/>.</param>
    /// <param name="preview">Whether to perform the conversion for preview.</param>
    /// <returns>The created <see cref="IPublishedElement"/>, or null if an element could not be created from the <see cref="BlockItemData"/>.</returns>
    Task<IPublishedElement?> BuildElementAsync(IPublishedElement owner, BlockItemData blockItemData, bool? preview = null);
}
