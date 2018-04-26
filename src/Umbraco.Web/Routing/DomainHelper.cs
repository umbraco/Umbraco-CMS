using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Web.Composing;
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
        /// <returns>The domain and its uri, if any, that best matches the specified uri, else null.</returns>
        /// <remarks>If at least a domain is set on the node then the method returns the domain that
        /// best matches the specified uri, else it returns null.</remarks>
        internal DomainAndUri DomainForNode(int nodeId, Uri current)
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
            var domainAndUri = DomainForUri(domains, current, domainAndUris => _siteDomainHelper.MapDomain(current, domainAndUris));

            if (domainAndUri == null)
                throw new Exception("DomainForUri returned null.");

            return domainAndUri;
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
            var domainAndUris = DomainsForUri(domains, current).ToArray();

            // filter
            return _siteDomainHelper.MapDomains(current, domainAndUris, excludeDefault).ToArray();
        }

        #endregion

        #region Domain for Uri

        /// <summary>
        /// Finds the domain that best matches a specified uri, into a group of domains.
        /// </summary>
        /// <param name="domains">The group of domains.</param>
        /// <param name="current">The uri, or null.</param>
        /// <param name="filter">A function to filter the list of domains, if more than one applies, or <c>null</c>.</param>
        /// <returns>The domain and its normalized uri, that best matches the specified uri.</returns>
        /// <remarks>
        /// <para>If more than one domain matches, then the <paramref name="filter"/> function is used to pick
        /// the right one, unless it is <c>null</c>, in which case the method returns <c>null</c>.</para>
        /// <para>The filter, if any, will be called only with a non-empty argument, and _must_ return something.</para>
        /// </remarks>
        internal static DomainAndUri DomainForUri(IEnumerable<Domain> domains, Uri current, Func<DomainAndUri[], DomainAndUri> filter = null)
        {
            // sanitize the list to have proper uris for comparison (scheme, path end with /)
            // we need to end with / because example.com/foo cannot match example.com/foobar
            // we need to order so example.com/foo matches before example.com/
            var domainsAndUris = domains
                .Where(d => d.IsWildcard == false)
                //.Select(SanitizeForBackwardCompatibility)
                .Select(d => new DomainAndUri(d, current))
                .OrderByDescending(d => d.Uri.ToString())
                .ToArray();

            if (domainsAndUris.Length == 0)
                return null;

            DomainAndUri domainAndUri;
            if (current == null)
            {
                //get the default domain (there should be one)
                domainAndUri = domainsAndUris.FirstOrDefault(x => x.IsDefault);
                if (domainAndUri == null)
                    domainAndUri = domainsAndUris.First(); // take the first one by default (what else can we do?)
            }
            else
            {
                // look for the first domain that would be the base of the current url
                // ie current is www.example.com/foo/bar, look for domain www.example.com
                var currentWithSlash = current.EndPathWithSlash();
                domainAndUri = domainsAndUris
                    .FirstOrDefault(d => d.Uri.EndPathWithSlash().IsBaseOf(currentWithSlash));
                if (domainAndUri != null) return domainAndUri;

                // if none matches, try again without the port
                // ie current is www.example.com:1234/foo/bar, look for domain www.example.com
                domainAndUri = domainsAndUris
                    .FirstOrDefault(d => d.Uri.EndPathWithSlash().IsBaseOf(currentWithSlash.WithoutPort()));
                if (domainAndUri != null) return domainAndUri;

                // if none matches, then try to run the filter to pick a domain
                if (filter != null)
                {
                    domainAndUri = filter(domainsAndUris);
                    // if still nothing, pick the first one?
                    // no: move that constraint to the filter, but check
                    if (domainAndUri == null)
                        throw new InvalidOperationException("The filter returned null.");
                }
            }

            return domainAndUri;
        }

        /// <summary>
        /// Gets the domains that match a specified uri, into a group of domains.
        /// </summary>
        /// <param name="domains">The group of domains.</param>
        /// <param name="current">The uri, or null.</param>
        /// <returns>The domains and their normalized uris, that match the specified uri.</returns>
        internal static IEnumerable<DomainAndUri> DomainsForUri(IEnumerable<Domain> domains, Uri current)
        {
            return domains
                .Where(d => d.IsWildcard == false)
                //.Select(SanitizeForBackwardCompatibility)
                .Select(d => new DomainAndUri(d, current))
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
