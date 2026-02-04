// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for color picker properties.
/// </summary>
[DefaultPropertyValueConverter]
public class ColorPickerValueConverter : PropertyValueConverterBase
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ColorPickerValueConverter" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public ColorPickerValueConverter(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.ColorPicker);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(PickedColor);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source is null)
        {
            return null;
        }

        var value = source.ToString()!;
        if (value.DetectIsJson())
        {
            PickedColor? convertedValue = _jsonSerializer.Deserialize<PickedColor>(value);
            return convertedValue;
        }
        else
        {
            // This seems to be something old old where it may not be json at all.
            return new PickedColor(value, value);
        }
    }

    private bool UseLabel(IPublishedPropertyType propertyType) => ConfigurationEditor
        .ConfigurationAs<ColorPickerConfiguration>(propertyType.DataType.ConfigurationObject)?.UseLabel ?? false;

    /// <summary>
    ///     Represents a picked color from the color picker.
    /// </summary>
    public class PickedColor
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PickedColor" /> class.
        /// </summary>
        /// <param name="color">The color value.</param>
        /// <param name="label">The color label.</param>
        public PickedColor(string color, string label)
        {
            Color = color;
            Label = label;
        }

        /// <summary>
        ///     Gets the color value.
        /// </summary>
        [JsonPropertyName("value")]
        public string Color { get; }

        /// <summary>
        ///     Gets the color label.
        /// </summary>
        public string Label { get; }

        /// <inheritdoc />
        public override string ToString() => Color;
    }
}
