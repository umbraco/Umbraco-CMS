using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    /// <summary>
    /// We need this property converter so that we always force the value of a label to be a string
    /// </summary>
    /// <remarks>
    /// Without a property converter defined for the label type, the value will be converted with
    /// the `ConvertUsingDarkMagic` method which will try to parse the value into it's correct type, but this
    /// can cause issues if the string is detected as a number and then strips leading zeros.
    /// Example: http://issues.umbraco.org/issue/U4-7929
    /// </remarks>
    [DefaultPropertyValueConverter]
    public class LabelValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
            => Constants.PropertyEditors.Aliases.NoEdit.Equals(propertyType.EditorAlias);

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (string);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            return source?.ToString() ?? string.Empty;
        }
    }
}
