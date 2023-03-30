using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.ContentApi;

public class ApiContentRouteBuilder : IApiContentRouteBuilder
{
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly GlobalSettings _globalSettings;
    private readonly IVariationContextAccessor _variationContextAccessor;

    public ApiContentRouteBuilder(IPublishedUrlProvider publishedUrlProvider, IOptions<GlobalSettings> globalSettings, IVariationContextAccessor variationContextAccessor)
    {
        _publishedUrlProvider = publishedUrlProvider;
        _variationContextAccessor = variationContextAccessor;
        _globalSettings = globalSettings.Value;
    }

    public IApiContentRoute Build(IPublishedContent content, string? culture = null)
    {
        if (content.ItemType != PublishedItemType.Content)
        {
            throw new ArgumentException("Content locations can only be built from Content items.", nameof(content));
        }

        IPublishedContent root = content.Root();
        var rootPath = root.UrlSegment(_variationContextAccessor, culture) ?? string.Empty;

        var contentPath = _publishedUrlProvider.GetUrl(content, UrlMode.Relative, culture).EnsureStartsWith("/");

        if (_globalSettings.HideTopLevelNodeFromPath == false)
        {
            contentPath = contentPath.TrimStart(rootPath.EnsureStartsWith("/")).EnsureStartsWith("/");
        }

        return new ApiContentRoute(contentPath, new ApiContentStartItem(root.Key, rootPath));
    }
}
