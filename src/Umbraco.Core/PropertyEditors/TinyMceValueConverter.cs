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
    internal class TinyMceValueConverter : PropertyValueConverterBase
	{
        public override bool IsDataToSourceConverter(PublishedPropertyType propertyType)
        {
            return Guid.Parse(Constants.PropertyEditors.TinyMCEv3).Equals(propertyType.PropertyEditorGuid);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            // in xml a string is: string
            // in the database a string is: string
            // default value is: null
            return source;
        }

        public override bool IsSourceToObjectConverter(PublishedPropertyType propertyType)
        {
            return IsDataToSourceConverter(propertyType);
        }

        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            // source should come from ConvertSource and be a string (or null) already
            return new HtmlString(source == null ? string.Empty : (string)source);
        }

        public override bool IsSourceToXPathConverter(PublishedPropertyType propertyType)
        {
            return IsDataToSourceConverter(propertyType);
        }

        public override object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
        {
            // source should come from ConvertSource and be a string (or null) already
            return source;
        }
    }
}