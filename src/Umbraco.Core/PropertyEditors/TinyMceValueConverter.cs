using System;
using System.Web;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors
{
	/// <summary>
	/// Value converter for the RTE so that it always returns IHtmlString so that Html.Raw doesn't have to be used.
	/// </summary>
    // PropertyCacheLevel.Content is ok here because that version of RTE converter does not parse {locallink} nor executes macros
    [PropertyValueType(typeof(IHtmlString))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    internal class TinyMceValueConverter : IPropertyValueConverter
	{
        public bool IsDataToSourceConverter(PublishedPropertyType propertyType)
        {
            return Guid.Parse(Constants.PropertyEditors.TinyMCEv3).Equals(propertyType.EditorGuid);
        }

        public virtual object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            // in xml a string is: string
            // in the database a string is: string
            // default value is: null
            return source;
        }

        public bool IsSourceToObjectConverter(PublishedPropertyType propertyType)
        {
            return IsDataToSourceConverter(propertyType);
        }

        public virtual object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            // source should come from ConvertSource and be a string already
            return new HtmlString((string)source);
        }

        public bool IsSourceToXPathConverter(PublishedPropertyType propertyType)
        {
            return IsDataToSourceConverter(propertyType);
        }

        public virtual object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
        {
            // source should come from ConvertSource and be a string already
            return source;
        }
    }
}