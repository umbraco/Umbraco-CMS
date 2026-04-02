using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Cache.PropertyEditors;

/// <summary>
/// Represents a cache responsible for storing and retrieving element types used by the block editor in Umbraco.
/// This cache helps optimize access to element type definitions, improving performance when working with block editor data structures.
/// </summary>
public interface IBlockEditorElementTypeCache
{
    /// <summary>
    /// Retrieves the content types corresponding to the specified keys.
    /// </summary>
    /// <param name="keys">A collection of unique identifiers (keys) for the content types to retrieve.</param>
    /// <returns>An enumerable collection of <see cref="IContentType"/> instances that match the provided keys. Only content types with matching keys are returned.</returns>
    IEnumerable<IContentType> GetMany(IEnumerable<Guid> keys);

    /// <summary>
    /// Gets all block editor element content types.
    /// </summary>
    /// <returns>An enumerable of all <see cref="Umbraco.Cms.Core.Models.IContentType"/> instances.</returns>
    IEnumerable<IContentType> GetAll();

    /// <summary>
    /// Clears all cached block editor element types.
    /// </summary>
    void ClearAll() { }
}
