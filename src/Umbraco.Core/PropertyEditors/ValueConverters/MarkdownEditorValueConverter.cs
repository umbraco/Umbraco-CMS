using System;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Strings;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class MarkdownEditorValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => Constants.PropertyEditors.Aliases.MarkdownEditor.Equals(propertyType.EditorAlias);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof (IHtmlEncodedString);

        // PropertyCacheLevel.Content is ok here because that converter does not parse {locallink} nor executes macros
        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            // in xml a string is: string
            // in the database a string is: string
            // default value is: null
            return source;
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            // source should come from ConvertSource and be a string (or null) already
            return new HtmlEncodedString(inter == null ? string.Empty : (string) inter);
        }

        public override object ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            // source should come from ConvertSource and be a string (or null) already
            return inter?.ToString() ?? string.Empty;
        }
    }
}
