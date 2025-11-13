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
        => GetContentTypeInfos(PublishedItemType.Content, _contentTypeService.GetAll());

    /// <inheritdoc/>
    public ICollection<ContentTypeInfo> GetMediaTypes()
        => GetContentTypeInfos(PublishedItemType.Media, _mediaTypeService.GetAll());

    private ICollection<ContentTypeInfo> GetContentTypeInfos(
        PublishedItemType itemType,
        IEnumerable<IContentTypeComposition> contentTypes)
    {
        List<ContentTypeInfo> result = [];

        foreach (IContentTypeComposition contentType in contentTypes)
        {
            IPublishedContentType publishedContentType = _publishedContentTypeCache.Get(itemType, contentType.Alias);
            HashSet<string> ownPropertyAliases = [.. contentType.PropertyTypes.Select(p => p.Alias)];

            result.Add(
                new ContentTypeInfo
                {
                    Alias = contentType.Alias,
                    SchemaId = GetContentTypeSchemaId(contentType.Alias),
                    CompositionSchemaIds = [.. publishedContentType.CompositionAliases.Select(GetContentTypeSchemaId)],
                    Properties =
                    [
                        .. publishedContentType.PropertyTypes.Select(p => new ContentTypePropertyInfo
                        {
                            Alias = p.Alias,
                            EditorAlias = p.EditorAlias,
                            DeliveryApiType = p.DeliveryApiModelClrType,
                            Inherited = !ownPropertyAliases.Contains(p.Alias),
                        })
                    ],
                    IsElement = publishedContentType.IsElement,
                });
        }

        return result;
    }

    private string GetContentTypeSchemaId(string contentTypeAlias) =>
        contentTypeAlias.ToCleanString(_shortStringHelper, CleanStringType.ConvertCase | CleanStringType.PascalCase);
}
