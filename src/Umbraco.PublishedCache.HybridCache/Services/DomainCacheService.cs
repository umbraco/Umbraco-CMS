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

public class DomainCacheService : IDomainCacheService
{
    private readonly IDomainService _domainService;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly ConcurrentDictionary<int, Domain> _domains;
    private bool _initialized = false;

    public DomainCacheService(IDomainService domainService, ICoreScopeProvider coreScopeProvider)
    {
        _domainService = domainService;
        _coreScopeProvider = coreScopeProvider;
        _domains = new ConcurrentDictionary<int, Domain>();
    }

    public IEnumerable<Domain> GetAll(bool includeWildcards)
    {
        InitializeIfMissing();
        return includeWildcards == false
            ? _domains.Select(x => x.Value).Where(x => x.IsWildcard == false).OrderBy(x => x.SortOrder)
            : _domains.Select(x => x.Value).OrderBy(x => x.SortOrder);
    }

    private void InitializeIfMissing()
    {
        if (_initialized)
        {
            return;
        }
        _initialized = true;
        LoadDomains();
    }

    /// <inheritdoc />
    public IEnumerable<Domain> GetAssigned(int documentId, bool includeWildcards = false)
    {
        InitializeIfMissing();
        // probably this could be optimized with an index
        // but then we'd need a custom DomainStore of some sort
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

                    var newDomain = new Domain(domain.Id, domain.DomainName, domain.RootContentId.Value, culture, domain.IsWildcard, domain.SortOrder);

                    // Feels wierd to use key and oldvalue, but we're using neither when updating.
                    _domains.AddOrUpdate(
                        domain.Id,
                        new Domain(domain.Id, domain.DomainName, domain.RootContentId.Value, culture, domain.IsWildcard, domain.SortOrder),
                        (key, oldValue) => newDomain);
                    break;
            }
        }
    }

    private void LoadDomains()
    {
        using (ICoreScope scope = _coreScopeProvider.CreateCoreScope())
        {
            scope.ReadLock(Constants.Locks.Domains);
            IEnumerable<IDomain> domains = _domainService.GetAll(true);
            foreach (Domain domain in domains
                         .Where(x => x.RootContentId.HasValue && x.LanguageIsoCode.IsNullOrWhiteSpace() == false)
                         .Select(x => new Domain(x.Id, x.DomainName, x.RootContentId!.Value, x.LanguageIsoCode!, x.IsWildcard, x.SortOrder)))
            {
                _domains.AddOrUpdate(domain.Id, domain, (key, oldValue) => domain);
            }
            scope.Complete();
        }


    }
}
