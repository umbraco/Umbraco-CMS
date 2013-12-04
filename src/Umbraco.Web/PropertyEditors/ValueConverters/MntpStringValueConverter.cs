using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// Ensures that no matter what is selected in MNTP that the value results in a string
    /// </summary>
    /// <remarks>
    /// See here for full details:http://issues.umbraco.org/issue/U4-3776
    /// </remarks>
    [DefaultPropertyValueConverter]
    [PropertyValueType(typeof (string))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Request)]
    public class MntpStringValueConverter : PropertyValueConverterBase
    {        
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias == Constants.PropertyEditors.MultiNodeTreePickerAlias;
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            return source.ToString();
        }
    }
}