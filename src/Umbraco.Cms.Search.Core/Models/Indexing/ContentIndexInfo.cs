using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Services;

namespace Umbraco.Cms.Search.Core.Models.Indexing;

/// <summary>
/// Describes a registered content-backed search index: its alias, the entity types it contains, and its indexer.
/// </summary>
/// <param name="IndexAlias">The alias of the index.</param>
/// <param name="ContainedObjectTypes">The entity types (document, media, member) contained in this index.</param>
/// <param name="Indexer">The indexer that writes to this index.</param>
public record ContentIndexInfo(string IndexAlias, IEnumerable<UmbracoObjectTypes> ContainedObjectTypes, IIndexer Indexer);
