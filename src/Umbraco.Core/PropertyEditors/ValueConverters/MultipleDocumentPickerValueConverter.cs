// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for multiple document picker properties.
/// </summary>
public class MultipleDocumentPickerValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IPublishedContentCache _publishedContentCache;
    private readonly IApiContentBuilder _apiContentBuilder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MultipleDocumentPickerValueConverter" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <param name="publishedContentCache">The published content cache.</param>
    /// <param name="apiContentBuilder">The API content builder.</param>
    public MultipleDocumentPickerValueConverter(
        IJsonSerializer jsonSerializer,
        IPublishedContentCache publishedContentCache,
        IApiContentBuilder apiContentBuilder)
    {
        _jsonSerializer = jsonSerializer;
        _publishedContentCache = publishedContentCache;
        _apiContentBuilder = apiContentBuilder;
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MultipleDocumentPicker);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IEnumerable<IPublishedContent>);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Elements;

    /// <inheritdoc />
    public override bool? IsValue(object? value, PropertyValueLevel level)
        => value is not null && value.ToString() != "[]";

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => source?.ToString();

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
        => GetDocuments(inter, preview);

    /// <inheritdoc />
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType)
        => GetPropertyCacheLevel(propertyType);

    /// <inheritdoc />
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevelForExpansion(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Snapshot;

    /// <inheritdoc />
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IEnumerable<IApiContent>);

    /// <inheritdoc />
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
        => GetDocuments(inter, preview).Select(_apiContentBuilder.Build).ToArray();

    private IPublishedContent[] GetDocuments(object? inter, bool preview)
    {
        var value = inter as string;
        if (value.IsNullOrWhiteSpace())
        {
            return [];
        }

        Guid[]? keys = _jsonSerializer.Deserialize<Guid[]>(value);

        return keys is null
            ? []
            : keys
                .Select(key => _publishedContentCache.GetById(preview, key))
                .WhereNotNull()
                .Where(content => content.ContentType.ItemType == PublishedItemType.Content)
                .ToArray();
    }
}
