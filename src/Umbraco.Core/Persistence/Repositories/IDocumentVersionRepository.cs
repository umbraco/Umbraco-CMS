using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for document version operations.
/// </summary>
public interface IDocumentVersionRepository : IRepository
{
    /// <summary>
    ///     Gets a list of all historic content versions.
    /// </summary>
    [Obsolete("Use the overload accepting olderThan and maxCount. Scheduled for removal in Umbraco 19.")]
    public IReadOnlyCollection<ContentVersionMeta> GetDocumentVersionsEligibleForCleanup();

    /// <summary>
    ///     Gets document versions eligible for cleanup, filtered to those older than the
    ///     specified date and limited to a maximum number of results, ordered oldest first.
    /// </summary>
    /// <param name="olderThan">Only versions with a date earlier than this are returned.</param>
    /// <param name="maxCount">The maximum number of versions to return, or <c>null</c> for no limit.</param>
    public IReadOnlyCollection<ContentVersionMeta> GetDocumentVersionsEligibleForCleanup(DateTime olderThan, int? maxCount)
#pragma warning disable CS0618 // Type or member is obsolete
        => GetDocumentVersionsEligibleForCleanup();
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    ///     Gets cleanup policy override settings per content type.
    /// </summary>
    public IReadOnlyCollection<ContentVersionCleanupPolicySettings> GetCleanupPolicies();

    /// <summary>
    ///     Gets paginated content versions for given content id paginated.
    /// </summary>
    public IEnumerable<ContentVersionMeta> GetPagedItemsByContentId(int contentId, long pageIndex, int pageSize, out long totalRecords, int? languageId = null);

    /// <summary>
    ///     Deletes multiple content versions by ID.
    /// </summary>
    void DeleteVersions(IEnumerable<int> versionIds);

    /// <summary>
    ///     Updates the prevent cleanup flag on a content version.
    /// </summary>
    void SetPreventCleanup(int versionId, bool preventCleanup);

    /// <summary>
    ///     Gets the content version metadata for a specific version.
    /// </summary>
    ContentVersionMeta? Get(int versionId);
}
