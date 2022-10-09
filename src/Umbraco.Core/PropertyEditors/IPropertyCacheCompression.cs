using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Determines if a property type's value should be compressed in memory
/// </summary>
/// <remarks>
/// </remarks>
public interface IPropertyCacheCompression
{
    /// <summary>
    ///     Whether a property on the content is/should be compressed
    /// </summary>
    /// <param name="content">The content</param>
    /// <param name="propertyTypeAlias">The property to compress or not</param>
    /// <param name="published">Whether this content is the published version</param>
    bool IsCompressed(IReadOnlyContentBase content, string propertyTypeAlias, bool published);
}
