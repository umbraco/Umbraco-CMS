using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Provides a default overridable implementation for <see cref="IPropertyValueConverter"/> that does nothing.
    /// </summary>
    class PropertyValueConverterBase : IPropertyValueConverter
    {
        public virtual bool IsDataToSourceConverter(PublishedPropertyType propertyType)
        {
            return false;
        }

        public virtual object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            return PublishedPropertyType.ConvertUsingDarkMagic(source);
        }

        public virtual bool IsSourceToObjectConverter(PublishedPropertyType propertyType)
        {
            return false;
        }

        public virtual object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            return source;
        }

        public virtual bool IsSourceToXPathConverter(PublishedPropertyType propertyType)
        {
            return false;
        }

        public virtual object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
        {
            return source.ToString();
        }
    }
}
