using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
/// Repository for document URL aliases.
/// </summary>
public interface IDocumentUrlAliasRepository
{
    /// <summary>
    /// Saves the specified aliases to the database.
    /// Handles insert/update/delete via diff - existing aliases not in the new set are deleted.
    /// </summary>
    /// <param name="aliases">The aliases to save.</param>
    void Save(IEnumerable<PublishedDocumentUrlAlias> aliases);

    /// <summary>
    /// Gets all persisted aliases from the database.
    /// </summary>
    /// <returns>All persisted aliases.</returns>
    IEnumerable<PublishedDocumentUrlAlias> GetAll();

    /// <summary>
    /// Deletes all aliases for the specified document keys.
    /// </summary>
    /// <param name="documentKeys">The document keys to delete aliases for.</param>
    void DeleteByDocumentKey(IEnumerable<Guid> documentKeys);

    /// <summary>
    /// Deletes every persisted alias, i.e. for a full rebuild that repopulates the table from scratch.
    /// </summary>
    // TODO (V19): Remove the default implementation.
    void DeleteAll()
        => DeleteByDocumentKey(GetAll().Select(x => x.DocumentKey).Distinct());

    /// <summary>
    /// Gets all document aliases.
    /// </summary>
    /// <returns>Raw alias data from documents with umbracoUrlAlias property.</returns>
    IEnumerable<DocumentUrlAliasRaw> GetAllDocumentUrlAliases();
}

/// <summary>
/// Raw alias data from a direct SQL query.
/// </summary>
public class DocumentUrlAliasRaw
{
    /// <summary>
    /// Gets or sets the document key.
    /// </summary>
    public Guid DocumentKey { get; set; }

    /// <summary>
    /// Gets or sets the language ID (null for invariant).
    /// </summary>
    public int? LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the raw alias value (may be comma-separated).
    /// </summary>
    public string AliasValue { get; set; } = string.Empty;
}
