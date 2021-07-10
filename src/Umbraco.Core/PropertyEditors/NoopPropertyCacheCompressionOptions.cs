using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Default implementation for <see cref="IPropertyCacheCompressionOptions"/> which does not compress any property data
    /// </summary>
    internal class NoopPropertyCacheCompressionOptions : IPropertyCacheCompressionOptions
    {
        public bool IsCompressed(IReadOnlyContentBase content, PropertyType propertyType, IDataEditor dataEditor, bool published) => false;
    }
}
