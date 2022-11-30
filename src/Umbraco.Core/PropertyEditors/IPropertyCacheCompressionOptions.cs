using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

public interface IPropertyCacheCompressionOptions
{
    /// <summary>
    ///     Whether a property on the content is/should be compressed
    /// </summary>
    /// <param name="content">The content</param>
    /// <param name="propertyType">The property to compress or not</param>
    /// <param name="dataEditor">The datatype of the property to compress or not</param>
    /// <param name="published">Whether this content is the published version</param>
    bool IsCompressed(IReadOnlyContentBase content, IPropertyType propertyType, IDataEditor dataEditor, bool published);
}
