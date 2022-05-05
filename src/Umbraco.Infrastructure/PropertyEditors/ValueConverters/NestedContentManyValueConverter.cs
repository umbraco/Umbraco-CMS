// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <inheritdoc />
/// <summary>
///     Provides an implementation for <see cref="T:Umbraco.Core.PropertyEditors.IPropertyValueConverter" /> for nested
///     content.
/// </summary>
[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public class NestedContentManyValueConverter : NestedContentValueConverterBase
{
    private readonly IProfilingLogger _proflog;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NestedContentManyValueConverter" /> class.
    /// </summary>
    public NestedContentManyValueConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IPublishedModelFactory publishedModelFactory,
        IProfilingLogger proflog)
        : base(publishedSnapshotAccessor, publishedModelFactory)
        => _proflog = proflog;

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => IsNestedMany(propertyType);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
    {
        NestedContentConfiguration.ContentType[]? contentTypes =
            propertyType.DataType.ConfigurationAs<NestedContentConfiguration>()?.ContentTypes;

        return contentTypes?.Length == 1
            ? typeof(IEnumerable<>).MakeGenericType(ModelType.For(contentTypes[0].Alias))
            : typeof(IEnumerable<IPublishedElement>);
    }

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => source?.ToString();

    /// <inheritdoc />
    public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        using (_proflog.DebugDuration<NestedContentManyValueConverter>(
                   $"ConvertPropertyToNestedContent ({propertyType.DataType.Id})"))
        {
            NestedContentConfiguration? configuration =
                propertyType.DataType.ConfigurationAs<NestedContentConfiguration>();
            NestedContentConfiguration.ContentType[]? contentTypes = configuration?.ContentTypes;
            IList elements = contentTypes?.Length == 1
                ? PublishedModelFactory.CreateModelList(contentTypes[0].Alias)!
                : new List<IPublishedElement>();

            var value = (string?)inter;
            if (string.IsNullOrWhiteSpace(value))
            {
                return elements;
            }

            List<JObject>? objects = JsonConvert.DeserializeObject<List<JObject>>(value);
            if (objects is null || objects.Count == 0)
            {
                return elements;
            }

            foreach (JObject sourceObject in objects)
            {
                IPublishedElement? element = ConvertToElement(sourceObject, referenceCacheLevel, preview);
                if (element != null)
                {
                    elements.Add(element);
                }
            }

            return elements;
        }
    }
}
