using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Web.PublishedCache; // published snapshot

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides utilities to handle domains.
    /// </summary>
    public class DomainHelper
    {
        private readonly IDomainCache _domainCache;
        private readonly ISiteDomainHelper _siteDomainHelper;

        public DomainHelper(IDomainCache domainCache, ISiteDomainHelper siteDomainHelper)
        {
            _domainCache = domainCache;
            _siteDomainHelper = siteDomainHelper;
        }

        #region Domain for Node

        /// <summary>
        /// Finds the domain for the specified node, if any, that best matches a specified uri.
        /// </summary>
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
        internal DomainAndUri DomainForNode(int nodeId, Uri current, string culture = null)
        {
            // be safe
            if (nodeId <= 0)
                return null;

            // get the domains on that node
            var domains = _domainCache.GetAssigned(nodeId, false).ToArray();

            // none?
            if (domains.Length == 0)
                return null;

            // else filter
            // it could be that none apply (due to culture)
            return SelectDomain(domains, current, culture, _domainCache.DefaultCulture,
                (cdomainAndUris, ccurrent, cculture, cdefaultCulture) => _siteDomainHelper.MapDomain(cdomainAndUris, ccurrent, cculture, cdefaultCulture));
        }

        /// <summary>
        /// Gets a value indicating whether a specified node has domains.
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        /// <returns>True if the node has domains, else false.</returns>
        internal bool NodeHasDomains(int nodeId)
        {
            return nodeId > 0 && _domainCache.GetAssigned(nodeId, false).Any();
        }

        /// <summary>
        /// Find the domains for the specified node, if any, that match a specified uri.
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        /// <param name="current">The uri, or null.</param>
        /// <param name="excludeDefault">A value indicating whether to exclude the current/default domain. True by default.</param>
        /// <returns>The domains and their uris, that match the specified uri, else null.</returns>
        /// <remarks>If at least a domain is set on the node then the method returns the domains that
        /// best match the specified uri, else it returns null.</remarks>
        internal IEnumerable<DomainAndUri> DomainsForNode(int nodeId, Uri current, bool excludeDefault = true)
        {
            // be safe
            if (nodeId <= 0)
                return null;

            // get the domains on that node
            var domains = _domainCache.GetAssigned(nodeId, false).ToArray();

            // none?
            if (domains.Length == 0)
                return null;

            // get the domains and their uris
            var domainAndUris = SelectDomains(domains, current).ToArray();

            // filter
            return _siteDomainHelper.MapDomains(domainAndUris, current, excludeDefault, null, _domainCache.DefaultCulture).ToArray();
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
        /// fixme - must document and explain this all
        /// <para>If <paramref name="uri"/> is null, pick the first domain that matches <paramref name="culture"/>,
        /// else the first that matches <paramref name="defaultCulture"/>, else the first one (ordered by id), else null.</para>
        /// <para>If <paramref name="uri"/> is not null, look for domains that would be a base uri of the current uri,</para>
        /// <para>If more than one domain matches, then the <paramref name="filter"/> function is used to pick
        /// the right one, unless it is <c>null</c>, in which case the method returns <c>null</c>.</para>
        /// <para>The filter, if any, will be called only with a non-empty argument, and _must_ return something.</para>
        /// </remarks>
        internal static DomainAndUri SelectDomain(IEnumerable<Domain> domains, Uri uri, string culture = null, string defaultCulture = null, Func<IReadOnlyCollection<DomainAndUri>, Uri, string, string, DomainAndUri> filter = null)
        {
            // sanitize the list to have proper uris for comparison (scheme, path end with /)
            // we need to end with / because example.com/foo cannot match example.com/foobar
            // we need to order so example.com/foo matches before example.com/
            var domainsAndUris = domains
                .Where(d => d.IsWildcard == false)
                .Select(d => new DomainAndUri(d, uri))
                .OrderByDescending(d => d.Uri.ToString())
                .ToList();

            // nothing = no magic, return null
            if (domainsAndUris.Count == 0)
                return null;

            // sanitize cultures
            culture = culture.NullOrWhiteSpaceAsNull();
            defaultCulture = defaultCulture.NullOrWhiteSpaceAsNull();

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
            var cultureDomains = SelectByCulture(domainsAndUris, culture, defaultCulture: null);
            IReadOnlyCollection<DomainAndUri> considerForBaseDomains = domainsAndUris;
            if (cultureDomains != null)
            {
                if (cultureDomains.Count == 1) // only 1, return
                    return cultureDomains.First();

                // else restrict to those domains, for base lookup
                considerForBaseDomains = cultureDomains;
            }

            // look for domains that would be the base of the uri
            var baseDomains = SelectByBase(considerForBaseDomains, uri);
            if (baseDomains.Count > 0) // found, return
                return baseDomains.First();

            // if nothing works, then try to run the filter to select a domain
            // either restricting on cultureDomains, or on all domains
            if (filter != null)
            {
                var domainAndUri = filter(cultureDomains ?? domainsAndUris, uri, culture, defaultCulture);
                // if still nothing, pick the first one?
                // no: move that constraint to the filter, but check
                if (domainAndUri == null)
                    throw new InvalidOperationException("The filter returned null.");
                return domainAndUri;
            }

            return null;
        }

        private static bool IsBaseOf(DomainAndUri domain, Uri uri)
            => domain.Uri.EndPathWithSlash().IsBaseOf(uri);

        private static IReadOnlyCollection<DomainAndUri> SelectByBase(IReadOnlyCollection<DomainAndUri> domainsAndUris, Uri uri)
        {
            // look for domains that would be the base of the uri
            // ie current is www.example.com/foo/bar, look for domain www.example.com
            var currentWithSlash = uri.EndPathWithSlash();
            var baseDomains = domainsAndUris.Where(d => IsBaseOf(d, currentWithSlash)).ToList();

            // if none matches, try again without the port
            // ie current is www.example.com:1234/foo/bar, look for domain www.example.com
            var currentWithoutPort = currentWithSlash.WithoutPort();
            if (baseDomains.Count == 0)
                baseDomains = domainsAndUris.Where(d => IsBaseOf(d, currentWithoutPort)).ToList();

            return baseDomains;
        }

        private static IReadOnlyCollection<DomainAndUri> SelectByCulture(IReadOnlyCollection<DomainAndUri> domainsAndUris, string culture, string defaultCulture)
        {
            // we try our best to match cultures, but may end with a bogus domain

            if (culture != null) // try the supplied culture
            {
                var cultureDomains = domainsAndUris.Where(x => x.Culture.Name.InvariantEquals(culture)).ToList();
                if (cultureDomains.Count > 0) return cultureDomains;
            }

            if (defaultCulture != null) // try the defaultCulture culture
            {
                var cultureDomains = domainsAndUris.Where(x => x.Culture.Name.InvariantEquals(defaultCulture)).ToList();
                if (cultureDomains.Count > 0) return cultureDomains;
            }

            return null;
        }

        private static DomainAndUri GetByCulture(IReadOnlyCollection<DomainAndUri> domainsAndUris, string culture, string defaultCulture)
        {
            DomainAndUri domainAndUri;

            // we try our best to match cultures, but may end with a bogus domain

            if (culture != null) // try the supplied culture
            {
                domainAndUri = domainsAndUris.FirstOrDefault(x => x.Culture.Name.InvariantEquals(culture));
                if (domainAndUri != null) return domainAndUri;
            }

            if (defaultCulture != null) // try the defaultCulture culture
            {
                domainAndUri = domainsAndUris.FirstOrDefault(x => x.Culture.Name.InvariantEquals(defaultCulture));
                if (domainAndUri != null) return domainAndUri;
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
            // fixme where are we matching ?!!?
            return domains
                .Where(d => d.IsWildcard == false)
                .Select(d => new DomainAndUri(d, uri))
                .OrderByDescending(d => d.Uri.ToString());
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
        internal static Domain FindDomainInPath(IEnumerable<Domain> domains, string path, int? rootNodeId)
        {
            var stopNodeId = rootNodeId ?? -1;

            return path.Split(',')
                       .Reverse()
                       .Select(int.Parse)
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
        internal static Domain FindWildcardDomainInPath(IEnumerable<Domain> domains, string path, int? rootNodeId)
        {
            var stopNodeId = rootNodeId ?? -1;

            return path.Split(',')
                       .Reverse()
                       .Select(int.Parse)
                       .TakeWhile(id => id != stopNodeId)
                       .Select(id => domains.FirstOrDefault(d => d.ContentId == id && d.IsWildcard))
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
        {
            return path.Substring(domainUri.GetAbsolutePathDecoded().Length).EnsureStartsWith('/');
        }

        #endregion
    }
}
