using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal class BlockElementService : IBlockElementService
{
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;
    private readonly IPublishedContentFactory _publishedContentFactory;
    private readonly IPublishedModelFactory _publishedModelFactory;
    private readonly ILanguageService _languageService;

    public BlockElementService(
        IPublishedContentTypeCache publishedContentTypeCache,
        IPublishedContentFactory publishedContentFactory,
        IPublishedModelFactory publishedModelFactory,
        ILanguageService languageService)
    {
        _publishedContentTypeCache = publishedContentTypeCache;
        _publishedContentFactory = publishedContentFactory;
        _publishedModelFactory = publishedModelFactory;
        _languageService = languageService;
    }

    public async Task<IPublishedElement?> BuildElementAsync(IPublishedElement owner, BlockItemData blockItemData, bool? preview = null)
    {
        ILanguage[]? allLanguages = null;
        ILanguage? defaultLanguage = null;

        // Only convert element types - content types will cause an exception when PublishedModelFactory creates the model
        IPublishedContentType? publishedContentType = _publishedContentTypeCache.Get(PublishedItemType.Element, blockItemData.ContentTypeKey);
        if (publishedContentType is null || publishedContentType.IsElement is false)
        {
            return null;
        }

        var propertyData = new Dictionary<string, PropertyData[]>();
        foreach (IGrouping<string, BlockPropertyValue> properties in blockItemData.Values.GroupBy(value => value.Alias))
        {
            IPublishedPropertyType? propertyType = publishedContentType.GetPropertyType(properties.Key);
            if (propertyType is null)
            {
                continue;
            }

            if (propertyType.VariesByCulture() && owner.ContentType.VariesByCulture() is false)
            {
                // Special case:
                // The element property type varies by culture, but the owner element (e.g. the page) content type does not
                // vary by culture. Since the created element is fully culture aware at render time, we need to replicate
                // property values across all available languages, to make them available for rendering.

                if (allLanguages is null)
                {
                    allLanguages = (await _languageService.GetAllAsync()).ToArray();
                    defaultLanguage = allLanguages.Single(l => l.IsDefault);
                }

                BlockPropertyValue property = properties.FirstOrDefault(p => p.Culture.InvariantEquals(defaultLanguage!.IsoCode))
                                              ?? properties.First();
                propertyData[properties.Key] = allLanguages.Select(language => new PropertyData
                {
                    Culture = language.IsoCode,
                    Segment = property.Segment ?? string.Empty, // throws an error if there is no value
                    Value = property.Value,
                }).ToArray();
            }
            else
            {
                propertyData[properties.Key] = properties.Select(property => new PropertyData
                {
                    Culture = property.Culture ?? string.Empty, // throws an error if there is no value
                    Segment = property.Segment ?? string.Empty, // throws an error if there is no value
                    Value = property.Value,
                }).ToArray();
            }
        }

        var published = preview is not true;
        var draft = published is false;

        const string name = "n/a";

        IEnumerable<string> cultures = publishedContentType.VariesByCulture()
            ? propertyData.SelectMany(p => p.Value.Select(v => v.Culture)).WhereNotNull().Distinct()
            : [];

        var cultureInfos = cultures.ToDictionary(
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
        return result.CreateModel(_publishedModelFactory);
    }
}
