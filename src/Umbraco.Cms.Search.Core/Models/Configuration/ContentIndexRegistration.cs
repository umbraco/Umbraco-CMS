using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Search.Core.Models.Configuration;

/// <summary>
/// Registers a content-backed search index, the entity types it contains, and how content changes for it are tracked.
/// </summary>
/// <param name="IndexAlias">The alias of the index.</param>
/// <param name="Indexer">The <see cref="Umbraco.Cms.Search.Core.Services.IIndexer"/> implementation type that writes to the index.</param>
/// <param name="Searcher">The <see cref="Umbraco.Cms.Search.Core.Services.ISearcher"/> implementation type that queries the index.</param>
/// <param name="ContentChangeStrategy">The <see cref="Umbraco.Cms.Search.Core.Services.ContentIndexing.IContentChangeStrategy"/> implementation type used to determine what needs re-indexing.</param>
/// <param name="ContainedObjectTypes">The entity types (document, media, member) contained in this index.</param>
/// <param name="SameOriginOnly">Whether indexing should only run on the server that originated the change.</param>
public record ContentIndexRegistration(
    string IndexAlias,
    Type Indexer,
    Type Searcher,
    Type ContentChangeStrategy,
    IEnumerable<UmbracoObjectTypes> ContainedObjectTypes,
    bool SameOriginOnly)
    : IndexRegistration(IndexAlias, Indexer, Searcher)
{
}
