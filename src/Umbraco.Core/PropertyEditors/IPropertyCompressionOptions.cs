using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Determines if a property type's value should be compressed
    /// </summary>
    public interface IPropertyCompressionOptions
    {        
        bool IsCompressed(int contentTypeId, string propertyTypeAlias);
    }
}
