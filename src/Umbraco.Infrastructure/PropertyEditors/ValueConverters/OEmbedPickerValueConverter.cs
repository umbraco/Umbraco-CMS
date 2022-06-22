using Newtonsoft.Json;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class OEmbedPickerValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.OEmbedPicker);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => IsMultipleDataType(propertyType.DataType)
                ? typeof(IEnumerable<OEmbedItem>)
                : typeof(OEmbedItem);
        public override bool? IsValue(object? value, PropertyValueLevel level) => value?.ToString() != "[]";

        public override object? ConvertSourceToIntermediate(
            IPublishedElement owner,
            IPublishedPropertyType propertyType,
            object? source,
            bool preview) =>
            source?.ToString();

        public override object? ConvertIntermediateToObject(
            IPublishedElement owner,
            IPublishedPropertyType propertyType,
            PropertyCacheLevel referenceCacheLevel,
            object? inter,
            bool preview)
        {
            var isMultiple = IsMultipleDataType(propertyType.DataType);

            if (string.IsNullOrWhiteSpace(inter?.ToString()))
            {
                return isMultiple ? Enumerable.Empty<OEmbedItem>() : null;
            }

            var items = JsonConvert.DeserializeObject<List<OEmbedItem>>(inter.ToString() ?? string.Empty);

            return isMultiple ? items : items?.FirstOrDefault();
        }

        private bool IsMultipleDataType(PublishedDataType dataType) => dataType.ConfigurationAs<OEmbedPickerConfiguration>()?.Multiple ?? false;
    }
}
