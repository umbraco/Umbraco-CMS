// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter]
public class ColorPickerValueConverter : PropertyValueConverterBase
{
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.ColorPicker);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => UseLabel(propertyType) ? typeof(PickedColor) : typeof(string);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        bool useLabel = UseLabel(propertyType);

        if (source == null)
        {
            return useLabel ? null : string.Empty;
        }

        string ssource = source.ToString()!;
        if (ssource.DetectIsJson())
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(ssource))
                {
                    JsonElement json = doc.RootElement;
                    if (useLabel)
                    {
                        return new PickedColor(json.GetProperty("value").GetString()!, json.GetProperty("label").GetString()!);
                    }

                    return json.GetProperty("value").GetString()!;
                }
            }
            catch
            {
                /* not json finally */
            }
        }

        if (useLabel)
        {
            return new PickedColor(ssource, ssource);
        }

        return ssource;
    }

    private bool UseLabel(IPublishedPropertyType propertyType) => ConfigurationEditor
        .ConfigurationAs<ColorPickerConfiguration>(propertyType.DataType.Configuration)?.UseLabel ?? false;

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
