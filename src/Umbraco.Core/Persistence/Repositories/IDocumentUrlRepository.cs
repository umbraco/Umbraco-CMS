using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for document URL segment entities.
/// </summary>
public interface IDocumentUrlRepository
{
    /// <summary>
    ///     Saves a collection of published document URL segments.
    /// </summary>
    /// <param name="publishedDocumentUrlSegments">The URL segments to save.</param>
    void Save(IEnumerable<PublishedDocumentUrlSegment> publishedDocumentUrlSegments);

    /// <summary>
    ///     Gets all published document URL segments.
    /// </summary>
    /// <returns>A collection of all published document URL segments.</returns>
    IEnumerable<PublishedDocumentUrlSegment> GetAll();

    /// <summary>
    ///     Deletes document URL segments by document keys.
    /// </summary>
    /// <param name="select">The document keys for which to delete URL segments.</param>
    void DeleteByDocumentKey(IEnumerable<Guid> select);
}
