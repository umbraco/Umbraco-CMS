using System.Linq;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    /// <summary>
    /// Ensures that no matter what is selected in (editor), the value results in a string.
    /// </summary>
    /// <remarks>
    /// <para>For more details see issues http://issues.umbraco.org/issue/U4-3776 (MNTP)
    /// and http://issues.umbraco.org/issue/U4-4160 (media picker).</para>
    /// <para>The cache level is set to .Content because the string is supposed to depend
    /// on the source value only, and not on any other content. It is NOT appropriate
    /// to use that converter for values whose .ToString() would depend on other content.</para>
    /// </remarks>
    [DefaultPropertyValueConverter]
    [PropertyValueType(typeof(string))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    public class MustBeStringValueConverter : PropertyValueConverterBase
    {
        private static readonly string[] Aliases =
        {
            Constants.PropertyEditors.MultiNodeTreePickerAlias,
            Constants.PropertyEditors.MultipleMediaPickerAlias
        };

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return Aliases.Contains(propertyType.PropertyEditorAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            return source.ToString();
        }
    }
}
