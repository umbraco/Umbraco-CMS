using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides services for managing content versions.
/// </summary>
public interface IContentVersionService
{
    /// <summary>
    ///     Removes historic content versions according to a policy.
    /// </summary>
    /// <param name="asAtDate">The date to use when applying cleanup policies.</param>
    /// <returns>A collection of version metadata for the versions that were cleaned up.</returns>
    IReadOnlyCollection<ContentVersionMeta> PerformContentVersionCleanup(DateTime asAtDate);

    /// <summary>
    ///     Gets the metadata for a specific content version.
    /// </summary>
    /// <param name="versionId">The identifier of the version.</param>
    /// <returns>The version metadata, or <c>null</c> if not found.</returns>
    ContentVersionMeta? Get(int versionId);

    /// <summary>
    ///     Gets a paged collection of content versions for a content item.
    /// </summary>
    /// <param name="contentId">The unique identifier of the content item.</param>
    /// <param name="culture">The optional culture to filter versions by.</param>
    /// <param name="skip">The number of versions to skip for pagination.</param>
    /// <param name="take">The number of versions to take for pagination.</param>
    /// <returns>An attempt containing the paged result or an error status.</returns>
    Task<Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus>> GetPagedContentVersionsAsync(Guid contentId, string? culture, int skip, int take);

    /// <summary>
    ///     Gets a specific content version by its unique identifier.
    /// </summary>
    /// <param name="versionId">The unique identifier of the version.</param>
    /// <returns>An attempt containing the content version or an error status.</returns>
    Task<Attempt<IContent?, ContentVersionOperationStatus>> GetAsync(Guid versionId);

    /// <summary>
    ///     Sets whether a content version should be prevented from automatic cleanup.
    /// </summary>
    /// <param name="versionId">The unique identifier of the version.</param>
    /// <param name="preventCleanup"><c>true</c> to prevent cleanup; <c>false</c> to allow cleanup.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the operation status.</returns>
    Task<Attempt<ContentVersionOperationStatus>> SetPreventCleanupAsync(Guid versionId, bool preventCleanup, Guid userKey);

    /// <summary>
    ///     Rolls back content to a specific version.
    /// </summary>
    /// <param name="versionId">The unique identifier of the version to roll back to.</param>
    /// <param name="culture">The optional culture to roll back. If <c>null</c>, all cultures are rolled back.</param>
    /// <param name="userKey">The unique identifier of the user performing the rollback.</param>
    /// <returns>An attempt containing the operation status.</returns>
    Task<Attempt<ContentVersionOperationStatus>> RollBackAsync(Guid versionId, string? culture, Guid userKey);
}
