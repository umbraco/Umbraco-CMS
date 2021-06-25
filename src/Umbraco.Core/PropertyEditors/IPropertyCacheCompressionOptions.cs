using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors
{
    public interface IPropertyCacheCompressionOptions
    {
        bool IsCompressed(IReadOnlyContentBase content, IPropertyType propertyType, IDataEditor dataEditor);
    }
}
