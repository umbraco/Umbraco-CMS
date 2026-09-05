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
///     Since this is a default converter, it is ignored when a non-default converter also applies. Other default converters
///     for property editors with a JSON value type must declare that they shadow this one.
/// </remarks>
[DefaultPropertyValueConverter]
public class JsonValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly ILogger<JsonValueConverter> _logger;
    private readonly PropertyEditorCollection _propertyEditors;

    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonValueConverter" /> class.
    /// </summary>
    public JsonValueConverter(PropertyEditorCollection propertyEditors, ILogger<JsonValueConverter> logger)
    {
        _propertyEditors = propertyEditors;
        _logger = logger;
    }

    /// <inheritdoc/>
    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        _propertyEditors.TryGet(propertyType.EditorAlias, out IDataEditor? editor)
        && editor.GetValueEditor().ValueType.InvariantEquals(ValueTypes.Json);

    /// <inheritdoc/>
    /// <remarks>A <see cref="JsonDocument"/> is returned because it is read-only and faster than <see cref="JsonNode"/>.</remarks>
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(JsonDocument);

    /// <inheritdoc/>
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc/>
    /// <remarks>
    /// A source value that is not detected as JSON is passed through as a string rather than discarded, so the property
    /// still yields its stored value when the editor holds non-JSON content.
    /// </remarks>
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

        return sourceString;
    }

    /// <inheritdoc/>
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType)
        => GetPropertyCacheLevel(propertyType);

    /// <inheritdoc/>
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(JsonNode);

    /// <inheritdoc/>
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
        => inter is not JsonDocument jsonDocument
            ? null
            : JsonNode.Parse(jsonDocument.RootElement.ToString());
}
