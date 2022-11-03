using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Compress large, non published text properties
/// </summary>
public class UnPublishedContentPropertyCacheCompressionOptions : IPropertyCacheCompressionOptions
{
    public bool IsCompressed(IReadOnlyContentBase content, IPropertyType propertyType, IDataEditor dataEditor, bool published)
    {
        if (!published && propertyType.SupportsPublishing && propertyType.ValueStorageType == ValueStorageType.Ntext)
        {
            // Only compress non published content that supports publishing and the property is text
            return true;
        }

        if (propertyType.ValueStorageType == ValueStorageType.Integer &&
            Constants.PropertyEditors.Aliases.Boolean.Equals(dataEditor.Alias))
        {
            // Compress boolean values from int to bool
            return true;
        }

        return false;
    }
}
