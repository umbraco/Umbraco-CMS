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
    /// <param name="propertyType"></param>
    /// <returns></returns>
    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        _propertyEditors.TryGet(propertyType.EditorAlias, out IDataEditor? editor)
        && editor.GetValueEditor().ValueType.InvariantEquals(ValueTypes.Json)
        && _excludedPropertyEditors.Contains(propertyType.EditorAlias) == false;

    // We return a JsonDocument here because it's readonly and faster than JsonNode.
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(JsonDocument);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

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

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType)
        => GetPropertyCacheLevel(propertyType);

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(JsonNode);

    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
        => inter is not JsonDocument jsonDocument
            ? null
            : JsonNode.Parse(jsonDocument.RootElement.ToString());
}
