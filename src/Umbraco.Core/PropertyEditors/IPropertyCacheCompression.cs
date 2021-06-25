using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Determines if a property type's value should be compressed in memory
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public interface IPropertyCacheCompression
    {
        bool IsCompressed(IReadOnlyContentBase content, string propertyTypeAlias);
    }
}
