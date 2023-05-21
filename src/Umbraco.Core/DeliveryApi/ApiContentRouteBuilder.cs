using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class ApiContentRouteBuilder : IApiContentRouteBuilder
{
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly GlobalSettings _globalSettings;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

    public ApiContentRouteBuilder(
        IPublishedUrlProvider publishedUrlProvider,
        IOptions<GlobalSettings> globalSettings,
        IVariationContextAccessor variationContextAccessor,
        IPublishedSnapshotAccessor publishedSnapshotAccessor)
    {
        _publishedUrlProvider = publishedUrlProvider;
        _variationContextAccessor = variationContextAccessor;
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _globalSettings = globalSettings.Value;
    }

    public IApiContentRoute? Build(IPublishedContent content, string? culture = null)
    {
        if (content.ItemType != PublishedItemType.Content)
        {
            throw new ArgumentException("Content locations can only be built from Content items.", nameof(content));
        }

        IPublishedContent root = content.Root();
        var rootPath = root.UrlSegment(_variationContextAccessor, culture) ?? string.Empty;

        var contentPath = _publishedUrlProvider.GetUrl(content, UrlMode.Relative, culture);

        // in some scenarios the published content is actually routable, but due to the built-in handling of i.e. lacking culture setup
        // the URL provider resolves the content URL as empty string or "#". since the Delivery API handles routing explicitly,
        // we can perform fallback to the content route.
        if (IsInvalidContentPath(contentPath))
        {
            contentPath = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot().Content?.GetRouteById(content.Id, culture) ?? contentPath;
        }

        // if the content path has still not been resolved as a valid path, the content is un-routable in this culture
        if (IsInvalidContentPath(contentPath))
        {
            return null;
        }

        contentPath = contentPath.EnsureStartsWith("/");
        if (_globalSettings.HideTopLevelNodeFromPath == false)
        {
            contentPath = contentPath.TrimStart(rootPath.EnsureStartsWith("/")).EnsureStartsWith("/");
        }

        return new ApiContentRoute(contentPath, new ApiContentStartItem(root.Key, rootPath));
    }

    private static bool IsInvalidContentPath(string path) => path.IsNullOrWhiteSpace() || "#".Equals(path);
}
