using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class ColorPickerValueConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
	        if (UmbracoConfig.For.UmbracoSettings().Content.EnablePropertyValueConverters)
	        {
				return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.ColorPickerAlias);
			}
	        return false;
        }

        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return UseLabel(propertyType) ? typeof(PickedColor) : typeof(string);
        }

        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType, PropertyCacheValue cacheValue)
        {
            return PropertyCacheLevel.Content;
        }

        private bool UseLabel(PublishedPropertyType propertyType)
        {
            var preValues = ApplicationContext.Current.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(propertyType.DataTypeId);
            PreValue preValue;
            return preValues.PreValuesAsDictionary.TryGetValue("useLabel", out preValue) && preValue.Value == "1";
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            var useLabel = UseLabel(propertyType);

            if (source == null) return useLabel ? null : string.Empty;

            var ssource = source.ToString();
            if (ssource.DetectIsJson())
            {
                try
                {
                    var jo = JsonConvert.DeserializeObject<JObject>(ssource);
                    if (useLabel) return new PickedColor(jo["value"].ToString(), jo["label"].ToString());
                    return jo["value"].ToString();
                }
                catch { /* not json finally */ }
            }

            if (useLabel) return new PickedColor(ssource, ssource);
            return ssource;
        }

        public class PickedColor
        {
            public PickedColor(string color, string label)
            {
                Color = color;
                Label = label;
            }

            public string Color { get; private set; }
            public string Label { get; private set; }

            public override string ToString()
            {
                return Color;
            }
        }
    }
}
