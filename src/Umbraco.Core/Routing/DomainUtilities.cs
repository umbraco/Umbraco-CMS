using System.Globalization;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing
{
    /// <summary>
    /// Provides utilities to handle domains.
    /// </summary>
    public static class DomainUtilities
    {
        #region Document Culture

        /// <summary>
        /// Gets the culture assigned to a document by domains, in the context of a current Uri.
        /// </summary>
        /// <param name="contentId">The document identifier.</param>
        /// <param name="contentPath">The document path.</param>
        /// <param name="current">An optional current Uri.</param>
        /// <param name="umbracoContext">An Umbraco context.</param>
        /// <param name="siteDomainMapper">The site domain helper.</param>
        /// <returns>The culture assigned to the document by domains.</returns>
        /// <remarks>
        /// <para>In 1:1 multilingual setup, a document contains several cultures (there is not
        /// one document per culture), and domains, withing the context of a current Uri, assign
        /// a culture to that document.</para>
        /// </remarks>
        public static string? GetCultureFromDomains(int contentId, string contentPath, Uri? current, IUmbracoContext umbracoContext, ISiteDomainMapper siteDomainMapper)
        {
            if (umbracoContext == null)
            {
                throw new InvalidOperationException("A current UmbracoContext is required.");
            }

            if (current == null)
            {
                current = umbracoContext.CleanedUmbracoUrl;
            }

            // get the published route, else the preview route
            // if both are null then the content does not exist
            var route = umbracoContext.Content?.GetRouteById(contentId) ??
                        umbracoContext.Content?.GetRouteById(true, contentId);

            if (route == null)
            {
                return null;
            }

            var pos = route.IndexOf('/');
            DomainAndUri? domain = pos == 0
                ? null
                : DomainForNode(umbracoContext.Domains, siteDomainMapper, int.Parse(route.Substring(0, pos), CultureInfo.InvariantCulture), current);

            var rootContentId = domain?.ContentId ?? -1;
            Domain? wcDomain = FindWildcardDomainInPath(umbracoContext.Domains?.GetAll(true), contentPath, rootContentId);

            if (wcDomain != null)
            {
                return wcDomain.Culture;
            }

            if (domain != null)
            {
                return domain.Culture;
            }

            return umbracoContext.Domains?.DefaultCulture;
        }

        #endregion

        #region Domain for Document

        /// <summary>
        /// Finds the domain for the specified node, if any, that best matches a specified uri.
        /// </summary>
        /// <param name="domainCache">A domain cache.</param>
        /// <param name="siteDomainMapper">The site domain helper.</param>
        /// <param name="nodeId">The node identifier.</param>
        /// <param name="current">The uri, or null.</param>
        /// <param name="culture">The culture, or null.</param>
        /// <returns>The domain and its uri, if any, that best matches the specified uri and culture, else null.</returns>
        /// <remarks>
        /// <para>If at least a domain is set on the node then the method returns the domain that
        /// best matches the specified uri and culture, else it returns null.</para>
        /// <para>If culture is null, uses the default culture for the installation instead. Otherwise,
        /// will try with the specified culture, else return null.</para>
        /// </remarks>
        internal static DomainAndUri? DomainForNode(IDomainCache? domainCache, ISiteDomainMapper siteDomainMapper, int nodeId, Uri current, string? culture = null)
        {
            // be safe
            if (nodeId <= 0)
            {
                return null;
            }

            // get the domains on that node
            Domain[]? domains = domainCache?.GetAssigned(nodeId).ToArray();

            // none?
            if (domains is null || domains.Length == 0)
            {
                return null;
            }

            // else filter
            // it could be that none apply (due to culture)
            return SelectDomain(domains, current, culture, domainCache?.DefaultCulture, siteDomainMapper.MapDomain);
        }

        /// <summary>
        /// Find the domains for the specified node, if any, that match a specified uri.
        /// </summary>
        /// <param name="domainCache">A domain cache.</param>
        /// <param name="siteDomainMapper">The site domain helper.</param>
        /// <param name="nodeId">The node identifier.</param>
        /// <param name="current">The uri, or null.</param>
        /// <param name="excludeDefault">A value indicating whether to exclude the current/default domain. True by default.</param>
        /// <returns>The domains and their uris, that match the specified uri, else null.</returns>
        /// <remarks>If at least a domain is set on the node then the method returns the domains that
        /// best match the specified uri, else it returns null.</remarks>
        internal static IEnumerable<DomainAndUri>? DomainsForNode(IDomainCache? domainCache, ISiteDomainMapper siteDomainMapper, int nodeId, Uri current, bool excludeDefault = true)
        {
            // be safe
            if (nodeId <= 0)
            {
                return null;
            }

            // get the domains on that node
            Domain[]? domains = domainCache?.GetAssigned(nodeId).ToArray();

            // none?
            if (domains is null || domains.Length == 0)
            {
                return null;
            }

            // get the domains and their uris
            DomainAndUri[] domainAndUris = SelectDomains(domains, current).ToArray();

            // filter
            return siteDomainMapper.MapDomains(domainAndUris, current, excludeDefault, null, domainCache?.DefaultCulture).ToArray();
        }

        #endregion

        #region Selects Domain(s)

        /// <summary>
        /// Selects the domain that best matches a specified uri and cultures, from a set of domains.
        /// </summary>
        /// <param name="domains">The group of domains.</param>
        /// <param name="uri">An optional uri.</param>
        /// <param name="culture">An optional culture.</param>
        /// <param name="defaultCulture">An optional default culture.</param>
        /// <param name="filter">An optional function to filter the list of domains, if more than one applies.</param>
        /// <returns>The domain and its normalized uri, that best matches the specified uri and cultures.</returns>
        /// <remarks>
        /// TODO: must document and explain this all
        /// <para>If <paramref name="uri"/> is null, pick the first domain that matches <paramref name="culture"/>,
        /// else the first that matches <paramref name="defaultCulture"/>, else the first one (ordered by id), else null.</para>
        /// <para>If <paramref name="uri"/> is not null, look for domains that would be a base uri of the current uri,</para>
        /// <para>If more than one domain matches, then the <paramref name="filter"/> function is used to pick
        /// the right one, unless it is <c>null</c>, in which case the method returns <c>null</c>.</para>
        /// <para>The filter, if any, will be called only with a non-empty argument, and _must_ return something.</para>
        /// </remarks>
        public static DomainAndUri? SelectDomain(IEnumerable<Domain>? domains, Uri uri, string? culture = null, string? defaultCulture = null, Func<IReadOnlyCollection<DomainAndUri>, Uri, string?, string?, DomainAndUri?>? filter = null)
        {
            // sanitize the list to have proper uris for comparison (scheme, path end with /)
            // we need to end with / because example.com/foo cannot match example.com/foobar
            // we need to order so example.com/foo matches before example.com/
            var domainsAndUris = domains?
                .Where(d => d.IsWildcard == false)
                .Select(d => new DomainAndUri(d, uri))
                .OrderByDescending(d => d.Uri.ToString())
                .ToList();

            // nothing = no magic, return null
            if (domainsAndUris is null || domainsAndUris.Count == 0)
            {
                return null;
            }

            // sanitize cultures
            culture = culture?.NullOrWhiteSpaceAsNull();
            defaultCulture = defaultCulture?.NullOrWhiteSpaceAsNull();

            if (uri == null)
            {
                // no uri - will only rely on culture
                return GetByCulture(domainsAndUris, culture, defaultCulture);
            }

            // else we have a uri,
            // try to match that uri, else filter

            // if a culture is specified, then try to get domains for that culture
            // (else cultureDomains will be null)
            // do NOT specify a default culture, else it would pick those domains
            IReadOnlyCollection<DomainAndUri>? cultureDomains = SelectByCulture(domainsAndUris, culture, defaultCulture: null);
            IReadOnlyCollection<DomainAndUri> considerForBaseDomains = domainsAndUris;
            if (cultureDomains != null)
            {
                if (cultureDomains.Count == 1) // only 1, return
                {
                    return cultureDomains.First();
                }

                // else restrict to those domains, for base lookup
                considerForBaseDomains = cultureDomains;
            }

            // look for domains that would be the base of the uri
            IReadOnlyCollection<DomainAndUri> baseDomains = SelectByBase(considerForBaseDomains, uri, culture);
            if (baseDomains.Count > 0) // found, return
            {
                return baseDomains.First();
            }

            // if nothing works, then try to run the filter to select a domain
            // either restricting on cultureDomains, or on all domains
            if (filter != null)
            {
                DomainAndUri? domainAndUri = filter(cultureDomains ?? domainsAndUris, uri, culture, defaultCulture);
                return domainAndUri;
            }

            return null;
        }

        private static bool IsBaseOf(DomainAndUri domain, Uri uri)
            => domain.Uri.EndPathWithSlash().IsBaseOf(uri);

        private static bool MatchesCulture(DomainAndUri domain, string? culture)
            => culture == null || domain.Culture.InvariantEquals(culture);

        private static IReadOnlyCollection<DomainAndUri> SelectByBase(IReadOnlyCollection<DomainAndUri> domainsAndUris, Uri uri, string? culture)
        {
            // look for domains that would be the base of the uri
            // ie current is www.example.com/foo/bar, look for domain www.example.com
            Uri currentWithSlash = uri.EndPathWithSlash();
            var baseDomains = domainsAndUris.Where(d => IsBaseOf(d, currentWithSlash) && MatchesCulture(d, culture)).ToList();

            // if none matches, try again without the port
            // ie current is www.example.com:1234/foo/bar, look for domain www.example.com
            Uri currentWithoutPort = currentWithSlash.WithoutPort();
            if (baseDomains.Count == 0)
            {
                baseDomains = domainsAndUris.Where(d => IsBaseOf(d, currentWithoutPort)).ToList();
            }

            return baseDomains;
        }

        private static IReadOnlyCollection<DomainAndUri>? SelectByCulture(IReadOnlyCollection<DomainAndUri> domainsAndUris, string? culture, string? defaultCulture)
        {
            // we try our best to match cultures, but may end with a bogus domain

            if (culture != null) // try the supplied culture
            {
                var cultureDomains = domainsAndUris.Where(x => x.Culture.InvariantEquals(culture)).ToList();
                if (cultureDomains.Count > 0)
                {
                    return cultureDomains;
                }
            }

            if (defaultCulture != null) // try the defaultCulture culture
            {
                var cultureDomains = domainsAndUris.Where(x => x.Culture.InvariantEquals(defaultCulture)).ToList();
                if (cultureDomains.Count > 0)
                {
                    return cultureDomains;
                }
            }

            return null;
        }

        private static DomainAndUri GetByCulture(IReadOnlyCollection<DomainAndUri> domainsAndUris, string? culture, string? defaultCulture)
        {
            DomainAndUri? domainAndUri;

            // we try our best to match cultures, but may end with a bogus domain

            if (culture != null) // try the supplied culture
            {
                domainAndUri = domainsAndUris.FirstOrDefault(x => x.Culture.InvariantEquals(culture));
                if (domainAndUri != null)
                {
                    return domainAndUri;
                }
            }

            if (defaultCulture != null) // try the defaultCulture culture
            {
                domainAndUri = domainsAndUris.FirstOrDefault(x => x.Culture.InvariantEquals(defaultCulture));
                if (domainAndUri != null)
                {
                    return domainAndUri;
                }
            }

            return domainsAndUris.First(); // what else?
        }

        /// <summary>
        /// Selects the domains that match a specified uri, from a set of domains.
        /// </summary>
        /// <param name="domains">The domains.</param>
        /// <param name="uri">The uri, or null.</param>
        /// <returns>The domains and their normalized uris, that match the specified uri.</returns>
        internal static IEnumerable<DomainAndUri> SelectDomains(IEnumerable<Domain> domains, Uri uri)
        {
            // TODO: where are we matching ?!!?
            return domains
                .Where(d => d.IsWildcard == false)
                .Select(d => new DomainAndUri(d, uri))
                .OrderByDescending(d => d.Uri.ToString());
        }

        /// <summary>
        /// Parses a domain name into a URI.
        /// </summary>
        /// <param name="domainName">The domain name to parse</param>
        /// <param name="currentUri">The currently requested URI. If the domain name is relative, the authority of URI will be used.</param>
        /// <returns>The domain name as a URI</returns>
        public static Uri ParseUriFromDomainName(string domainName, Uri currentUri)
        {
            // turn "/en" into "http://whatever.com/en" so it becomes a parseable uri
            var name = domainName.StartsWith("/") && currentUri != null
                ? currentUri.GetLeftPart(UriPartial.Authority) + domainName
                : domainName;
            var scheme = currentUri?.Scheme ?? Uri.UriSchemeHttp;
            return new Uri(UriUtilityCore.TrimPathEndSlash(UriUtilityCore.StartWithScheme(name, scheme)));
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets a value indicating whether there is another domain defined down in the path to a node under the current domain's root node.
        /// </summary>
        /// <param name="domains">The domains.</param>
        /// <param name="path">The path to a node under the current domain's root node eg '-1,1234,5678'.</param>
        /// <param name="rootNodeId">The current domain root node identifier, or null.</param>
        /// <returns>A value indicating if there is another domain defined down in the path.</returns>
        /// <remarks>Looks _under_ rootNodeId but not _at_ rootNodeId.</remarks>
        internal static bool ExistsDomainInPath(IEnumerable<Domain> domains, string path, int? rootNodeId)
        {
            return FindDomainInPath(domains, path, rootNodeId) != null;
        }

        /// <summary>
        /// Gets the deepest non-wildcard Domain, if any, from a group of Domains, in a node path.
        /// </summary>
        /// <param name="domains">The domains.</param>
        /// <param name="path">The node path eg '-1,1234,5678'.</param>
        /// <param name="rootNodeId">The current domain root node identifier, or null.</param>
        /// <returns>The deepest non-wildcard Domain in the path, or null.</returns>
        /// <remarks>Looks _under_ rootNodeId but not _at_ rootNodeId.</remarks>
        internal static Domain? FindDomainInPath(IEnumerable<Domain> domains, string path, int? rootNodeId)
        {
            var stopNodeId = rootNodeId ?? -1;

            return path.Split(Constants.CharArrays.Comma)
                       .Reverse()
                       .Select(s => int.Parse(s, CultureInfo.InvariantCulture))
                       .TakeWhile(id => id != stopNodeId)
                       .Select(id => domains.FirstOrDefault(d => d.ContentId == id && d.IsWildcard == false))
                       .SkipWhile(domain => domain == null)
                       .FirstOrDefault();
        }

        /// <summary>
        /// Gets the deepest wildcard Domain, if any, from a group of Domains, in a node path.
        /// </summary>
        /// <param name="domains">The domains.</param>
        /// <param name="path">The node path eg '-1,1234,5678'.</param>
        /// <param name="rootNodeId">The current domain root node identifier, or null.</param>
        /// <returns>The deepest wildcard Domain in the path, or null.</returns>
        /// <remarks>Looks _under_ rootNodeId but not _at_ rootNodeId.</remarks>
        public static Domain? FindWildcardDomainInPath(IEnumerable<Domain>? domains, string path, int? rootNodeId)
        {
            var stopNodeId = rootNodeId ?? -1;

            return path.Split(Constants.CharArrays.Comma)
                       .Reverse()
                       .Select(s => int.Parse(s, CultureInfo.InvariantCulture))
                       .TakeWhile(id => id != stopNodeId)
                       .Select(id => domains?.FirstOrDefault(d => d.ContentId == id && d.IsWildcard))
                       .FirstOrDefault(domain => domain != null);
        }

        /// <summary>
        /// Returns the part of a path relative to the uri of a domain.
        /// </summary>
        /// <param name="domainUri">The normalized uri of the domain.</param>
        /// <param name="path">The full path of the uri.</param>
        /// <returns>The path part relative to the uri of the domain.</returns>
        /// <remarks>Eg the relative part of <c>/foo/bar/nil</c> to domain <c>example.com/foo</c> is <c>/bar/nil</c>.</remarks>
        public static string PathRelativeToDomain(Uri domainUri, string path)
            => path.Substring(domainUri.GetAbsolutePathDecoded().Length).EnsureStartsWith('/');

        #endregion
    }
}
