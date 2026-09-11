// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for dropdown list properties.
/// </summary>
/// <remarks>
///     There is one dropdown editor per number of values it holds, so that the type a dropdown property yields
///     follows from the editor rather than from the configuration of the data type it is used through. Both store the
///     same value - a JSON array - which is what this base reads.
/// </remarks>
public abstract class DropDownPropertyValueConverterBase : PropertyValueConverterBase
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DropDownPropertyValueConverterBase" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    protected DropDownPropertyValueConverterBase(IJsonSerializer jsonSerializer) => _jsonSerializer = jsonSerializer;

    /// <summary>
    ///     Gets a value indicating whether the editor this converter serves holds more than one value.
    /// </summary>
    protected abstract bool HoldsMultipleValues { get; }

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source is null)
        {
            return Array.Empty<string>();
        }

        var sourceString = source.ToString();

        return string.IsNullOrWhiteSpace(sourceString)
            ? Array.Empty<string>()
            : _jsonSerializer.Deserialize<string[]>(source.ToString()!) ?? Array.Empty<string>();
    }

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        if (inter is null)
        {
            return null;
        }

        var selectedValues = (string[])inter;
        if (selectedValues.Length > 0)
        {
            return HoldsMultipleValues
                ? selectedValues
                : selectedValues[0];
        }

        return HoldsMultipleValues
            ? inter
            : string.Empty;
    }
}
