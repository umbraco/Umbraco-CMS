namespace Umbraco.Cms.Core.Search.Configuration;

/// <summary>
/// Registers a search index and the searcher/indexer implementations that serve it.
/// </summary>
/// <param name="IndexAlias">The alias of the index.</param>
/// <param name="Indexer">The <see cref="Umbraco.Cms.Core.Search.IIndexer"/> implementation type that writes to the index.</param>
/// <param name="Searcher">The <see cref="Umbraco.Cms.Core.Search.ISearcher"/> implementation type that queries the index.</param>
public record IndexRegistration(string IndexAlias, Type Indexer, Type Searcher);
