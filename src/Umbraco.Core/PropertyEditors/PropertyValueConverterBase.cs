using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Provides a default overridable implementation for <see cref="IPropertyValueConverter"/> that does nothing.
    /// </summary>
    class PropertyValueConverterBase : IPropertyValueConverter
    {
        public virtual bool IsSourceToObjectConverter(Models.PublishedContent.PublishedPropertyType propertyType)
        {
            return false;
        }

        public virtual object ConvertSourceToObject(Models.PublishedContent.PublishedPropertyType propertyType, object source, bool preview)
        {
            return null;
        }

        public virtual bool IsDataToSourceConverter(Models.PublishedContent.PublishedPropertyType propertyType)
        {
            return false;
        }

        public virtual object ConvertDataToSource(Models.PublishedContent.PublishedPropertyType propertyType, object source, bool preview)
        {
            return null;
        }

        public virtual bool IsSourceToXPathConverter(Models.PublishedContent.PublishedPropertyType propertyType)
        {
            return false;
        }

        public virtual object ConvertSourceToXPath(Models.PublishedContent.PublishedPropertyType propertyType, object source, bool preview)
        {
            return null;
        }
    }
}
