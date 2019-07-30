﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class ColorPickerValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.ColorPicker);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => UseLabel(propertyType) ? typeof(PickedColor) : typeof(string);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
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

        private bool UseLabel(IPublishedPropertyType propertyType)
        {
            return ConfigurationEditor.ConfigurationAs<ColorPickerConfiguration>(propertyType.DataType.Configuration).UseLabel;
        }

        public class PickedColor
        {
            public PickedColor(string color, string label)
            {
                Color = color;
                Label = label;
            }

            public string Color { get; }
            public string Label { get; }

            public override string ToString() => Color;
        }
    }
}
