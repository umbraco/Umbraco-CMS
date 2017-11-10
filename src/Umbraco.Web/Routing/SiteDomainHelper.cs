using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides utilities to handle site domains.
    /// </summary>
    public class SiteDomainHelper : ISiteDomainHelper2
    {
        #region Configure

        private static readonly ReaderWriterLockSlim ConfigLock = new ReaderWriterLockSlim();
        private static Dictionary<string, string[]> _sites;
        private static Dictionary<string, List<string>> _bindings;
        private static Dictionary<string, Dictionary<string, string[]>> _qualifiedSites;

        // these are for unit tests *only*
        internal static Dictionary<string, string[]> Sites { get { return _sites; } }
        internal static Dictionary<string, List<string>> Bindings { get { return _bindings; } }

        // these are for validation
        //private const string DomainValidationSource = @"^(\*|((?i:http[s]?://)?([-\w]+(\.[-\w]+)*)(:\d+)?(/[-\w]*)?))$";
        private const string DomainValidationSource = @"^(((?i:http[s]?://)?([-\w]+(\.[-\w]+)*)(:\d+)?(/)?))$";
        private static readonly Regex DomainValidation = new Regex(DomainValidationSource, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Returns a disposable object that represents safe write access to config.
        /// </summary>
        /// <remarks>Should be used in a <c>using(SiteDomainHelper.ConfigWriteLock) { ... }</c>  mode.</remarks>
        protected static IDisposable ConfigWriteLock
        {
            get { return new WriteLock(ConfigLock); }
        }

        /// <summary>
        /// Returns a disposable object that represents safe read access to config.
        /// </summary>
        /// <remarks>Should be used in a <c>using(SiteDomainHelper.ConfigWriteLock) { ... }</c>  mode.</remarks>
        protected static IDisposable ConfigReadLock
        {
            get { return new ReadLock(ConfigLock); }
        }

        /// <summary>
        /// Clears the entire configuration.
        /// </summary>
        public static void Clear()
        {
            using (ConfigWriteLock)
            {
                _sites = null;
                _bindings = null;
                _qualifiedSites = null;
            }
        }

        private static IEnumerable<string> ValidateDomains(IEnumerable<string> domains)
        {
            // must use authority format w/optional scheme and port, but no path
            // any domain should appear only once
            return domains.Select(domain =>
                {
                    if (!DomainValidation.IsMatch(domain))
                        throw new ArgumentOutOfRangeException("domains", string.Format("Invalid domain: \"{0}\"", domain));
                    return domain;
                });
        }

        /// <summary>
        /// Adds a site.
        /// </summary>
        /// <param name="key">A key uniquely identifying the site.</param>
        /// <param name="domains">The site domains.</param>
        /// <remarks>At the moment there is no public way to remove a site. Clear and reconfigure.</remarks>
        public static void AddSite(string key, IEnumerable<string> domains)
        {
            using (ConfigWriteLock)
            {
                _sites = _sites ?? new Dictionary<string, string[]>();
                _sites[key] = ValidateDomains(domains).ToArray();
                _qualifiedSites = null;
            }
        }

        /// <summary>
        /// Adds a site.
        /// </summary>
        /// <param name="key">A key uniquely identifying the site.</param>
        /// <param name="domains">The site domains.</param>
        /// <remarks>At the moment there is no public way to remove a site. Clear and reconfigure.</remarks>
        public static void AddSite(string key, params string[] domains)
        {
            using (ConfigWriteLock)
            {
                _sites = _sites ?? new Dictionary<string, string[]>();
                _sites[key] = ValidateDomains(domains).ToArray();
                _qualifiedSites = null;
            }
        }

        /// <summary>
        /// Removes a site.
        /// </summary>
        /// <param name="key">A key uniquely identifying the site.</param>
        internal static void RemoveSite(string key)
        {
            using (ConfigWriteLock)
            {
                if (_sites != null && _sites.ContainsKey(key))
                {
                    _sites.Remove(key);
                    if (_sites.Count == 0)
                        _sites = null;

                    if (_bindings != null && _bindings.ContainsKey(key))
                    {
                        foreach (var b in _bindings[key])
                        {
                            _bindings[b].Remove(key);
                            if (_bindings[b].Count == 0)
                                _bindings.Remove(b);
                        }
                        _bindings.Remove(key);
                        if (_bindings.Count > 0)
                            _bindings = null;
                    }

                    _qualifiedSites = null;
                }
            }
        }

        /// <summary>
        /// Binds some sites.
        /// </summary>
        /// <param name="keys">The keys uniquely identifying the sites to bind.</param>
        /// <remarks>
        /// <para>At the moment there is no public way to unbind sites. Clear and reconfigure.</para>
        /// <para>If site1 is bound to site2 and site2 is bound to site3 then site1 is bound to site3.</para>
        /// </remarks>
        public static void BindSites(params string[] keys)
        {
            using (ConfigWriteLock)
            {
                foreach (var key in keys.Where(key => !_sites.ContainsKey(key)))
                    throw new ArgumentException(string.Format("Not an existing site key: {0}", key), "keys");

                _bindings = _bindings ?? new Dictionary<string, List<string>>();

                var allkeys = _bindings
                    .Where(kvp => keys.Contains(kvp.Key))
                    .SelectMany(kvp => kvp.Value)
                    .Union(keys)
                    .ToArray();

                foreach (var key in allkeys)
                {
                    if (!_bindings.ContainsKey(key))
                        _bindings[key] = new List<string>();
                    var xkey = key;
                    var addKeys = allkeys.Where(k => k != xkey).Except(_bindings[key]);
                    _bindings[key].AddRange(addKeys);
                }
            }
        }

        #endregion

        #region Map domains

        [Obsolete("Use the overload specifying HttpRequestBase instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual DomainAndUri MapDomain(Uri current, DomainAndUri[] domainAndUris)
        {
            //NOTE - this method should not be used since it won't work for proxied sites behind load balancers with SSL
            // we no longer use this method in core, it's here for backwards compat only

            return MapDomain(current, null, domainAndUris);
        }

        /// <summary>
        /// Filters a list of <c>DomainAndUri</c> to pick one that best matches the current request.
        /// </summary>
        /// <param name="current">The Uri of the current request.</param>
        /// <param name="httpRequest"></param>
        /// <param name="domainAndUris">The list of <c>DomainAndUri</c> to filter.</param>
        /// <returns>The selected <c>DomainAndUri</c>.</returns>
        /// <remarks>
        /// <para>If the filter is invoked then <paramref name="domainAndUris"/> is _not_ empty and
        /// <paramref name="current"/> is _not_ null, and <paramref name="current"/> could not be
        /// matched with anything in <paramref name="domainAndUris"/>.</para>
        /// <para>The filter _must_ return something else an exception will be thrown.</para>
        /// </remarks>
        public virtual DomainAndUri MapDomain(Uri current, HttpRequestBase httpRequest, DomainAndUri[] domainAndUris)
        {
            var currentAuthority = httpRequest.GetLeftUriPart(current, UriPartial.Authority);
            var qualifiedSites = GetQualifiedSites(current, httpRequest);

            return MapDomain(httpRequest, domainAndUris, qualifiedSites, currentAuthority);
        }

        [Obsolete("Use the overload specifying HttpRequestBase instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerable<DomainAndUri> MapDomains(Uri current, DomainAndUri[] domainAndUris, bool excludeDefault)
        {
            //NOTE - this method should not be used since it won't work for proxied sites behind load balancers with SSL
            // we no longer use this method in core, it's here for backwards compat only

            return MapDomains(current, null, domainAndUris, excludeDefault);
        }

        /// <summary>
        /// Filters a list of <c>DomainAndUri</c> to pick those that best matches the current request.
        /// </summary>
        /// <param name="current">The Uri of the current request.</param>
        /// <param name="httpRequest"></param>
        /// <param name="domainAndUris">The list of <c>DomainAndUri</c> to filter.</param>
        /// <param name="excludeDefault">A value indicating whether to exclude the current/default domain.</param>
        /// <returns>The selected <c>DomainAndUri</c> items.</returns>
        /// <remarks>The filter must return something, even empty, else an exception will be thrown.</remarks>
        public virtual IEnumerable<DomainAndUri> MapDomains(Uri current, HttpRequestBase httpRequest, DomainAndUri[] domainAndUris, bool excludeDefault)
        {
            var currentAuthority = httpRequest.GetLeftUriPart(current, UriPartial.Authority);
            KeyValuePair<string, string[]>[] candidateSites = null;
            IEnumerable<DomainAndUri> ret = domainAndUris;

            using (ConfigReadLock) // so nothing changes between GetQualifiedSites and access to bindings
            {
                var qualifiedSites = GetQualifiedSitesInsideLock(current, httpRequest);

                if (excludeDefault)
                {
                    // exclude the current one (avoid producing the absolute equivalent of what GetUrl returns)
                    var hintWithSlash = current.EndPathWithSlash();
                    var hinted = domainAndUris.FirstOrDefault(d => d.Uri.EndPathWithSlash().IsBaseOf(hintWithSlash));
                    if (hinted != null)
                        ret = ret.Where(d => d != hinted);

                    // exclude the default one (avoid producing a possible duplicate of what GetUrl returns)
                    // only if the default one cannot be the current one ie if hinted is not null
                    if (hinted == null && domainAndUris.Any())
                    {
                        // it is illegal to call MapDomain if domainAndUris is empty
                        // also, domainAndUris should NOT contain current, hence the test on hinted
                        var mainDomain = MapDomain(httpRequest, domainAndUris, qualifiedSites, currentAuthority); // what GetUrl would get
                        ret = ret.Where(d => d != mainDomain);
                    }
                }

                // we do our best, but can't do the impossible
                if (qualifiedSites == null)
                    return ret;

                // find a site that contains the current authority
                var currentSite = qualifiedSites.FirstOrDefault(site => site.Value.Contains(currentAuthority));

                // if current belongs to a site, pick every element from domainAndUris that also belong
                // to that site -- or to any site bound to that site

                if (!currentSite.Equals(default(KeyValuePair<string, string[]>)))
                {
                    candidateSites = new[] { currentSite };
                    if (_bindings != null && _bindings.ContainsKey(currentSite.Key))
                    {
                        var boundSites = qualifiedSites.Where(site => _bindings[currentSite.Key].Contains(site.Key));
                        candidateSites = candidateSites.Union(boundSites).ToArray();

                        // .ToArray ensures it is evaluated before the configuration lock is exited
                    }
                }
            }

            // if we are able to filter, then filter, else return the whole lot
            return candidateSites == null ? ret : ret.Where(d =>
                {
                    var authority = httpRequest.GetLeftUriPart(d.Uri, UriPartial.Authority);
                    return candidateSites.Any(site => site.Value.Contains(authority));
                });
        }

        private static Dictionary<string, string[]> GetQualifiedSites(Uri current, HttpRequestBase httpRequest)
        {
            using (ConfigReadLock)
            {
                return GetQualifiedSitesInsideLock(current, httpRequest);
            }
        }

        private static Dictionary<string, string[]> GetQualifiedSitesInsideLock(Uri current, HttpRequestBase httpRequest)
        {
            // we do our best, but can't do the impossible
            if (_sites == null)
                return null;

            // cached?
            if (_qualifiedSites != null && _qualifiedSites.ContainsKey(current.Scheme))
                return _qualifiedSites[current.Scheme];

            _qualifiedSites = _qualifiedSites ?? new Dictionary<string, Dictionary<string, string[]>>();

            // convert sites into authority sites based upon current scheme
            // because some domains in the sites might not have a scheme -- and cache
            return _qualifiedSites[current.Scheme] = _sites
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(d => GetLeftPartWithBackwardsCompat(httpRequest, new Uri(UriUtility.StartWithScheme(d, current.Scheme)))).ToArray()
                );

            // .ToDictionary will evaluate and create the dictionary immediately
            // the new value is .ToArray so it will also be evaluated immediately
            // therefore it is safe to return and exit the configuration lock
        }

        private static DomainAndUri MapDomain(HttpRequestBase httpRequest, DomainAndUri[] domainAndUris, Dictionary<string, string[]> qualifiedSites, string currentAuthority)
        {
            if (domainAndUris == null)
                throw new ArgumentNullException("domainAndUris");
            if (!domainAndUris.Any())
                throw new ArgumentException("Cannot be empty.", "domainAndUris");

            // we do our best, but can't do the impossible
            if (qualifiedSites == null)
                return domainAndUris.First();

            // find a site that contains the current authority
            var currentSite = qualifiedSites.FirstOrDefault(site => site.Value.Contains(currentAuthority));

            // if current belongs to a site - try to pick the first element 
            // from domainAndUris that also belongs to that site
            var ret = currentSite.Equals(default(KeyValuePair<string, string[]>))
                ? null
                : domainAndUris.FirstOrDefault(d => currentSite.Value.Contains(GetLeftPartWithBackwardsCompat(httpRequest, d.Uri)));

            // no match means that either current does not belong to a site, or the site it belongs to
            // does not contain any of domainAndUris. Yet we have to return something. here, it becomes
            // a bit arbitrary.

            // look through sites in order and pick the first domainAndUri that belongs to a site
            ret = ret ?? qualifiedSites
                .Where(site => site.Key != currentSite.Key)
                .Select(site => domainAndUris.FirstOrDefault(domainAndUri => site.Value.Contains(GetLeftPartWithBackwardsCompat(httpRequest, domainAndUri.Uri))))
                .FirstOrDefault(domainAndUri => domainAndUri != null);

            // random, really
            ret = ret ?? domainAndUris.First();

            return ret;
        }

        #endregion

        /// <summary>
        /// Gets the left part but checks for null on the request for backwards compat reasons
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        private static string GetLeftPartWithBackwardsCompat(HttpRequestBase httpRequest, Uri uri)
        {
            return httpRequest != null
                ? httpRequest.GetLeftUriPart(uri, UriPartial.Authority)
                : uri.GetLeftPart(UriPartial.Authority);
        }
    }
}
