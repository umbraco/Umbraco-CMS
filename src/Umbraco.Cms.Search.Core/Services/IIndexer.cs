using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services;

/// <summary>
/// Writes to and manages a search index. Implemented by search providers (e.g. Examine) for each index alias.
/// </summary>
public interface IIndexer
{
    /// <summary>
    /// Adds or updates the document(s) for an item in the index.
    /// </summary>
    /// <param name="indexAlias">The alias of the index to write to.</param>
    /// <param name="id">The key of the item being indexed.</param>
    /// <param name="objectType">The entity type of the item being indexed.</param>
    /// <param name="variations">The culture/segment variations to index.</param>
    /// <param name="fields">The fields to write.</param>
    /// <param name="protection">The public access restrictions to attach to the document, if any.</param>
    Task AddOrUpdateAsync(string indexAlias, Guid id, UmbracoObjectTypes objectType, IEnumerable<Variation> variations, IEnumerable<IndexField> fields, ContentProtection? protection);

    /// <summary>
    /// Removes the document(s) for the given items from the index.
    /// </summary>
    /// <param name="indexAlias">The alias of the index to delete from.</param>
    /// <param name="ids">The keys of the items to remove.</param>
    Task DeleteAsync(string indexAlias, IEnumerable<Guid> ids);

    /// <summary>
    /// Clears the index of all documents.
    /// </summary>
    /// <param name="indexAlias">The alias of the index to reset.</param>
    Task ResetAsync(string indexAlias);

    /// <summary>
    /// Gets metadata (e.g. health, document count) for the index.
    /// </summary>
    /// <param name="indexAlias">The alias of the index to inspect.</param>
    /// <returns>The index's metadata.</returns>
    Task<IndexMetadata> GetMetadataAsync(string indexAlias);
}
