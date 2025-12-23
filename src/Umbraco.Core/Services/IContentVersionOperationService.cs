// src/Umbraco.Core/Services/IContentVersionOperationService.cs
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for content version operations (retrieving versions, rollback, deleting versions).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Implementation Note:</strong> Do not implement this interface directly.
/// Instead, inherit from <see cref="ContentServiceBase"/> which provides required
/// infrastructure (scoping, repository access, auditing). Direct implementation
/// without this base class will result in missing functionality.
/// </para>
/// <para>
/// This interface is part of the ContentService refactoring initiative (Phase 3).
/// It extracts version operations into a focused, testable service.
/// </para>
/// <para>
/// <strong>Note:</strong> This interface provides synchronous version operations
/// extracted from <see cref="IContentService"/>. For async API-layer version operations,
/// see <see cref="IContentVersionService"/> which orchestrates via this service.
/// </para>
/// <para>
/// <strong>Versioning Policy:</strong> This interface follows additive-only changes.
/// New methods may be added with default implementations. Existing methods will not
/// be removed or have signatures changed without a 2 major version deprecation period.
/// </para>
/// <para>
/// <strong>Version History:</strong>
/// <list type="bullet">
///   <item><description>v1.0 (Phase 3): Initial interface with GetVersion, GetVersions, Rollback, DeleteVersions operations</description></item>
/// </list>
/// </para>
/// </remarks>
/// <since>1.0</since>
public interface IContentVersionOperationService : IService
{
    #region Version Retrieval

    /// <summary>
    /// Gets a specific version of content by version id.
    /// </summary>
    /// <param name="versionId">The version id to retrieve.</param>
    /// <returns>The content version, or null if not found.</returns>
    IContent? GetVersion(int versionId);

    /// <summary>
    /// Gets all versions of a content item.
    /// </summary>
    /// <param name="id">The content id.</param>
    /// <returns>All versions of the content, ordered by version date descending.</returns>
    IEnumerable<IContent> GetVersions(int id);

    /// <summary>
    /// Gets a paged subset of versions for a content item.
    /// </summary>
    /// <param name="id">The content id.</param>
    /// <param name="skip">Number of versions to skip.</param>
    /// <param name="take">Number of versions to take.</param>
    /// <returns>Paged versions of the content, ordered by version date descending.</returns>
    IEnumerable<IContent> GetVersionsSlim(int id, int skip, int take);

    /// <summary>
    /// Gets version ids for a content item, ordered with latest first.
    /// </summary>
    /// <param name="id">The content id.</param>
    /// <param name="maxRows">Maximum number of version ids to return. Must be positive.</param>
    /// <returns>Version ids ordered with latest first. Empty if content not found.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if maxRows is less than or equal to zero.</exception>
    /// <remarks>
    /// This method acquires a read lock on the content tree for consistency with other
    /// version retrieval methods. If content with the specified id does not exist,
    /// an empty enumerable is returned rather than throwing an exception.
    /// </remarks>
    IEnumerable<int> GetVersionIds(int id, int maxRows);

    #endregion

    #region Rollback

    /// <summary>
    /// Rolls back content to a previous version.
    /// </summary>
    /// <param name="id">The content id to rollback.</param>
    /// <param name="versionId">The version id to rollback to.</param>
    /// <param name="culture">The culture to rollback, or "*" for all cultures.</param>
    /// <param name="userId">The user performing the rollback.</param>
    /// <returns>The operation result indicating success or failure.</returns>
    /// <remarks>
    /// Fires <see cref="Notifications.ContentRollingBackNotification"/> (cancellable) before rollback
    /// and <see cref="Notifications.ContentRolledBackNotification"/> after successful rollback.
    /// The rollback copies property values from the target version to the current content
    /// and saves it, creating a new version.
    /// </remarks>
    OperationResult Rollback(int id, int versionId, string culture = "*", int userId = Constants.Security.SuperUserId);

    #endregion

    #region Version Deletion

    /// <summary>
    /// Permanently deletes versions of content prior to a specific date.
    /// </summary>
    /// <param name="id">The content id.</param>
    /// <param name="versionDate">Delete versions older than this date.</param>
    /// <param name="userId">The user performing the deletion.</param>
    /// <remarks>
    /// This method will never delete the latest version of a content item.
    /// Fires <see cref="Notifications.ContentDeletingVersionsNotification"/> (cancellable) before deletion
    /// and <see cref="Notifications.ContentDeletedVersionsNotification"/> after deletion.
    /// </remarks>
    void DeleteVersions(int id, DateTime versionDate, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Permanently deletes a specific version of content.
    /// </summary>
    /// <param name="id">The content id.</param>
    /// <param name="versionId">The version id to delete.</param>
    /// <param name="deletePriorVersions">If true, also deletes all versions prior to the specified version.</param>
    /// <param name="userId">The user performing the deletion.</param>
    /// <remarks>
    /// This method will never delete the current version or published version of a content item.
    /// Fires <see cref="Notifications.ContentDeletingVersionsNotification"/> (cancellable) before deletion
    /// and <see cref="Notifications.ContentDeletedVersionsNotification"/> after deletion.
    /// If deletePriorVersions is true, it first deletes all versions prior to the specified version's date,
    /// then deletes the specified version.
    /// </remarks>
    void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId = Constants.Security.SuperUserId);

    #endregion
}
