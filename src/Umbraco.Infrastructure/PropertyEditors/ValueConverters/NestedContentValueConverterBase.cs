// Copyright (c) Umbraco.
// See LICENSE for more details.

using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

public abstract class NestedContentValueConverterBase : PropertyValueConverterBase
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

    protected NestedContentValueConverterBase(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IPublishedModelFactory publishedModelFactory)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        PublishedModelFactory = publishedModelFactory;
    }

    protected IPublishedModelFactory PublishedModelFactory { get; }

    public static bool IsNested(IPublishedPropertyType publishedProperty)
        => publishedProperty.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.NestedContent);

    public static bool IsNestedSingle(IPublishedPropertyType publishedProperty)
        => IsNested(publishedProperty) && IsSingle(publishedProperty);

    private static bool IsSingle(IPublishedPropertyType publishedProperty)
    {
        NestedContentConfiguration? config = publishedProperty.DataType.ConfigurationAs<NestedContentConfiguration>();

        return config is not null && config.MinItems == 1 && config.MaxItems == 1;
    }

    public static bool IsNestedMany(IPublishedPropertyType publishedProperty)
        => IsNested(publishedProperty) && !IsSingle(publishedProperty);

    protected IPublishedElement? ConvertToElement(JObject sourceObject, PropertyCacheLevel referenceCacheLevel, bool preview)
    {
        var elementTypeAlias =
            sourceObject[NestedContentPropertyEditor.ContentTypeAliasPropertyKey]?.ToObject<string>();
        if (string.IsNullOrEmpty(elementTypeAlias))
        {
            return null;
        }

        IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();

        // Only convert element types - content types will cause an exception when PublishedModelFactory creates the model
        IPublishedContentType? publishedContentType = publishedSnapshot.Content?.GetContentType(elementTypeAlias);
        if (publishedContentType is null || publishedContentType.IsElement == false)
        {
            return null;
        }

        Dictionary<string, object?>? propertyValues = sourceObject.ToObject<Dictionary<string, object?>>();
        if (propertyValues is null || !propertyValues.TryGetValue("key", out var keyo) ||
            !Guid.TryParse(keyo?.ToString(), out Guid key))
        {
            key = Guid.Empty;
        }

        IPublishedElement element = new PublishedElement(publishedContentType, key, propertyValues, preview, referenceCacheLevel, _publishedSnapshotAccessor);
        element = PublishedModelFactory.CreateModel(element);

        return element;
    }
}
