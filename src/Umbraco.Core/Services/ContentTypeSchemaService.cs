using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class ContentTypeSchemaService : IContentTypeSchemaService
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;
    private readonly IShortStringHelper _shortStringHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeSchemaService"/> class.
    /// </summary>
    public ContentTypeSchemaService(
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
    public IReadOnlyCollection<ContentTypeSchemaInfo> GetDocumentTypes()
        => GetContentTypeSchemaInfos(PublishedItemType.Content, _contentTypeService.GetAll());

    /// <inheritdoc/>
    public IReadOnlyCollection<ContentTypeSchemaInfo> GetMediaTypes()
        => GetContentTypeSchemaInfos(PublishedItemType.Media, _mediaTypeService.GetAll());

    private List<ContentTypeSchemaInfo> GetContentTypeSchemaInfos(
        PublishedItemType itemType,
        IEnumerable<IContentTypeComposition> contentTypes)
    {
        List<ContentTypeSchemaInfo> result = [];

        foreach (IContentTypeComposition contentType in contentTypes)
        {
            IPublishedContentType publishedContentType;
            try
            {
                publishedContentType = _publishedContentTypeCache.Get(itemType, contentType.Alias);
            }
            catch
            {
                // Skip content types that fail to load from cache
                continue;
            }

            HashSet<string> ownPropertyAliases = [.. contentType.PropertyTypes.Select(p => p.Alias)];

            result.Add(
                new ContentTypeSchemaInfo
                {
                    Alias = contentType.Alias,
                    SchemaId = GetContentTypeSchemaId(contentType.Alias),
                    CompositionSchemaIds = [.. publishedContentType.CompositionAliases.Select(GetContentTypeSchemaId)],
                    Properties =
                    [
                        .. publishedContentType.PropertyTypes.Select(p => new ContentTypePropertySchemaInfo
                        {
                            Alias = p.Alias,
                            EditorAlias = p.EditorAlias,
                            DeliveryApiClrType = p.DeliveryApiModelClrType,
                            Inherited = ownPropertyAliases.Contains(p.Alias) is false,
                        })
                    ],
                    IsElement = publishedContentType.IsElement,
                });
        }

        return result;
    }

    // Currently uses the same transformation as ModelsBuilder (UmbracoServices.GetClrName)
    private string GetContentTypeSchemaId(string contentTypeAlias) =>
        contentTypeAlias.ToCleanString(_shortStringHelper, CleanStringType.ConvertCase | CleanStringType.PascalCase);
}
