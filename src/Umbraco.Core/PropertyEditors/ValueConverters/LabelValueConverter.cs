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
    [PropertyValueType(typeof (string))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    public class LabelValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return Constants.PropertyEditors.NoEditAlias.Equals(propertyType.PropertyEditorAlias);
        }
        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            return source == null ? string.Empty : source.ToString();
        }
    }
}