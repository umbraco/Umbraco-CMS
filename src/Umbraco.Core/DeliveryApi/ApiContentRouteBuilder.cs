using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Default implementation of <see cref="IApiContentRouteBuilder"/> that builds content routes for the Delivery API.
/// </summary>
public sealed class ApiContentRouteBuilder : IApiContentRouteBuilder
{
    private readonly IApiContentPathProvider _apiContentPathProvider;
    private readonly GlobalSettings _globalSettings;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IRequestPreviewService _requestPreviewService;
    private readonly IPublishedContentCache _contentCache;
    private readonly IDocumentNavigationQueryService _navigationQueryService;
    private readonly IPublishStatusQueryService _publishStatusQueryService;
    private readonly IDocumentUrlService _documentUrlService;
    private RequestHandlerSettings _requestSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiContentRouteBuilder"/> class.
    /// </summary>
    /// <param name="apiContentPathProvider">The API content path provider.</param>
    /// <param name="globalSettings">The global settings.</param>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
    /// <param name="requestPreviewService">The request preview service.</param>
    /// <param name="requestSettings">The request handler settings.</param>
    /// <param name="contentCache">The published content cache.</param>
    /// <param name="navigationQueryService">The document navigation query service.</param>
    /// <param name="publishStatusQueryService">The publish status query service.</param>
    /// <param name="documentUrlService">The document URL service.</param>
    public ApiContentRouteBuilder(
        IApiContentPathProvider apiContentPathProvider,
        IOptions<GlobalSettings> globalSettings,
        IVariationContextAccessor variationContextAccessor,
        IRequestPreviewService requestPreviewService,
        IOptionsMonitor<RequestHandlerSettings> requestSettings,
        IPublishedContentCache contentCache,
        IDocumentNavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        IDocumentUrlService documentUrlService)
    {
        _apiContentPathProvider = apiContentPathProvider;
        _variationContextAccessor = variationContextAccessor;
        _requestPreviewService = requestPreviewService;
        _contentCache = contentCache;
        _navigationQueryService = navigationQueryService;
        _publishStatusQueryService = publishStatusQueryService;
        _documentUrlService = documentUrlService;
        _globalSettings = globalSettings.Value;
        _requestSettings = requestSettings.CurrentValue;
        requestSettings.OnChange(settings => _requestSettings = settings);
    }

    /// <inheritdoc />
    public IApiContentRoute? Build(IPublishedContent content, string? culture = null)
    {
        if (content.ItemType != PublishedItemType.Content)
        {
            throw new ArgumentException("Content locations can only be built from Content items.", nameof(content));
        }

        var isPreview = _requestPreviewService.IsPreview();

        var contentPath = GetContentPath(content, culture, isPreview);
        if (contentPath == null)
        {
            return null;
        }

        contentPath = contentPath.EnsureStartsWith("/");

        IPublishedContent? root = GetRoot(content, isPreview);
        if (root is null)
        {
            return null;
        }

        var rootPath = root.UrlSegment(_variationContextAccessor, culture) ?? string.Empty;

        if (_globalSettings.HideTopLevelNodeFromPath == false)
        {
            contentPath = contentPath.TrimStart(rootPath.EnsureStartsWith("/")).EnsureStartsWith("/");
        }

        return new ApiContentRoute(contentPath, new ApiContentStartItem(root.Key, rootPath));
    }

    private string? GetContentPath(IPublishedContent content, string? culture, bool isPreview)
    {
        // entirely unpublished content does not resolve any route, but we need one i.e. for preview to work,
        // so we'll use the content key as path.
        if (isPreview && _publishStatusQueryService.IsDocumentPublished(content.Key, culture ?? string.Empty) is false)
        {
            return ContentPreviewPath(content);
        }

        // grab the content path from the URL provider
        var contentPath = _apiContentPathProvider.GetContentPath(content, culture);

        // in some scenarios the published content is actually routable, but due to the built-in handling of i.e. lacking culture setup
        // the URL provider resolves the content URL as empty string or "#". since the Delivery API handles routing explicitly,
        // we can perform fallback to the content route.
        if (IsInvalidContentPath(contentPath))
        {
            contentPath = _documentUrlService.GetLegacyRouteFormat(content.Key, culture ?? _variationContextAccessor.VariationContext?.Culture, isPreview);
        }

        // if the content path has still not been resolved as a valid path, the content is un-routable in this culture
        // - unless we are routing for preview
        if (IsInvalidContentPath(contentPath))
        {
            return isPreview
                ? ContentPreviewPath(content)
                : null;
        }

        return _requestSettings.AddTrailingSlash
            ? contentPath?.EnsureEndsWith('/')
            : contentPath?.TrimEnd('/');
    }

    private string ContentPreviewPath(IPublishedContent content) => $"{Constants.DeliveryApi.Routing.PreviewContentPathPrefix}{content.Key:D}{(_requestSettings.AddTrailingSlash ? "/" : string.Empty)}";

    private static bool IsInvalidContentPath(string? path) => path.IsNullOrWhiteSpace() || "#".Equals(path);

    private IPublishedContent? GetRoot(IPublishedContent content, bool isPreview)
    {
        if (content.Level == 1)
        {
            return content;
        }

        if (_navigationQueryService.TryGetAncestorsKeys(content.Key, out IEnumerable<Guid> ancestorKeys) is false)
        {
            return null;
        }

        Guid[] ancestorKeysAsArray = ancestorKeys as Guid[] ?? ancestorKeys.ToArray();
        return ancestorKeysAsArray.Length > 0
            ? _contentCache.GetById(isPreview, ancestorKeysAsArray.Last())
            : content;
    }
}
