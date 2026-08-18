using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Configuration;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Configuration;

/// <summary>
/// Holds the registered indexes and their indexer/searcher/change-strategy implementations, keyed by index alias.
/// </summary>
public sealed class IndexOptions
{
    private readonly Dictionary<string, IndexRegistration> _register = [];

    /// <summary>
    /// Registers a non-content index and its indexer/searcher implementations.
    /// </summary>
    /// <typeparam name="TIndexer">The indexer implementation type.</typeparam>
    /// <typeparam name="TSearcher">The searcher implementation type.</typeparam>
    /// <param name="indexAlias">The alias of the index.</param>
    public void RegisterIndex<TIndexer, TSearcher>(string indexAlias)
        where TIndexer : class, IIndexer
        where TSearcher : class, ISearcher
    {
        ArgumentException.ThrowIfNullOrEmpty("Index alias cannot be empty", nameof(indexAlias));

        _register[indexAlias] = new IndexRegistration(indexAlias, typeof(TIndexer), typeof(TSearcher));
    }

    /// <summary>
    /// Registers a content-backed index, its indexer/searcher/change-strategy implementations, and the object types it contains.
    /// </summary>
    /// <typeparam name="TIndexer">The indexer implementation type.</typeparam>
    /// <typeparam name="TSearcher">The searcher implementation type.</typeparam>
    /// <typeparam name="TContentChangeStrategy">The content change strategy implementation type.</typeparam>
    /// <param name="indexAlias">The alias of the index.</param>
    /// <param name="containedObjectTypes">The object types (e.g. documents, media) contained in this index.</param>
    public void RegisterContentIndex<TIndexer, TSearcher, TContentChangeStrategy>(string indexAlias, params UmbracoObjectTypes[] containedObjectTypes)
        where TIndexer : class, IIndexer
        where TSearcher : class, ISearcher
        where TContentChangeStrategy : class, IContentChangeStrategy
        => RegisterContentIndex<TIndexer, TSearcher, TContentChangeStrategy>(indexAlias, false, containedObjectTypes);

    /// <summary>
    /// Registers a content-backed index, its indexer/searcher/change-strategy implementations, and the object types it contains.
    /// </summary>
    /// <typeparam name="TIndexer">The indexer implementation type.</typeparam>
    /// <typeparam name="TSearcher">The searcher implementation type.</typeparam>
    /// <typeparam name="TContentChangeStrategy">The content change strategy implementation type.</typeparam>
    /// <param name="indexAlias">The alias of the index.</param>
    /// <param name="sameOriginOnly">Whether this index only accepts changes originating from the same server.</param>
    /// <param name="containedObjectTypes">The object types (e.g. documents, media) contained in this index.</param>
    public void RegisterContentIndex<TIndexer, TSearcher, TContentChangeStrategy>(string indexAlias, bool sameOriginOnly, params UmbracoObjectTypes[] containedObjectTypes)
        where TIndexer : class, IIndexer
        where TSearcher : class, ISearcher
        where TContentChangeStrategy : class, IContentChangeStrategy
    {
        ArgumentException.ThrowIfNullOrEmpty("Index alias cannot be empty", nameof(indexAlias));
        if (containedObjectTypes.Length is 0)
        {
            throw new ArgumentException($"Index \"{indexAlias}\" must define at least one contained object type",  nameof(containedObjectTypes));
        }

        _register[indexAlias] = new ContentIndexRegistration(indexAlias, typeof(TIndexer), typeof(TSearcher), typeof(TContentChangeStrategy), containedObjectTypes.Distinct(), sameOriginOnly);
    }

    /// <summary>
    /// Gets all registered content-backed indexes.
    /// </summary>
    /// <returns>The registered content index registrations.</returns>
    public ContentIndexRegistration[] GetContentIndexRegistrations()
        => _register.Values.OfType<ContentIndexRegistration>().ToArray();

    /// <summary>
    /// Gets the registration for an index, if one exists.
    /// </summary>
    /// <param name="indexAlias">The alias of the index.</param>
    /// <returns>The index registration, or null if no index is registered with that alias.</returns>
    public IndexRegistration? GetIndexRegistration(string indexAlias)
        => _register.TryGetValue(indexAlias, out IndexRegistration? indexRegistration) ? indexRegistration : null;

    /// <summary>
    /// Gets the content-backed registration for an index, if one exists.
    /// </summary>
    /// <param name="indexAlias">The alias of the index.</param>
    /// <returns>The content index registration, or null if no content index is registered with that alias.</returns>
    public ContentIndexRegistration? GetContentIndexRegistration(string indexAlias)
        => GetIndexRegistration(indexAlias) as ContentIndexRegistration;
}
