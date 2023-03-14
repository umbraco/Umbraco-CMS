using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides URLs using the <c>umbracoUrlAlias</c> property.
/// </summary>
public class AliasUrlProvider : IUrlProvider
{
    private readonly IPublishedValueFallback _publishedValueFallback;
    private readonly ISiteDomainMapper _siteDomainMapper;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly UriUtility _uriUtility;
    private RequestHandlerSettings _requestConfig;

    public AliasUrlProvider(
        IOptionsMonitor<RequestHandlerSettings> requestConfig,
        ISiteDomainMapper siteDomainMapper,
        UriUtility uriUtility,
        IPublishedValueFallback publishedValueFallback,
        IUmbracoContextAccessor umbracoContextAccessor)
    {
        _requestConfig = requestConfig.CurrentValue;
        _siteDomainMapper = siteDomainMapper;
        _uriUtility = uriUtility;
        _publishedValueFallback = publishedValueFallback;
        _umbracoContextAccessor = umbracoContextAccessor;

        requestConfig.OnChange(x => _requestConfig = x);
    }

    // note - at the moment we seem to accept pretty much anything as an alias
    // without any form of validation ... could even prob. kill the XPath ...
    // ok, this is somewhat experimental and is NOT enabled by default
    #region GetUrl

    /// <inheritdoc />
    public UrlInfo? GetUrl(IPublishedContent content, UrlMode mode, string? culture, Uri current) =>
        null; // we have nothing to say

    #endregion

    #region GetOtherUrls

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
    public IEnumerable<UrlInfo> GetOtherUrls(int id, Uri current)
    {
        IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
        IPublishedContent? node = umbracoContext.Content?.GetById(id);
        if (node == null)
        {
            yield break;
        }

        if (!node.HasProperty(Constants.Conventions.Content.UrlAlias))
        {
            yield break;
        }

        // look for domains, walking up the tree
        IPublishedContent? n = node;
        IEnumerable<DomainAndUri>? domainUris = DomainUtilities.DomainsForNode(umbracoContext.PublishedSnapshot.Domains, _siteDomainMapper, n.Id, current, false);

        // n is null at root
        while (domainUris == null && n != null)
        {
            // move to parent node
            n = n.Parent;
            domainUris = n == null
                ? null
                : DomainUtilities.DomainsForNode(umbracoContext.PublishedSnapshot.Domains, _siteDomainMapper, n.Id, current, false);
        }

        // determine whether the alias property varies
        var varies = node.GetProperty(Constants.Conventions.Content.UrlAlias)!.PropertyType.VariesByCulture();

        if (domainUris == null)
        {
            // no domain
            // if the property is invariant, then URL "/<alias>" is ok
            // if the property varies, then what are we supposed to do?
            //  the content finder may work, depending on the 'current' culture,
            //  but there's no way we can return something meaningful here
            if (varies)
            {
                yield break;
            }

            var umbracoUrlName = node.Value<string>(_publishedValueFallback, Constants.Conventions.Content.UrlAlias);
            var aliases = umbracoUrlName?.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);

            if (aliases == null || aliases.Any() == false)
            {
                yield break;
            }

            foreach (var alias in aliases.Distinct())
            {
                var path = "/" + alias;
                var uri = new Uri(path, UriKind.Relative);
                yield return UrlInfo.Url(_uriUtility.UriFromUmbraco(uri, _requestConfig).ToString());
            }
        }
        else
        {
            // some domains: one URL per domain, which is "<domain>/<alias>"
            foreach (DomainAndUri domainUri in domainUris)
            {
                // if the property is invariant, get the invariant value, URL is "<domain>/<invariant-alias>"
                // if the property varies, get the variant value, URL is "<domain>/<variant-alias>"

                // but! only if the culture is published, else ignore
                if (varies && !node.HasCulture(domainUri.Culture))
                {
                    continue;
                }

                var umbracoUrlName = varies
                    ? node.Value<string>(_publishedValueFallback, Constants.Conventions.Content.UrlAlias, domainUri.Culture)
                    : node.Value<string>(_publishedValueFallback, Constants.Conventions.Content.UrlAlias);

                var aliases = umbracoUrlName?.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);

                if (aliases == null || aliases.Any() == false)
                {
                    continue;
                }

                foreach (var alias in aliases.Distinct())
                {
                    var path = "/" + alias;
                    var uri = new Uri(CombinePaths(domainUri.Uri.GetLeftPart(UriPartial.Path), path));
                    yield return UrlInfo.Url(
                        _uriUtility.UriFromUmbraco(uri, _requestConfig).ToString(),
                        domainUri.Culture);
                }
            }
        }
    }

    #endregion

    #region Utilities

    private string CombinePaths(string path1, string path2)
    {
        var path = path1.TrimEnd(Constants.CharArrays.ForwardSlash) + path2;
        return path == "/" ? path : path.TrimEnd(Constants.CharArrays.ForwardSlash);
    }

    #endregion
}
