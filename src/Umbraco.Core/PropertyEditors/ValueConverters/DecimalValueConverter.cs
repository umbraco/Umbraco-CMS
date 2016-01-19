using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [PropertyValueType(typeof(decimal))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    public class DecimalValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return Constants.PropertyEditors.DecimalAlias.Equals(propertyType.PropertyEditorAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return 0M;

            // in XML a decimal is a string
            var sourceString = source as string;
            if (sourceString != null)
            {
                decimal d;
                return (decimal.TryParse(sourceString, out d)) ? d : 0M;
            }

            // in the database an a decimal is an a decimal 
            // default value is zero
            return (source is decimal) ? source : 0M;
        }
    }
}
