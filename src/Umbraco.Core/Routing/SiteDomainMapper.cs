using System.Text.RegularExpressions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing
{
    /// <summary>
    ///     Provides utilities to handle site domains.
    /// </summary>
    public class SiteDomainMapper : ISiteDomainMapper, IDisposable
    {
        public void Dispose() =>
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // This is pretty nasty disposing a static on an instance but it's because this whole class
                    // is pretty fubar. I'm sure we've fixed this all up in netcore now? We need to remove all statics.
                    _configLock.Dispose();
                }

                _disposedValue = true;
            }
        }

        #region Configure

        private readonly ReaderWriterLockSlim _configLock = new();
        private Dictionary<string, Dictionary<string, string[]>>? _qualifiedSites;
        private bool _disposedValue;

        internal Dictionary<string, string[]>? Sites { get; private set; }
        internal Dictionary<string, List<string>>? Bindings { get; private set; }

        // these are for validation
        //private const string DomainValidationSource = @"^(\*|((?i:http[s]?://)?([-\w]+(\.[-\w]+)*)(:\d+)?(/[-\w]*)?))$";
        private const string DomainValidationSource = @"^(((?i:http[s]?://)?([-\w]+(\.[-\w]+)*)(:\d+)?(/)?))$";

        private static readonly Regex s_domainValidation =
            new(DomainValidationSource, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        ///     Clears the entire configuration.
        /// </summary>
        public void Clear()
        {
            try
            {
                _configLock.EnterWriteLock();

                Sites = null;
                Bindings = null;
                _qualifiedSites = null;
            }
            finally
            {
                if (_configLock.IsWriteLockHeld)
                {
                    _configLock.ExitWriteLock();
                }
            }
        }

        private IEnumerable<string> ValidateDomains(IEnumerable<string> domains) =>
            // must use authority format w/optional scheme and port, but no path
            // any domain should appear only once
            domains.Select(domain =>
            {
                if (!s_domainValidation.IsMatch(domain))
                {
                    throw new ArgumentOutOfRangeException(nameof(domains), $"Invalid domain: \"{domain}\".");
                }

                return domain;
            });

        /// <summary>
        ///     Adds a site.
        /// </summary>
        /// <param name="key">A key uniquely identifying the site.</param>
        /// <param name="domains">The site domains.</param>
        /// <remarks>At the moment there is no public way to remove a site. Clear and reconfigure.</remarks>
        public void AddSite(string key, IEnumerable<string> domains)
        {
            try
            {
                _configLock.EnterWriteLock();

                Sites = Sites ?? new Dictionary<string, string[]>();
                Sites[key] = ValidateDomains(domains).ToArray();
                _qualifiedSites = null;
            }
            finally
            {
                if (_configLock.IsWriteLockHeld)
                {
                    _configLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        ///     Adds a site.
        /// </summary>
        /// <param name="key">A key uniquely identifying the site.</param>
        /// <param name="domains">The site domains.</param>
        /// <remarks>At the moment there is no public way to remove a site. Clear and reconfigure.</remarks>
        public void AddSite(string key, params string[] domains)
        {
            try
            {
                _configLock.EnterWriteLock();

                Sites = Sites ?? new Dictionary<string, string[]>();
                Sites[key] = ValidateDomains(domains).ToArray();
                _qualifiedSites = null;
            }
            finally
            {
                if (_configLock.IsWriteLockHeld)
                {
                    _configLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        ///     Removes a site.
        /// </summary>
        /// <param name="key">A key uniquely identifying the site.</param>
        internal void RemoveSite(string key)
        {
            try
            {
                _configLock.EnterWriteLock();

                if (Sites == null || !Sites.ContainsKey(key))
                {
                    return;
                }

                Sites.Remove(key);
                if (Sites.Count == 0)
                {
                    Sites = null;
                }

                if (Bindings != null && Bindings.ContainsKey(key))
                {
                    foreach (var b in Bindings[key])
                    {
                        Bindings[b].Remove(key);
                        if (Bindings[b].Count == 0)
                        {
                            Bindings.Remove(b);
                        }
                    }

                    Bindings.Remove(key);
                    if (Bindings.Count > 0)
                    {
                        Bindings = null;
                    }
                }

                _qualifiedSites = null;
            }
            finally
            {
                if (_configLock.IsWriteLockHeld)
                {
                    _configLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        ///     Binds some sites.
        /// </summary>
        /// <param name="keys">The keys uniquely identifying the sites to bind.</param>
        /// <remarks>
        ///     <para>At the moment there is no public way to unbind sites. Clear and reconfigure.</para>
        ///     <para>If site1 is bound to site2 and site2 is bound to site3 then site1 is bound to site3.</para>
        /// </remarks>
        public void BindSites(params string[] keys)
        {
            try
            {
                _configLock.EnterWriteLock();

                foreach (var key in keys.Where(key => !Sites?.ContainsKey(key) ?? false))
                {
                    throw new ArgumentException($"Not an existing site key: {key}.", nameof(keys));
                }

                Bindings = Bindings ?? new Dictionary<string, List<string>>();

                var allkeys = Bindings
                    .Where(kvp => keys.Contains(kvp.Key))
                    .SelectMany(kvp => kvp.Value)
                    .Union(keys)
                    .ToArray();

                foreach (var key in allkeys)
                {
                    if (!Bindings.ContainsKey(key))
                    {
                        Bindings[key] = new List<string>();
                    }

                    var xkey = key;
                    IEnumerable<string> addKeys = allkeys.Where(k => k != xkey).Except(Bindings[key]);
                    Bindings[key].AddRange(addKeys);
                }
            }
            finally
            {
                if (_configLock.IsWriteLockHeld)
                {
                    _configLock.ExitWriteLock();
                }
            }
        }

        #endregion

        #region Map domains

        /// <inheritdoc />
        public virtual DomainAndUri? MapDomain(IReadOnlyCollection<DomainAndUri> domainAndUris, Uri current, string? culture, string? defaultCulture)
        {
            var currentAuthority = current.GetLeftPart(UriPartial.Authority);
            Dictionary<string, string[]>? qualifiedSites = GetQualifiedSites(current);

            return MapDomain(domainAndUris, qualifiedSites, currentAuthority, culture, defaultCulture);
        }

        /// <inheritdoc />
        public virtual IEnumerable<DomainAndUri> MapDomains(IReadOnlyCollection<DomainAndUri> domainAndUris, Uri current, bool excludeDefault, string? culture, string? defaultCulture)
        {
            // TODO: ignoring cultures entirely?

            var currentAuthority = current.GetLeftPart(UriPartial.Authority);
            KeyValuePair<string, string[]>[]? candidateSites = null;
            IEnumerable<DomainAndUri> ret = domainAndUris;

            try
            {
                _configLock.EnterReadLock();

                Dictionary<string, string[]>? qualifiedSites = GetQualifiedSitesInsideLock(current);

                if (excludeDefault)
                {
                    // exclude the current one (avoid producing the absolute equivalent of what GetUrl returns)
                    Uri hintWithSlash = current.EndPathWithSlash();
                    DomainAndUri? hinted =
                        domainAndUris.FirstOrDefault(d => d.Uri.EndPathWithSlash().IsBaseOf(hintWithSlash));
                    if (hinted != null)
                    {
                        ret = ret.Where(d => d != hinted);
                    }

                    // exclude the default one (avoid producing a possible duplicate of what GetUrl returns)
                    // only if the default one cannot be the current one ie if hinted is not null
                    if (hinted == null && domainAndUris.Any())
                    {
                        // it is illegal to call MapDomain if domainAndUris is empty
                        // also, domainAndUris should NOT contain current, hence the test on hinted
                        DomainAndUri? mainDomain = MapDomain(domainAndUris, qualifiedSites, currentAuthority, culture, defaultCulture); // what GetUrl would get
                        ret = ret.Where(d => d != mainDomain);
                    }
                }

                // we do our best, but can't do the impossible
                if (qualifiedSites == null)
                {
                    return ret;
                }

                // find a site that contains the current authority
                KeyValuePair<string, string[]> currentSite =
                    qualifiedSites.FirstOrDefault(site => site.Value.Contains(currentAuthority));

                // if current belongs to a site, pick every element from domainAndUris that also belong
                // to that site -- or to any site bound to that site

                if (!currentSite.Equals(default(KeyValuePair<string, string[]>)))
                {
                    candidateSites = new[] { currentSite };
                    if (Bindings != null && Bindings.ContainsKey(currentSite.Key))
                    {
                        IEnumerable<KeyValuePair<string, string[]>> boundSites =
                            qualifiedSites.Where(site => Bindings[currentSite.Key].Contains(site.Key));
                        candidateSites = candidateSites.Union(boundSites).ToArray();

                        // .ToArray ensures it is evaluated before the configuration lock is exited
                    }
                }
            }
            finally
            {
                if (_configLock.IsReadLockHeld)
                {
                    _configLock.ExitReadLock();
                }
            }

            // if we are able to filter, then filter, else return the whole lot
            return candidateSites == null
                ? ret
                : ret.Where(d =>
                {
                    var authority = d.Uri.GetLeftPart(UriPartial.Authority);
                    return candidateSites.Any(site => site.Value.Contains(authority));
                });
        }

        private Dictionary<string, string[]>? GetQualifiedSites(Uri current)
        {
            try
            {
                _configLock.EnterReadLock();

                return GetQualifiedSitesInsideLock(current);
            }
            finally
            {
                if (_configLock.IsReadLockHeld)
                {
                    _configLock.ExitReadLock();
                }
            }
        }

        private Dictionary<string, string[]>? GetQualifiedSitesInsideLock(Uri current)
        {
            // we do our best, but can't do the impossible
            if (Sites == null)
            {
                return null;
            }

            // cached?
            if (_qualifiedSites != null && _qualifiedSites.ContainsKey(current.Scheme))
            {
                return _qualifiedSites[current.Scheme];
            }

            _qualifiedSites = _qualifiedSites ?? new Dictionary<string, Dictionary<string, string[]>>();

            // convert sites into authority sites based upon current scheme
            // because some domains in the sites might not have a scheme -- and cache
            return _qualifiedSites[current.Scheme] = Sites
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(d =>
                            new Uri(UriUtilityCore.StartWithScheme(d, current.Scheme))
                                .GetLeftPart(UriPartial.Authority))
                        .ToArray());

            // .ToDictionary will evaluate and create the dictionary immediately
            // the new value is .ToArray so it will also be evaluated immediately
            // therefore it is safe to return and exit the configuration lock
        }

        private DomainAndUri? MapDomain(
            IReadOnlyCollection<DomainAndUri> domainAndUris,
            Dictionary<string, string[]>? qualifiedSites,
            string currentAuthority,
            string? culture,
            string? defaultCulture)
        {
            if (domainAndUris == null)
            {
                throw new ArgumentNullException(nameof(domainAndUris));
            }

            if (domainAndUris.Count == 0)
            {
                throw new ArgumentException("Cannot be empty.", nameof(domainAndUris));
            }

            if (qualifiedSites == null)
            {
                return domainAndUris.FirstOrDefault(x => x.Culture.InvariantEquals(culture))
                       ?? domainAndUris.FirstOrDefault(x => x.Culture.InvariantEquals(defaultCulture))
                       ?? (culture is null ? domainAndUris.First() : null);
            }

            // find a site that contains the current authority
            KeyValuePair<string, string[]> currentSite =
                qualifiedSites.FirstOrDefault(site => site.Value.Contains(currentAuthority));

            // if current belongs to a site - try to pick the first element
            // from domainAndUris that also belongs to that site
            DomainAndUri? ret = currentSite.Equals(default(KeyValuePair<string, string[]>))
                ? null
                : domainAndUris.FirstOrDefault(d =>
                    currentSite.Value.Contains(d.Uri.GetLeftPart(UriPartial.Authority)));

            // no match means that either current does not belong to a site, or the site it belongs to
            // does not contain any of domainAndUris. Yet we have to return something. here, it becomes
            // a bit arbitrary.

            // look through sites in order and pick the first domainAndUri that belongs to a site
            ret = ret ?? qualifiedSites
                .Where(site => site.Key != currentSite.Key)
                .Select(site => domainAndUris.FirstOrDefault(domainAndUri =>
                    site.Value.Contains(domainAndUri.Uri.GetLeftPart(UriPartial.Authority))))
                .FirstOrDefault(domainAndUri => domainAndUri != null);

            // random, really
            ret = ret ?? domainAndUris.FirstOrDefault(x => x.Culture.InvariantEquals(culture)) ?? domainAndUris.First();

            return ret;
        }

        #endregion
    }
}
