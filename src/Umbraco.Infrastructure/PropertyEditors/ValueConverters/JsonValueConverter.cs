// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     The default converter for all property editors that expose a JSON value type
/// </summary>
/// <remarks>
///     Since this is a default (umbraco) converter it will be ignored if another converter found conflicts with this one.
/// </remarks>
[DefaultPropertyValueConverter]
public class JsonValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly ILogger<JsonValueConverter> _logger;
    private readonly PropertyEditorCollection _propertyEditors;

    private readonly string[] _excludedPropertyEditors = { Constants.PropertyEditors.Aliases.MediaPicker3 };

    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonValueConverter" /> class.
    /// </summary>
    public JsonValueConverter(PropertyEditorCollection propertyEditors, ILogger<JsonValueConverter> logger)
    {
        _propertyEditors = propertyEditors;
        _logger = logger;
    }

    /// <summary>
    ///     It is a converter for any value type that is "JSON"
    ///     Unless it's in the Excluded Property Editors list
    ///     The new MediaPicker 3 stores JSON but we want to use its own ValueConvertor
    /// </summary>
    /// <param name="propertyType">The published property type to check.</param>
    /// <returns>True if this converter can convert the property type.</returns>
    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        _propertyEditors.TryGet(propertyType.EditorAlias, out IDataEditor? editor)
        && editor.GetValueEditor().ValueType.InvariantEquals(ValueTypes.Json)
        && _excludedPropertyEditors.Contains(propertyType.EditorAlias) == false;

    /// <summary>
    /// Gets the type of the property value for the given published property type.
    /// This implementation always returns <see cref="JsonDocument"/> as the property value type.
    /// </summary>
    /// <remarks>We return a JsonDocument here because it's readonly and faster than JsonNode.</remarks>
    /// <param name="propertyType">The published property type.</param>
    /// <returns>The <see cref="Type"/> representing <see cref="JsonDocument"/>.</returns>
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(JsonDocument);

    /// <summary>
    /// Gets the property cache level for the specified property type.
    /// </summary>
    /// <param name="propertyType">The property type for which to determine the cache level.</param>
    /// <returns>Always returns <see cref="PropertyCacheLevel.Element"/>.</returns>
    /// <remarks>
    /// This method overrides the base implementation and always returns <see cref="PropertyCacheLevel.Element"/> regardless of the property type.
    /// </remarks>
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <summary>
    /// Converts the source value to an intermediate representation suitable for further processing.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">The type of the published property.</param>
    /// <param name="source">The source value to convert.</param>
    /// <param name="preview">Indicates whether the conversion is for preview mode.</param>
    /// <returns>
    /// If <paramref name="source"/> is a JSON string, returns a <see cref="JsonDocument"/>; if not, returns the original string; returns <c>null</c> if <paramref name="source"/> is <c>null</c>.
    /// </returns>
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source == null)
        {
            return null;
        }

        var sourceString = source.ToString()!;

        if (sourceString.DetectIsJson())
        {
            try
            {
                return JsonDocument.Parse(sourceString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not parse the string '{JsonString}' to a json object", sourceString);
            }
        }

        // it's not json, just return the string
        return sourceString;
    }

    /// <summary>
    /// Determines the appropriate cache level for a property when accessed via the delivery API.
    /// </summary>
    /// <param name="propertyType">The published property type for which to retrieve the cache level.</param>
    /// <returns>The <see cref="PropertyCacheLevel"/> to be used for the specified property type.</returns>
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType)
        => GetPropertyCacheLevel(propertyType);

    /// <summary>
    /// Returns the .NET type used to represent the property value for the Delivery API, based on the specified published property type.
    /// </summary>
    /// <param name="propertyType">The published property type for which to determine the Delivery API value type.</param>
    /// <returns>The <see cref="Type"/> representing the Delivery API property value, typically <see cref="JsonNode"/>.</returns>
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(JsonNode);

    /// <summary>
    /// Converts an intermediate JSON value, represented as a <see cref="JsonDocument"/>, to a <see cref="JsonNode"/> object suitable for use by the Delivery API.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">Metadata describing the property type.</param>
    /// <param name="referenceCacheLevel">The cache level for property references.</param>
    /// <param name="inter">The intermediate value to convert; expected to be a <see cref="JsonDocument"/>.</param>
    /// <param name="preview">True if the conversion is for preview mode; otherwise, false.</param>
    /// <param name="expanding">True if nested properties are being expanded during conversion; otherwise, false.</param>
    /// <returns>
    /// A <see cref="JsonNode"/> representing the converted value for the Delivery API, or <c>null</c> if <paramref name="inter"/> is not a <see cref="JsonDocument"/>.
    /// </returns>
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
        => inter is not JsonDocument jsonDocument
            ? null
            : JsonNode.Parse(jsonDocument.RootElement.ToString());
}
