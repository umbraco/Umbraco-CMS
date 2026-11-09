namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for querying document publish status.
/// </summary>
public interface IPublishStatusRepository
{
    /// <summary>
    ///     Gets the publish status for all documents.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A dictionary mapping document keys to their published culture codes.</returns>
    Task<IDictionary<Guid, ISet<string>>> GetAllPublishStatusAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the publish status for a specific document.
    /// </summary>
    /// <param name="documentKey">The unique key of the document.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A set of culture codes for which the document is published.</returns>
    Task<ISet<string>> GetPublishStatusAsync(Guid documentKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the publish status for a document and all its descendants.
    /// </summary>
    /// <param name="rootDocumentKey">The unique key of the root document.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A dictionary mapping document keys to their published culture codes.</returns>
    Task<IDictionary<Guid, ISet<string>>> GetDescendantsOrSelfPublishStatusAsync(Guid rootDocumentKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the keys of all documents that are published in a culture.
    /// </summary>
    /// <param name="culture">The culture code.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>The keys of the documents published in the culture, including invariant published documents.</returns>
    // TODO (V20): Remove the default implementation.
    async Task<IEnumerable<Guid>> GetPublishedKeysAsync(string culture, CancellationToken cancellationToken)
        => (await GetAllPublishStatusAsync(cancellationToken))
            .Where(x => x.Value.Contains(culture, StringComparer.InvariantCultureIgnoreCase))
            .Select(x => x.Key)
            .ToArray();

    /// <summary>
    ///     Gets the publish status for all elements.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A dictionary mapping element keys to their published culture codes.</returns>
    Task<IDictionary<Guid, ISet<string>>> GetAllElementPublishStatusAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the publish status for a specific element.
    /// </summary>
    /// <param name="elementKey">The unique key of the element.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A set of culture codes for which the element is published.</returns>
    Task<ISet<string>> GetElementPublishStatusAsync(Guid elementKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the keys of all elements that are published in a culture.
    /// </summary>
    /// <param name="culture">The culture code.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>The keys of the elements published in the culture, including invariant published elements.</returns>
    // TODO (V20): Remove the default implementation.
    async Task<IEnumerable<Guid>> GetPublishedElementKeysAsync(string culture, CancellationToken cancellationToken)
        => (await GetAllElementPublishStatusAsync(cancellationToken))
            .Where(x => x.Value.Contains(culture, StringComparer.InvariantCultureIgnoreCase))
            .Select(x => x.Key)
            .ToArray();
}
