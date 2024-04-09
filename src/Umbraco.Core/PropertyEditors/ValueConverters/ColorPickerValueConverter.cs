// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter]
public class ColorPickerValueConverter : PropertyValueConverterBase
{
    private readonly IJsonSerializer _jsonSerializer;

    public ColorPickerValueConverter(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.ColorPicker);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => UseLabel(propertyType) ? typeof(PickedColor) : typeof(string);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        var useLabel = UseLabel(propertyType);

        if (source is null)
        {
            return useLabel ? null : string.Empty;
        }

        var value = source.ToString()!;
        if (value.DetectIsJson())
        {
            PickedColor? convertedValue = _jsonSerializer.Deserialize<PickedColor>(value);
            return useLabel ? convertedValue : convertedValue?.Color;
        }

        // This seems to be something old old where it may not be json at all.
        if (useLabel)
        {
            return new PickedColor(value, value);
        }

        return value;
    }

    private bool UseLabel(IPublishedPropertyType propertyType) => ConfigurationEditor
        .ConfigurationAs<ColorPickerConfiguration>(propertyType.DataType.ConfigurationObject)?.UseLabel ?? false;

    public class PickedColor
    {
        public PickedColor(string color, string label)
        {
            Color = color;
            Label = label;
        }

        [JsonPropertyName("value")]
        public string Color { get; }

        public string Label { get; }

        public override string ToString() => Color;
    }
}
