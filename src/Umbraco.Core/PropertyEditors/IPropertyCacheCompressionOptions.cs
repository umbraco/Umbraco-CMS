using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    public interface IPropertyCacheCompressionOptions
    {
        bool IsCompressed(IReadOnlyContentBase content, PropertyType propertyType, IDataEditor dataEditor);
    }
}
