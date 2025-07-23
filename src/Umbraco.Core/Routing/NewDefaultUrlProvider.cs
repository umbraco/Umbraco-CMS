using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides URLs.
/// </summary>
public class NewDefaultUrlProvider : IUrlProvider
{
    private readonly IPublishedContentCache _publishedContentCache;
    private readonly IDomainCache _domainCache;
    private readonly IIdKeyMap _idKeyMap;
    private readonly IDocumentUrlService _documentUrlService;
    private readonly IDocumentNavigationQueryService _navigationQueryService;
    private readonly IPublishedContentStatusFilteringService _publishedContentStatusFilteringService;
    private readonly ILogger<DefaultUrlProvider> _logger;
    private readonly ISiteDomainMapper _siteDomainMapper;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly UriUtility _uriUtility;
    private RequestHandlerSettings _requestSettings;
    private readonly ILanguageService _languageService;

    // TODO (V17): When removing the obsolete constructors, remove the unused localizationService parameter from the constructor (we can't do it now
    // because it is used in the obsolete constructors and leads to an ambigious constructor error).
    // See also if we can make GetUrlFromRoute asynchronous and avoid the GetAwaiter().GetResult() in when using ILanguageService.

    /// <summary>
    /// Initializes a new instance of the <see cref="NewDefaultUrlProvider"/> class.
    /// </summary>
    public NewDefaultUrlProvider(
        IOptionsMonitor<RequestHandlerSettings> requestSettings,
        ILogger<DefaultUrlProvider> logger,
        ISiteDomainMapper siteDomainMapper,
        IUmbracoContextAccessor umbracoContextAccessor,
        UriUtility uriUtility,
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable IDE0060 // Remove unused parameter
        ILocalizationService localizationService,
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CS0618 // Type or member is obsolete
        IPublishedContentCache publishedContentCache,
        IDomainCache domainCache,
        IIdKeyMap idKeyMap,
        IDocumentUrlService documentUrlService,
        IDocumentNavigationQueryService navigationQueryService,
        IPublishedContentStatusFilteringService publishedContentStatusFilteringService,
        ILanguageService languageService)
    {
        _requestSettings = requestSettings.CurrentValue;
        _logger = logger;
        _siteDomainMapper = siteDomainMapper;
        _umbracoContextAccessor = umbracoContextAccessor;
        _uriUtility = uriUtility;
        _publishedContentCache = publishedContentCache;
        _domainCache = domainCache;
        _idKeyMap = idKeyMap;
        _documentUrlService = documentUrlService;
        _navigationQueryService = navigationQueryService;
        _publishedContentStatusFilteringService = publishedContentStatusFilteringService;
        _languageService = languageService;

        requestSettings.OnChange(x => _requestSettings = x);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NewDefaultUrlProvider"/> class.
    /// </summary>
    [Obsolete("Use the non-obsolete constructor. Scheduled for removal in V17.")]
    public NewDefaultUrlProvider(
        IOptionsMonitor<RequestHandlerSettings> requestSettings,
        ILogger<DefaultUrlProvider> logger,
        ISiteDomainMapper siteDomainMapper,
        IUmbracoContextAccessor umbracoContextAccessor,
        UriUtility uriUtility,
        ILocalizationService localizationService,
        IPublishedContentCache publishedContentCache,
        IDomainCache domainCache,
        IIdKeyMap idKeyMap,
        IDocumentUrlService documentUrlService,
        IDocumentNavigationQueryService navigationQueryService,
        IPublishedContentStatusFilteringService publishedContentStatusFilteringService)
        : this(
            requestSettings,
            logger,
            siteDomainMapper,
            umbracoContextAccessor,
            uriUtility,
            localizationService,
            publishedContentCache,
            domainCache,
            idKeyMap,
            documentUrlService,
            navigationQueryService,
            publishedContentStatusFilteringService,
            StaticServiceProvider.Instance.GetRequiredService<ILanguageService>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NewDefaultUrlProvider"/> class.
    /// </summary>
    [Obsolete("Use the non-obsolete constructor. Scheduled for removal in V17.")]
    public NewDefaultUrlProvider(
        IOptionsMonitor<RequestHandlerSettings> requestSettings,
        ILogger<DefaultUrlProvider> logger,
        ISiteDomainMapper siteDomainMapper,
        IUmbracoContextAccessor umbracoContextAccessor,
        UriUtility uriUtility,
        ILocalizationService localizationService,
        IPublishedContentCache publishedContentCache,
        IDomainCache domainCache,
        IIdKeyMap idKeyMap,
        IDocumentUrlService documentUrlService,
        IDocumentNavigationQueryService navigationQueryService)
        : this(
            requestSettings,
            logger,
            siteDomainMapper,
            umbracoContextAccessor,
            uriUtility,
            localizationService,
            publishedContentCache,
            domainCache,
            idKeyMap,
            documentUrlService,
            navigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IPublishedContentStatusFilteringService>())
    {
    }

    /// <summary>
    ///     Gets the other URLs of a published content.
    /// </summary>
    /// <param name="id">The published content id.</param>
    /// <param name="current">The current absolute URL.</param>
    /// <returns>The other URLs for the published content.</returns>
    /// <remarks>
    ///     <para>
    ///         Other URLs are those that <c>GetUrl</c> would not return in the current context, but would be valid
    ///         URLs for the node in other contexts (different domain for current request, umbracoUrlAlias...).
    ///     </para>
    /// </remarks>
    public virtual IEnumerable<UrlInfo> GetOtherUrls(int id, Uri current)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Document);

        if (keyAttempt.Success is false)
        {
            yield break;
        }

        Guid key = keyAttempt.Result;

        IPublishedContent? node = _publishedContentCache.GetById(key);
        if (node == null)
        {
            yield break;
        }

        // look for domains, walking up the tree
        IPublishedContent? n = node;
        IEnumerable<DomainAndUri>? domainUris =
            DomainUtilities.DomainsForNode(_domainCache, _siteDomainMapper, n.Id, current, false);

        // n is null at root
        while (domainUris == null && n != null)
        {
            n = n.Parent<IPublishedContent>(_navigationQueryService, _publishedContentStatusFilteringService); // move to parent node
            domainUris = n == null
                ? null
                : DomainUtilities.DomainsForNode(_domainCache, _siteDomainMapper, n.Id, current);
        }

        // no domains = exit
        if (domainUris == null)
        {
            yield break;
        }

        foreach (DomainAndUri d in domainUris)
        {
            var culture = d.Culture;

            // although we are passing in culture here, if any node in this path is invariant, it ignores the culture anyways so this is ok
            var route = GetLegacyRouteFormatById(key, culture);
            if (route == null || route == "#")
            {
                continue;
            }

            // need to strip off the leading ID for the route if it exists (occurs if the route is for a node with a domain assigned)
            var pos = route.IndexOf('/', StringComparison.Ordinal);
            var path = pos == 0 ? route : route[pos..];

            var uri = new Uri(CombinePaths(d.Uri.GetLeftPart(UriPartial.Path), path));
            uri = _uriUtility.UriFromUmbraco(uri, _requestSettings);
            yield return UrlInfo.Url(uri.ToString(), culture);
        }
    }

    /// <summary>
    /// Gets the legacy route format by id
    /// </summary>
    /// <param name="key"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    /// <remarks>
    /// When no domain is set the route can be something like /child/grandchild
    /// When a domain is set, the route can be something like 1234/grandchild
    /// </remarks>

    private string GetLegacyRouteFormatById(Guid key, string? culture)
    {
        var isDraft = _umbracoContextAccessor.GetRequiredUmbracoContext().InPreviewMode;
        return _documentUrlService.GetLegacyRouteFormat(key, culture, isDraft);
    }

    /// <inheritdoc />
    public virtual UrlInfo? GetUrl(IPublishedContent content, UrlMode mode, string? culture, Uri current)
    {
        if (!current.IsAbsoluteUri)
        {
            throw new ArgumentException("Current URL must be absolute.", nameof(current));
        }

        // This might seem to be some code duplication, as we do the same check in GetLegacyRouteFormat
        // but this is strictly neccesary, as if we're coming from a published notification
        // this document will still not always be in the memory cache. And thus we have to hit the DB
        // We have the published content now, so we can check if the culture is published, and thus avoid the DB hit.
        string route;
        var isDraft = _umbracoContextAccessor.GetRequiredUmbracoContext().InPreviewMode;
        if(isDraft is false && string.IsNullOrWhiteSpace(culture) is false && content.Cultures.Any() && content.IsInvariantOrHasCulture(culture) is false)
        {
            route = "#";
        }
        else
        {
            route = GetLegacyRouteFormatById(content.Key, culture);
        }

        // will not use cache if previewing

        return GetUrlFromRoute(route, content.Id, current, mode, culture);
    }

    /// <summary>
    /// Gets the URL from the provided route.
    /// </summary>
    internal UrlInfo? GetUrlFromRoute(
        string? route,
        int id,
        Uri current,
        UrlMode mode,
        string? culture)
    {
        if (string.IsNullOrWhiteSpace(route) || route.Equals("#"))
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                "Couldn't find any page with nodeId={NodeId}. This is most likely caused by the page not being published.",
                id);
            }
            return null;
        }

        // extract domainUri and path
        // route is /<path> or <domainRootId>/<path>
        var pos = route.IndexOf('/', StringComparison.Ordinal);
        var path = pos == 0 ? route : route[pos..];
        DomainAndUri? domainUri = pos == 0
            ? null
            : DomainUtilities.DomainForNode(
                _domainCache,
                _siteDomainMapper,
                int.Parse(route[..pos], CultureInfo.InvariantCulture),
                current,
                culture);

        var defaultCulture = _languageService.GetDefaultIsoCodeAsync().GetAwaiter().GetResult();
        if (domainUri is not null ||
            string.IsNullOrEmpty(culture) ||
            culture.Equals(defaultCulture, StringComparison.InvariantCultureIgnoreCase))
        {
            var url = AssembleUrl(domainUri, path, current, mode).ToString();
            return UrlInfo.Url(url, culture);
        }

        return null;
    }

    private Uri AssembleUrl(DomainAndUri? domainUri, string path, Uri current, UrlMode mode)
    {
        Uri uri;

        // ignore vdir at that point, UriFromUmbraco will do it
        // no domain was found
        if (domainUri == null)
        {
            if (current == null)
            {
                mode = UrlMode.Relative; // best we can do
            }

            switch (mode)
            {
                case UrlMode.Absolute:
                    uri = new Uri(current!.GetLeftPart(UriPartial.Authority) + path);
                    break;
                case UrlMode.Relative:
                case UrlMode.Auto:
                    uri = new Uri(path, UriKind.Relative);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }

        // a domain was found
        else
        {
            if (mode == UrlMode.Auto)
            {
                // this check is a little tricky, we can't just compare domains
                if (current != null && domainUri.Uri.GetLeftPart(UriPartial.Authority) ==
                    current.GetLeftPart(UriPartial.Authority))
                {
                    mode = UrlMode.Relative;
                }
                else
                {
                    mode = UrlMode.Absolute;
                }
            }

            switch (mode)
            {
                case UrlMode.Absolute:
                    uri = new Uri(CombinePaths(domainUri.Uri.GetLeftPart(UriPartial.Path), path));
                    break;
                case UrlMode.Relative:
                    uri = new Uri(CombinePaths(domainUri.Uri.AbsolutePath, path), UriKind.Relative);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }

        // UriFromUmbraco will handle vdir
        // meaning it will add vdir into domain URLs too!
        return _uriUtility.UriFromUmbraco(uri, _requestSettings);
    }

    private string CombinePaths(string path1, string path2)
    {
        var path = path1.TrimEnd(Constants.CharArrays.ForwardSlash) + path2;
        return path == "/" ? path : path.TrimEnd(Constants.CharArrays.ForwardSlash);
    }
}
