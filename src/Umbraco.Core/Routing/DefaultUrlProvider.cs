using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides urls.
/// </summary>
public class DefaultUrlProvider : IUrlProvider
{
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedTextService? _localizedTextService;
    private readonly ILogger<DefaultUrlProvider> _logger;
    private readonly ISiteDomainMapper _siteDomainMapper;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly UriUtility _uriUtility;
    private RequestHandlerSettings _requestSettings;

    [Obsolete("Use ctor with all parameters")]
    public DefaultUrlProvider(
        IOptionsMonitor<RequestHandlerSettings> requestSettings,
        ILogger<DefaultUrlProvider> logger,
        ISiteDomainMapper siteDomainMapper,
        IUmbracoContextAccessor umbracoContextAccessor,
        UriUtility uriUtility)
        : this(
            requestSettings,
            logger,
            siteDomainMapper,
            umbracoContextAccessor,
            uriUtility,
            StaticServiceProvider.Instance.GetRequiredService<ILocalizationService>())
    {
    }

    public DefaultUrlProvider(
        IOptionsMonitor<RequestHandlerSettings> requestSettings,
        ILogger<DefaultUrlProvider> logger,
        ISiteDomainMapper siteDomainMapper,
        IUmbracoContextAccessor umbracoContextAccessor,
        UriUtility uriUtility,
        ILocalizationService localizationService)
    {
        _requestSettings = requestSettings.CurrentValue;
        _logger = logger;
        _siteDomainMapper = siteDomainMapper;
        _umbracoContextAccessor = umbracoContextAccessor;
        _uriUtility = uriUtility;
        _localizationService = localizationService;

        requestSettings.OnChange(x => _requestSettings = x);
    }

    #region GetOtherUrls

    /// <summary>
    ///     Gets the other URLs of a published content.
    /// </summary>
    /// <param name="umbracoContextAccessor">The Umbraco context.</param>
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
        IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
        IPublishedContent? node = umbracoContext.Content?.GetById(id);
        if (node == null)
        {
            yield break;
        }

        // look for domains, walking up the tree
        IPublishedContent? n = node;
        IEnumerable<DomainAndUri>? domainUris =
            DomainUtilities.DomainsForNode(umbracoContext.PublishedSnapshot.Domains, _siteDomainMapper, n.Id, current, false);

        // n is null at root
        while (domainUris == null && n != null)
        {
            n = n.Parent; // move to parent node
            domainUris = n == null
                ? null
                : DomainUtilities.DomainsForNode(umbracoContext.PublishedSnapshot.Domains, _siteDomainMapper, n.Id, current);
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
            var route = umbracoContext.Content?.GetRouteById(id, culture);
            if (route == null)
            {
                continue;
            }

            // need to strip off the leading ID for the route if it exists (occurs if the route is for a node with a domain assigned)
            var pos = route.IndexOf('/');
            var path = pos == 0 ? route : route.Substring(pos);

            var uri = new Uri(CombinePaths(d.Uri.GetLeftPart(UriPartial.Path), path));
            uri = _uriUtility.UriFromUmbraco(uri, _requestSettings);
            yield return UrlInfo.Url(uri.ToString(), culture);
        }
    }

    #endregion

    #region GetUrl

    /// <inheritdoc />
    public virtual UrlInfo? GetUrl(IPublishedContent content, UrlMode mode, string? culture, Uri current)
    {
        if (!current.IsAbsoluteUri)
        {
            throw new ArgumentException("Current URL must be absolute.", nameof(current));
        }

        IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();

        // will not use cache if previewing
        var route = umbracoContext.Content?.GetRouteById(content.Id, culture);

        return GetUrlFromRoute(route, umbracoContext, content.Id, current, mode, culture);
    }

    internal UrlInfo? GetUrlFromRoute(
        string? route,
        IUmbracoContext umbracoContext,
        int id,
        Uri current,
        UrlMode mode,
        string? culture)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            _logger.LogDebug(
                "Couldn't find any page with nodeId={NodeId}. This is most likely caused by the page not being published.",
                id);
            return null;
        }

        // extract domainUri and path
        // route is /<path> or <domainRootId>/<path>
        var pos = route.IndexOf('/');
        var path = pos == 0 ? route : route[pos..];
        DomainAndUri? domainUri = pos == 0
            ? null
            : DomainUtilities.DomainForNode(
                umbracoContext.PublishedSnapshot.Domains,
                _siteDomainMapper,
                int.Parse(route[..pos], CultureInfo.InvariantCulture),
                current,
                culture);

        var defaultCulture = _localizationService.GetDefaultLanguageIsoCode();
        if (domainUri is not null || string.IsNullOrEmpty(culture) ||
            culture.Equals(defaultCulture, StringComparison.InvariantCultureIgnoreCase))
        {
            var url = AssembleUrl(domainUri, path, current, mode).ToString();
            return UrlInfo.Url(url, culture);
        }

        return null;
    }

    #endregion

    #region Utilities

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

    #endregion
}
