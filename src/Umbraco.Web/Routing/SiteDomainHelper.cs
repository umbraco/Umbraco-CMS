using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;
using Umbraco.Core;
using System.ComponentModel;

namespace Umbraco.Web.Routing
{
     /// <summary>
    /// Provides utilities to handle site domains.
     /// </summary>
    public class SiteDomainHelper : ISiteDomainHelper, IDisposable
    {
        #region Configure

        private static readonly ReaderWriterLockSlim ConfigLock = new ReaderWriterLockSlim();
        private static Dictionary<string, string[]> _sites;
        private static Dictionary<string, List<string>> _bindings;
        private static Dictionary<string, Dictionary<string, string[]>> _qualifiedSites;
        private bool _disposedValue;

        // these are for unit tests *only*
        // ReSharper disable ConvertToAutoPropertyWithPrivateSetter
        internal static Dictionary<string, string[]> Sites => _sites;
        internal static Dictionary<string, List<string>> Bindings => _bindings;
        // ReSharper restore ConvertToAutoPropertyWithPrivateSetter

        // these are for validation
        //private const string DomainValidationSource = @"^(\*|((?i:http[s]?://)?([-\w]+(\.[-\w]+)*)(:\d+)?(/[-\w]*)?))$";
        private const string DomainValidationSource = @"^(((?i:http[s]?://)?([-\w]+(\.[-\w]+)*)(:\d+)?(/)?))$";
        private static readonly Regex DomainValidation = new Regex(DomainValidationSource, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected static IDisposable ConfigWriteLock => new WriteLock(ConfigLock);

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected static IDisposable ConfigReadLock => new ReadLock(ConfigLock);

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
                        throw new ArgumentOutOfRangeException(nameof(domains), $"Invalid domain: \"{domain}\".");
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
                if (_sites == null || !_sites.ContainsKey(key))
                    return;

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
                    throw new ArgumentException($"Not an existing site key: {key}.", nameof(keys));

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

        /// <inheritdoc />
        public virtual DomainAndUri MapDomain(IReadOnlyCollection<DomainAndUri> domainAndUris, Uri current, string culture, string defaultCulture)
        {
            var currentAuthority = current.GetLeftPart(UriPartial.Authority);
            var qualifiedSites = GetQualifiedSites(current);

            return MapDomain(domainAndUris, qualifiedSites, currentAuthority, culture, defaultCulture);
        }

        /// <inheritdoc />
        public virtual IEnumerable<DomainAndUri> MapDomains(IReadOnlyCollection<DomainAndUri> domainAndUris, Uri current, bool excludeDefault, string culture, string defaultCulture)
        {
            // TODO: ignoring cultures entirely?

            var currentAuthority = current.GetLeftPart(UriPartial.Authority);
            KeyValuePair<string, string[]>[] candidateSites = null;
             IEnumerable<DomainAndUri> ret = domainAndUris;

            using (ConfigReadLock) // so nothing changes between GetQualifiedSites and access to bindings
            {
                var qualifiedSites = GetQualifiedSitesInsideLock(current);

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
                        var mainDomain = MapDomain(domainAndUris, qualifiedSites, currentAuthority, culture, defaultCulture); // what GetUrl would get
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
                    var authority = d.Uri.GetLeftPart(UriPartial.Authority);
                    return candidateSites.Any(site => site.Value.Contains(authority));
                });
        }

        private static Dictionary<string, string[]> GetQualifiedSites(Uri current)
        {
            using (ConfigReadLock)
            {
                return GetQualifiedSitesInsideLock(current);
            }
        }

        private static Dictionary<string, string[]> GetQualifiedSitesInsideLock(Uri current)
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
                    kvp => kvp.Value.Select(d => new Uri(UriUtility.StartWithScheme(d, current.Scheme)).GetLeftPart(UriPartial.Authority)).ToArray()
                );

            // .ToDictionary will evaluate and create the dictionary immediately
            // the new value is .ToArray so it will also be evaluated immediately
            // therefore it is safe to return and exit the configuration lock
        }

        private static DomainAndUri MapDomain(IReadOnlyCollection<DomainAndUri> domainAndUris, Dictionary<string, string[]> qualifiedSites, string currentAuthority, string culture, string defaultCulture)
        {
            if (domainAndUris == null) throw new ArgumentNullException(nameof(domainAndUris));
            if (domainAndUris.Count == 0) throw new ArgumentException("Cannot be empty.", nameof(domainAndUris));

            // TODO: how shall we deal with cultures?

            // we do our best, but can't do the impossible
            // get the "default" domain ie the first one for the culture, else the first one (exists, length > 0)
            if (qualifiedSites == null)
                return domainAndUris.FirstOrDefault(x => x.Culture.Name.InvariantEquals(culture)) ??
                       domainAndUris.FirstOrDefault(x => x.Culture.Name.InvariantEquals(defaultCulture)) ??
                       domainAndUris.First();

            // find a site that contains the current authority
            var currentSite = qualifiedSites.FirstOrDefault(site => site.Value.Contains(currentAuthority));

            // if current belongs to a site - try to pick the first element
            // from domainAndUris that also belongs to that site
            var ret = currentSite.Equals(default(KeyValuePair<string, string[]>))
                ? null
                : domainAndUris.FirstOrDefault(d => currentSite.Value.Contains(d.Uri.GetLeftPart(UriPartial.Authority)));

            // no match means that either current does not belong to a site, or the site it belongs to
            // does not contain any of domainAndUris. Yet we have to return something. here, it becomes
            // a bit arbitrary.

            // look through sites in order and pick the first domainAndUri that belongs to a site
            ret = ret ?? qualifiedSites
                .Where(site => site.Key != currentSite.Key)
                .Select(site => domainAndUris.FirstOrDefault(domainAndUri => site.Value.Contains(domainAndUri.Uri.GetLeftPart(UriPartial.Authority))))
                .FirstOrDefault(domainAndUri => domainAndUri != null);

            // random, really
            ret = ret ?? domainAndUris.FirstOrDefault(x => x.Culture.Name.InvariantEquals(culture)) ?? domainAndUris.First();

            return ret;
        }


        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // This is pretty nasty disposing a static on an instance but it's because this whole class
                    // is pretty fubar. I'm sure we've fixed this all up in netcore now? We need to remove all statics.
                    ConfigLock.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}
