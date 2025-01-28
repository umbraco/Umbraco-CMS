using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class ApiContentRouteBuilder : IApiContentRouteBuilder
{
    private readonly IApiContentPathProvider _apiContentPathProvider;
    private readonly GlobalSettings _globalSettings;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IRequestPreviewService _requestPreviewService;
    private RequestHandlerSettings _requestSettings;

    [Obsolete($"Use the constructor that does not accept {nameof(IPublishedUrlProvider)}. Will be removed in V15.")]
    public ApiContentRouteBuilder(
        IPublishedUrlProvider publishedUrlProvider,
        IOptions<GlobalSettings> globalSettings,
        IVariationContextAccessor variationContextAccessor,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IRequestPreviewService requestPreviewService,
        IOptionsMonitor<RequestHandlerSettings> requestSettings)
        : this(StaticServiceProvider.Instance.GetRequiredService<IApiContentPathProvider>(), globalSettings, variationContextAccessor, publishedSnapshotAccessor, requestPreviewService, requestSettings)
    {
    }

    [Obsolete($"Use the constructor that does not accept {nameof(IPublishedUrlProvider)}. Will be removed in V15.")]
    public ApiContentRouteBuilder(
        IPublishedUrlProvider publishedUrlProvider,
        IApiContentPathProvider apiContentPathProvider,
        IOptions<GlobalSettings> globalSettings,
        IVariationContextAccessor variationContextAccessor,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IRequestPreviewService requestPreviewService,
        IOptionsMonitor<RequestHandlerSettings> requestSettings)
        : this(apiContentPathProvider, globalSettings, variationContextAccessor, publishedSnapshotAccessor, requestPreviewService, requestSettings)
    {
    }

    public ApiContentRouteBuilder(
        IApiContentPathProvider apiContentPathProvider,
        IOptions<GlobalSettings> globalSettings,
        IVariationContextAccessor variationContextAccessor,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IRequestPreviewService requestPreviewService,
        IOptionsMonitor<RequestHandlerSettings> requestSettings)
    {
        _apiContentPathProvider = apiContentPathProvider;
        _variationContextAccessor = variationContextAccessor;
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _requestPreviewService = requestPreviewService;
        _globalSettings = globalSettings.Value;
        _requestSettings = requestSettings.CurrentValue;
        requestSettings.OnChange(settings => _requestSettings = settings);
    }

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

        IPublishedContent root = GetRoot(content, isPreview);
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
        if (isPreview && content.IsPublished(culture) is false)
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
            contentPath = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot().Content?.GetRouteById(content.Id, culture) ?? contentPath;
        }

        // if the content path has still not been resolved as a valid path, the content is un-routable in this culture
        // - unless we are routing for preview
        if (IsInvalidContentPath(contentPath))
        {
            return isPreview
                ? ContentPreviewPath(content)
                : null;
        }

        return contentPath;
    }

    private string ContentPreviewPath(IPublishedContent content) => $"{Constants.DeliveryApi.Routing.PreviewContentPathPrefix}{content.Key:D}{(_requestSettings.AddTrailingSlash ? "/" : string.Empty)}";

    private static bool IsInvalidContentPath(string? path) => path.IsNullOrWhiteSpace() || "#".Equals(path);

    private IPublishedContent GetRoot(IPublishedContent content, bool isPreview)
    {
        if (isPreview is false)
        {
            return content.Root();
        }

        // in very edge case scenarios during preview, content.Root() does not map to the root.
        // we'll code our way around it for the time being.
        return _publishedSnapshotAccessor
                   .GetRequiredPublishedSnapshot()
                   .Content?
                   .GetAtRoot(true)
                   .FirstOrDefault(root => root.IsAncestorOrSelf(content))
               ?? content.Root();
    }
}
