using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal class BlockElementService : IBlockElementService
{
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;
    private readonly IPublishedContentFactory _publishedContentFactory;
    private readonly IPublishedModelFactory _publishedModelFactory;

    public BlockElementService(
        IPublishedContentTypeCache publishedContentTypeCache,
        IPublishedContentFactory publishedContentFactory,
        IPublishedModelFactory publishedModelFactory)
    {
        _publishedContentTypeCache = publishedContentTypeCache;
        _publishedContentFactory = publishedContentFactory;
        _publishedModelFactory = publishedModelFactory;
    }

    public Task<IPublishedElement?> BuildElementAsync(BlockItemData blockItemData, bool? preview = null)
    {
        // Only convert element types - content types will cause an exception when PublishedModelFactory creates the model
        IPublishedContentType? publishedContentType = _publishedContentTypeCache.Get(PublishedItemType.Element, blockItemData.ContentTypeKey);
        if (publishedContentType is null || publishedContentType.IsElement is false)
        {
            return Task.FromResult<IPublishedElement?>(null);
        }

        var propertyData = new Dictionary<string, PropertyData[]>();
        foreach (IGrouping<string, BlockPropertyValue> properties in blockItemData.Values.GroupBy(value => value.Alias))
        {
            propertyData[properties.Key] = properties.Select(property => new PropertyData
            {
                Culture = property.Culture ?? string.Empty, // throws an error if there is no value
                Segment = property.Segment ?? string.Empty, // throws an error if there is no value
                Value = property.Value,
            }).ToArray();
        }

        var published = preview is not true;
        var draft = published is false;

        const string name = "n/a";

        var cultureInfos = (publishedContentType.VariesByCulture()
            ? blockItemData.Values.Select(value => value.Culture).WhereNotNull().Distinct()
            : []).ToDictionary(
            culture => culture,
            _ => new CultureVariation
            {
               IsDraft = draft,
               Date = DateTime.MinValue,
               Name = name,
               UrlSegment = null,
            });

        var contentData = new ContentData(
            name: name,
            urlSegment: null,
            versionId: 0,
            versionDate: DateTime.MinValue,
            writerId: 0,
            templateId: null,
            published: published,
            properties: propertyData,
            cultureInfos: cultureInfos);

        var contentCacheNode = new ContentCacheNode
        {
            ContentTypeId = publishedContentType.Id,
            CreateDate = DateTime.MinValue,
            CreatorId = 0,
            Data = contentData,
            Id = 0,
            IsDraft = draft,
            Key = blockItemData.Key,
            SortOrder = 0,
        };

        var result = _publishedContentFactory.ToIPublishedElement(contentCacheNode, draft);
        return Task.FromResult(result.CreateModel(_publishedModelFactory));
    }
}
