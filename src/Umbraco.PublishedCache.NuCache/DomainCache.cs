using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

/// <summary>
///     Implements <see cref="IDomainCache" /> for NuCache.
/// </summary>
public class DomainCache : IDomainCache
{
    private readonly SnapDictionary<int, Domain>.Snapshot _snapshot;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DomainCache" /> class.
    /// </summary>
    public DomainCache(SnapDictionary<int, Domain>.Snapshot snapshot, string defaultCulture)
    {
        _snapshot = snapshot;
        DefaultCulture = defaultCulture;
    }

    /// <inheritdoc />
    public string DefaultCulture { get; }

    /// <inheritdoc />
    public IEnumerable<Domain> GetAll(bool includeWildcards)
    {
        IEnumerable<Domain> list = _snapshot.GetAll();
        if (includeWildcards == false)
        {
            list = list.Where(x => x.IsWildcard == false);
        }

        return list;
    }

    /// <inheritdoc />
    public IEnumerable<Domain> GetAssigned(int documentId, bool includeWildcards = false)
    {
        // probably this could be optimized with an index
        // but then we'd need a custom DomainStore of some sort
        IEnumerable<Domain> list = _snapshot.GetAll();
        list = list.Where(x => x.ContentId == documentId);
        if (includeWildcards == false)
        {
            list = list.Where(x => x.IsWildcard == false);
        }

        return list;
    }

    /// <inheritdoc />
    public bool HasAssigned(int documentId, bool includeWildcards = false)
        => documentId > 0 && GetAssigned(documentId, includeWildcards).Any();
}
