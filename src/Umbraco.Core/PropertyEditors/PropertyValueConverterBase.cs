using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Provides a default overridable implementation for <see cref="IPropertyValueConverter"/> that does nothing.
    /// </summary>
    public class PropertyValueConverterBase : IPropertyValueConverter
    {
        public virtual bool IsConverter(PublishedPropertyType propertyType)
        {
            return false;
        }

        public virtual object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            return PublishedPropertyType.ConvertUsingDarkMagic(source);
        }

        public virtual object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            return source;
        }

        public virtual object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
        {
            return source.ToString();
        }
    }
}
