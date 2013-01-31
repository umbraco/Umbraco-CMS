using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides utilities to handle domains.
	/// </summary>
	internal class DomainHelper
	{
		private static bool IsWildcardDomain(Domain d)
		{
			// supporting null or whitespace for backward compatibility, 
			// although we should not allow ppl to create them anymore
			return string.IsNullOrWhiteSpace(d.Name) || d.Name.StartsWith("*");
		}

		private static Domain SanitizeForBackwardCompatibility(Domain d)
		{
			// this is a _really_ nasty one that should be removed in 6.x
			// some people were using hostnames such as "/en" which happened to work pre-4.10
			// but make _no_ sense at all... and 4.10 throws on them, so here we just try
			// to find a way so 4.11 does not throw.
			// but, really.
			// no.
			var context = System.Web.HttpContext.Current;
			if (context != null && d.Name.StartsWith("/"))
			{
				// turn /en into http://whatever.com/en so it becomes a parseable uri
				var authority = context.Request.Url.GetLeftPart(UriPartial.Authority);
				d.Name = authority + d.Name;
			}
			return d;
		}

        /// <summary>
        /// Finds the domain that best matches the current uri, into an enumeration of domains.
        /// </summary>
        /// <param name="domains">The enumeration of Umbraco domains.</param>
        /// <param name="current">The uri of the current request, or null.</param>
        /// <param name="filter">A function to filter the list of domains, if more than one applies, or <c>null</c>.</param>
        /// <returns>The domain and its normalized uri, that best matches the current uri.</returns>
        /// <remarks>
        /// <para>If more than one domain matches, then the <paramref name="filter"/> function is used to pick
        /// the right one, unless it is <c>null</c>, in which case the method returns <c>null</c>.</para>
        /// <para>The filter, if any, will be called only with a non-empty argument, and _must_ return something.</para>
        /// </remarks>
        public static DomainAndUri DomainMatch(Domain[] domains, Uri current, Func<DomainAndUri[], DomainAndUri> filter = null)
        {
            // sanitize the list to have proper uris for comparison (scheme, path end with /)
            // we need to end with / because example.com/foo cannot match example.com/foobar
            // we need to order so example.com/foo matches before example.com/
            var scheme = current == null ? Uri.UriSchemeHttp : current.Scheme;
            var domainsAndUris = domains
                .Where(d => !IsWildcardDomain(d))
                .Select(SanitizeForBackwardCompatibility)
                .Select(d => new { Domain = d, UriString = UriUtility.EndPathWithSlash(UriUtility.StartWithScheme(d.Name, scheme)) })
                .OrderByDescending(t => t.UriString)
                .Select(t => new DomainAndUri(t.Domain, new Uri(t.UriString)))
                .ToArray();

            if (!domainsAndUris.Any())
                return null;

            DomainAndUri domainAndUri;
            if (current == null)
            {
                // take the first one by default (is that OK?)
                domainAndUri = domainsAndUris.First();
            }
            else
            {
                // look for a domain that would be the base of the hint
                // assume only one can match the hint (is that OK?)
                var hintWithSlash = current.EndPathWithSlash();
                domainAndUri = domainsAndUris
                    .FirstOrDefault(t => t.Uri.IsBaseOf(hintWithSlash));
                // if none matches, then try to run the filter to sort them out
                if (domainAndUri == null && filter != null)
                {
                    domainAndUri = filter(domainsAndUris);
                    // if still nothing, pick the first one?
                    // no: move that constraint to the filter, but check
                    if (domainAndUri == null)
                        throw new InvalidOperationException("The filter returned null.");
                }
            }

            if (domainAndUri != null)
                domainAndUri.Uri = domainAndUri.Uri.TrimPathEndSlash();
            return domainAndUri;
        }

		/// <summary>
		/// Gets an enumeration of <see cref="DomainAndUri"/> matching an enumeration of Umbraco domains.
		/// </summary>
		/// <param name="domains">The enumeration of Umbraco domains.</param>
		/// <param name="current">The uri of the current request, or null.</param>
		/// <returns>The enumeration of <see cref="DomainAndUri"/> matching the enumeration of Umbraco domains.</returns>
		public static IEnumerable<DomainAndUri> DomainMatches(Domain[] domains, Uri current)
		{
            var scheme = current == null ? Uri.UriSchemeHttp : current.Scheme;
			var domainsAndUris = domains
				.Where(d => !IsWildcardDomain(d))
				.Select(SanitizeForBackwardCompatibility)
				.Select(d => new { Domain = d, UriString = UriUtility.TrimPathEndSlash(UriUtility.StartWithScheme(d.Name, scheme)) })
				.OrderByDescending(t => t.UriString)
				.Select(t => new DomainAndUri(t.Domain, new Uri(t.UriString)));
			return domainsAndUris;
		}

		/// <summary>
		/// Gets a value indicating whether there is another domain defined down in the path to a node under the current domain's root node.
		/// </summary>
		/// <param name="current">The current domain.</param>
		/// <param name="path">The path to a node under the current domain's root node.</param>
		/// <returns>A value indicating if there is another domain defined down in the path.</returns>
		public static bool ExistsDomainInPath(Domain current, string path)
		{
			var domains = Domain.GetDomains();
			var stopNodeId = current == null ? -1 : current.RootNodeId;

			return path.Split(',')
				.Reverse()
				.Select(int.Parse)
				.TakeWhile(id => id != stopNodeId)
				.Any(id => domains.Any(d => d.RootNodeId == id && !IsWildcardDomain(d)));
		}

		/// <summary>
		/// Gets the deepest wildcard <see cref="Domain"/> in a node path.
		/// </summary>
		/// <param name="domains">The Umbraco domains.</param>
		/// <param name="path">The node path.</param>
		/// <param name="rootNodeId">The current domain root node identifier, or null.</param>
		/// <returns>The deepest wildcard <see cref="Domain"/> in the path, or null.</returns>
		public static Domain LookForWildcardDomain(Domain[] domains, string path, int? rootNodeId)
		{
            // "When you perform comparisons with nullable types, if the value of one of the nullable
            // types is null and the other is not, all comparisons evaluate to false."

            return path
                .Split(',')
                .Select(int.Parse)
                .Skip(1)
                .Reverse()
                .TakeWhile(id => !rootNodeId.HasValue || id != rootNodeId)
                .Select(nodeId => domains.FirstOrDefault(d => d.RootNodeId == nodeId && IsWildcardDomain(d)))
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
			return path.Substring(domainUri.AbsolutePath.Length).EnsureStartsWith('/');
		}
	}
}
