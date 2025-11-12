using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class ContentTypeInfoService : IContentTypeInfoService
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;
    private readonly IShortStringHelper _shortStringHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeInfoService"/> class.
    /// </summary>
    public ContentTypeInfoService(
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IPublishedContentTypeCache publishedContentTypeCache,
        IShortStringHelper shortStringHelper)
    {
        _contentTypeService = contentTypeService;
        _mediaTypeService = mediaTypeService;
        _publishedContentTypeCache = publishedContentTypeCache;
        _shortStringHelper = shortStringHelper;
    }

    /// <inheritdoc/>
    public ICollection<ContentTypeInfo> GetContentTypes()
        => GetContentTypeInfos(PublishedItemType.Content, _contentTypeService.GetAll().Select(x => x.Alias));

    /// <inheritdoc/>
    public ICollection<ContentTypeInfo> GetMediaTypes()
        => GetContentTypeInfos(PublishedItemType.Media, _mediaTypeService.GetAll().Select(x => x.Alias));

    private ICollection<ContentTypeInfo> GetContentTypeInfos(PublishedItemType itemType, IEnumerable<string> contentTypeAliases)
    {
        List<ContentTypeInfo> result = [];
        HashSet<string> compositionAliases = [];

        foreach (var contentTypeAlias in contentTypeAliases)
        {
            IPublishedContentType publishedContentType = _publishedContentTypeCache.Get(itemType, contentTypeAlias);
            HashSet<string> ownPropertyAliases = [.. publishedContentType.PropertyTypes.Select(p => p.Alias)];

            result.Add(
                new ContentTypeInfo
                {
                    Alias = contentTypeAlias,
                    SchemaId = GetContentTypeSchemaId(contentTypeAlias),
                    CompositionSchemaIds = [.. publishedContentType.CompositionAliases.Select(GetContentTypeSchemaId)],
                    Properties =
                    [
                        .. publishedContentType.PropertyTypes.Select(p => new ContentTypePropertyInfo
                        {
                            Alias = p.Alias,
                            EditorAlias = p.EditorAlias,
                            Type = p.DeliveryApiModelClrType,
                            Inherited = !ownPropertyAliases.Contains(p.Alias),
                        })
                    ],
                    IsElement = publishedContentType.IsElement,
                    IsComposition = false,
                });

            compositionAliases.UnionWith(publishedContentType.CompositionAliases);
        }

        result.ForEach(c => c.IsComposition = compositionAliases.Contains(c.Alias));

        return result;
    }

    private string GetContentTypeSchemaId(string contentTypeAlias) =>
        contentTypeAlias.ToCleanString(_shortStringHelper, CleanStringType.ConvertCase | CleanStringType.PascalCase);
}
