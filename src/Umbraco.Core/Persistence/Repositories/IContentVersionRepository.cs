using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IContentVersionRepository : IRepository
{
    /// <summary>
    ///     Gets content versions eligible for cleanup, filtered to those older than the
    ///     specified date and limited to a maximum number of results, ordered oldest first.
    /// </summary>
    /// <param name="olderThan">Only versions with a date earlier than this are returned.</param>
    /// <param name="maxCount">The maximum number of versions to return, or <c>null</c> for no limit.</param>
    IReadOnlyCollection<ContentVersionMeta> GetContentVersionsEligibleForCleanup(DateTime olderThan, int? maxCount);

    /// <summary>
    ///     Gets cleanup policy override settings per content type.
    /// </summary>
    IReadOnlyCollection<ContentVersionCleanupPolicySettings> GetCleanupPolicies();

    /// <summary>
    ///     Gets paginated content versions for given content id paginated.
    /// </summary>
    IEnumerable<ContentVersionMeta> GetPagedItemsByContentId(int contentId, long pageIndex, int pageSize, out long totalRecords, int? languageId = null);

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
