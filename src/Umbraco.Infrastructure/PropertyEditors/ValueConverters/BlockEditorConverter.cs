// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Converts JSON block objects into <see cref="IPublishedElement" />.
/// </summary>
public sealed class BlockEditorConverter
{
    private readonly IPublishedModelFactory _publishedModelFactory;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly BlockEditorVarianceHandler _blockEditorVarianceHandler;

    public BlockEditorConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IPublishedModelFactory publishedModelFactory,
        IVariationContextAccessor variationContextAccessor,
        BlockEditorVarianceHandler blockEditorVarianceHandler)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _publishedModelFactory = publishedModelFactory;
        _variationContextAccessor = variationContextAccessor;
        _blockEditorVarianceHandler = blockEditorVarianceHandler;
    }

    public IPublishedElement? ConvertToElement(IPublishedElement owner, BlockItemData data, PropertyCacheLevel referenceCacheLevel, bool preview)
    {
        IPublishedContentCache? publishedContentCache =
            _publishedSnapshotAccessor.GetRequiredPublishedSnapshot().Content;

        // Only convert element types - content types will cause an exception when PublishedModelFactory creates the model
        IPublishedContentType? publishedContentType = publishedContentCache?.GetContentType(data.ContentTypeKey);
        if (publishedContentType == null || publishedContentType.IsElement == false)
        {
            return null;
        }

        VariationContext variationContext = _variationContextAccessor.VariationContext ?? new VariationContext();

        var propertyTypesByAlias = publishedContentType
            .PropertyTypes
            .ToDictionary(propertyType => propertyType.Alias);

        var propertyValues = new Dictionary<string, object?>();
        foreach (BlockPropertyValue property in data.Values)
        {
            if (!propertyTypesByAlias.TryGetValue(property.Alias, out IPublishedPropertyType? propertyType))
            {
                continue;
            }

            // if case changes have been made to the element type variation since the parent content was published,
            // we need to contextualize those changes manually here - unlike for root level properties, where these
            // things are handled when a content type is saved (copies of property values are created in the DB).
            _blockEditorVarianceHandler.AlignPropertyVariance(property, propertyType, owner).GetAwaiter().GetResult();

            var culture = owner.ContentType.VariesByCulture() && publishedContentType.VariesByCulture() && propertyType.VariesByCulture()
                ? variationContext.Culture
                : null;
            var segment = owner.ContentType.VariesBySegment() && publishedContentType.VariesBySegment() && propertyType.VariesBySegment()
                ? variationContext.Segment
                : null;

            if (property.Culture.NullOrWhiteSpaceAsNull().InvariantEquals(culture.NullOrWhiteSpaceAsNull())
                && property.Segment.NullOrWhiteSpaceAsNull().InvariantEquals(segment.NullOrWhiteSpaceAsNull()))
            {
                propertyValues[property.Alias] = property.Value;
            }
        }

        // Get the key from the deserialized object. If this is empty, we can fallback to checking the 'key' if there is one
        Guid key = data.Key;
        if (key == Guid.Empty && propertyValues.TryGetValue("key", out var keyo))
        {
            Guid.TryParse(keyo!.ToString(), out key);
        }

        if (key == Guid.Empty)
        {
            return null;
        }

        IPublishedElement element = new PublishedElement(publishedContentType, key, propertyValues, preview, referenceCacheLevel, _publishedSnapshotAccessor);
        element = _publishedModelFactory.CreateModel(element);

        return element;
    }

    public Type GetModelType(Guid contentTypeKey)
    {
        IPublishedContentCache? publishedContentCache =
            _publishedSnapshotAccessor.GetRequiredPublishedSnapshot().Content;
        IPublishedContentType? publishedContentType = publishedContentCache?.GetContentType(contentTypeKey);
        if (publishedContentType is not null && publishedContentType.IsElement)
        {
            return _publishedModelFactory.GetModelType(publishedContentType.Alias);
        }

        return typeof(IPublishedElement);
    }
}
