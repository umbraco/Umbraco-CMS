// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <inheritdoc />
/// <summary>
///     Provides an implementation for <see cref="T:Umbraco.Core.PropertyEditors.IPropertyValueConverter" /> for nested
///     content.
/// </summary>
[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
[Obsolete("Nested content is obsolete, will be removed in V13")]
public class NestedContentSingleValueConverter : NestedContentValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IProfilingLogger _proflog;
    private readonly IApiElementBuilder _apiElementBuilder;

    [Obsolete("Use constructor that takes all parameters, scheduled for removal in V13")]
    public NestedContentSingleValueConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IPublishedModelFactory publishedModelFactory,
        IProfilingLogger proflog)
        : this(
            publishedSnapshotAccessor,
            publishedModelFactory,
            proflog,
            StaticServiceProvider.Instance.GetRequiredService<IApiElementBuilder>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NestedContentSingleValueConverter" /> class.
    /// </summary>
    public NestedContentSingleValueConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IPublishedModelFactory publishedModelFactory,
        IProfilingLogger proflog,
        IApiElementBuilder apiElementBuilder)
        : base(publishedSnapshotAccessor, publishedModelFactory)
    {
        _proflog = proflog;
        _apiElementBuilder = apiElementBuilder;
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => IsNestedSingle(propertyType);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
    {
        NestedContentConfiguration.ContentType[]? contentTypes =
            propertyType.DataType.ConfigurationAs<NestedContentConfiguration>()?.ContentTypes;

        return contentTypes?.Length == 1
            ? ModelType.For(contentTypes[0].Alias)
            : typeof(IPublishedElement);
    }

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => source?.ToString();

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        using (_proflog.DebugDuration<NestedContentSingleValueConverter>(
                   $"ConvertPropertyToNestedContent ({propertyType.DataType.Id})"))
        {
            var value = (string?)inter;
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            List<JObject>? objects = JsonConvert.DeserializeObject<List<JObject>>(value)!;
            if (objects.Count == 0)
            {
                return null;
            }

            // Only return the first (existing data might contain more than is currently configured)
            return ConvertToElement(objects[0], referenceCacheLevel, preview);
        }
    }

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => GetPropertyCacheLevel(propertyType);

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(IEnumerable<IApiElement>);

    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        var converted = ConvertIntermediateToObject(owner, propertyType, referenceCacheLevel, inter, preview);
        if (converted is not IPublishedElement element)
        {
            return Array.Empty<IApiElement>();
        }

        return new [] { _apiElementBuilder.Build(element) };
    }
}
