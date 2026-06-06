using System.Collections.Concurrent;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

/// <summary>
/// Implements <see cref="IDomainCacheService" />, providing an in-memory cache of the configured <see cref="Domain" />s.
/// </summary>
/// <remarks>
/// The cache is lazily populated from the database on first access and kept up to date in response to domain
/// cache refresher notifications. It is registered as a singleton, so a single instance serves all requests.
/// </remarks>
public class DomainCacheService : IDomainCacheService
{
    private readonly IDomainService _domainService;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly object _initializationLock = new();

    // Both fields are written under _initializationLock but read on the hot path (request routing) without
    // it. Marking them volatile makes those lock-free reads acquire-reads, so a reader is guaranteed to see
    // the fully populated dictionary and the completed-initialization flag together, never a stale or
    // half-published value. This is required for correctness on weak memory models such as ARM; on x86/x64
    // ordinary reads already have acquire semantics, but we cannot rely on that.
    private volatile ConcurrentDictionary<int, Domain> _domains = new();
    private volatile bool _initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainCacheService" /> class.
    /// </summary>
    /// <param name="domainService">The service used to load domains from the database.</param>
    /// <param name="coreScopeProvider">The provider used to create scopes for database access.</param>
    public DomainCacheService(IDomainService domainService, ICoreScopeProvider coreScopeProvider)
    {
        _domainService = domainService;
        _coreScopeProvider = coreScopeProvider;
    }

    /// <inheritdoc />
    public IEnumerable<Domain> GetAll(bool includeWildcards)
    {
        InitializeIfMissing();
        return includeWildcards == false
            ? _domains.Select(x => x.Value).Where(x => x.IsWildcard == false).OrderBy(x => x.SortOrder)
            : _domains.Select(x => x.Value).OrderBy(x => x.SortOrder);
    }

    /// <summary>
    /// Loads the domains on first access, ensuring the cache is populated before any caller reads from it.
    /// </summary>
    private void InitializeIfMissing()
    {
        // Lazy, on-demand initialization triggered by the first request to reach the cache.
        // The flag must only be set to true *after* the domains have been loaded and published.
        // Setting it beforehand creates a window where a concurrent caller observes _initialized == true,
        // skips loading, and reads an empty domain cache. On a multi-site setup that empties domain
        // resolution, causing every site to fall back to the first root node (see ContentFinderByUrlNew).
        // The double-checked lock ensures a single load while concurrent readers block until it completes.
        if (_initialized)
        {
            return;
        }

        lock (_initializationLock)
        {
            if (_initialized)
            {
                return;
            }

            LoadDomains();
            _initialized = true;
        }
    }

    /// <inheritdoc />
    public IEnumerable<Domain> GetAssigned(int documentId, bool includeWildcards = false)
    {
        InitializeIfMissing();
        IEnumerable<Domain> list = _domains.Values.Where(x => x.ContentId == documentId);
        if (includeWildcards == false)
        {
            list = list.Where(x => x.IsWildcard == false);
        }

        return list.OrderBy(x => x.SortOrder);
    }

    /// <inheritdoc />
    public bool HasAssigned(int documentId, bool includeWildcards = false)
    {
        InitializeIfMissing();
        return documentId > 0 && GetAssigned(documentId, includeWildcards).Any();
    }

    /// <inheritdoc />
    public void Refresh(DomainCacheRefresher.JsonPayload[] payloads)
    {
        foreach (DomainCacheRefresher.JsonPayload payload in payloads)
        {
            switch (payload.ChangeType)
            {
                case DomainChangeTypes.RefreshAll:
                    using (ICoreScope scope = _coreScopeProvider.CreateCoreScope())
                    {
                        scope.ReadLock(Constants.Locks.Domains);
                        LoadDomains();
                        scope.Complete();
                    }

                    break;
                case DomainChangeTypes.Remove:
                    _domains.Remove(payload.Id, out _);
                    break;
                case DomainChangeTypes.Refresh:
                    IDomain? domain = _domainService.GetById(payload.Id);
                    if (domain == null)
                    {
                        continue;
                    }

                    if (domain.RootContentId.HasValue == false)
                    {
                        continue; // anomaly
                    }

                    var culture = domain.LanguageIsoCode;
                    if (string.IsNullOrWhiteSpace(culture))
                    {
                        continue; // anomaly
                    }

                    _domains[domain.Id] = new Domain(domain.Id, domain.DomainName, domain.RootContentId.Value, culture, domain.IsWildcard, domain.SortOrder);
                    break;
            }
        }
    }

    /// <summary>
    /// Reads the configured domains from the database into a fresh dictionary and atomically swaps it in
    /// as the current cache.
    /// </summary>
    private void LoadDomains()
    {
        // Build the replacement set in a local dictionary and publish it with a single write to the
        // (volatile) _domains field. A reader never observes a partially populated cache during a RefreshAll
        // rebuild, and the published set contains exactly the current domains (any removed since the last
        // load are absent).
        var newDomains = new ConcurrentDictionary<int, Domain>();
        using (ICoreScope scope = _coreScopeProvider.CreateCoreScope())
        {
            scope.ReadLock(Constants.Locks.Domains);
            IEnumerable<IDomain> domains = _domainService.GetAll(true);
            foreach (Domain domain in domains
                         .Where(x => x.RootContentId.HasValue && x.LanguageIsoCode.IsNullOrWhiteSpace() == false)
                         .Select(x => new Domain(x.Id, x.DomainName, x.RootContentId!.Value, x.LanguageIsoCode!, x.IsWildcard, x.SortOrder)))
            {
                newDomains[domain.Id] = domain;
            }
            scope.Complete();
        }

        _domains = newDomains;
    }
}
