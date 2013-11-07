using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [PropertyValueType(typeof(int))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    public class IntegerValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return Constants.PropertyEditors.IntegerAlias.Equals(propertyType.PropertyEditorAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return 0;

            // in XML an integer is a string
            var sourceString = source as string;
            if (sourceString != null)
            {
                int i;
                return (int.TryParse(sourceString, out i)) ? i : 0;
            }

            // in the database an integer is an integer
            // default value is zero
            return (source is int) ? source : 0;
        }
    }
}
