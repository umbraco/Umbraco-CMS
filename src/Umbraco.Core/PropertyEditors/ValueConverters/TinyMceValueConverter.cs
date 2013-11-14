using System;
using System.Web;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
	/// <summary>
	/// Value converter for the RTE so that it always returns IHtmlString so that Html.Raw doesn't have to be used.
	/// </summary>
    // PropertyCacheLevel.Content is ok here because that version of RTE converter does not parse {locallink} nor executes macros
    [PropertyValueType(typeof(IHtmlString))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    public class TinyMceValueConverter : PropertyValueConverterBase
	{
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias == Constants.PropertyEditors.TinyMCEAlias;
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            // in xml a string is: string
            // in the database a string is: string
            // default value is: null
            return source;
        }

        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            // source should come from ConvertSource and be a string (or null) already
            return new HtmlString(source == null ? string.Empty : (string)source);
        }

        public override object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
        {
            // source should come from ConvertSource and be a string (or null) already
            return source;
        }
    }
}