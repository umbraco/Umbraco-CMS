// src/Umbraco.Core/Services/IContentMoveOperationService.cs
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for content move, copy, sort, and recycle bin operations.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Implementation Note:</strong> Do not implement this interface directly.
/// Instead, inherit from <see cref="ContentServiceBase"/> which provides required
/// infrastructure (scoping, repository access, auditing). Direct implementation
/// without this base class will result in missing functionality.
/// </para>
/// <para>
/// This interface is part of the ContentService refactoring initiative (Phase 4).
/// It extracts move/copy/sort operations into a focused, testable service.
/// </para>
/// <para>
/// <strong>Note:</strong> <c>MoveToRecycleBin</c> is NOT part of this interface because
/// it orchestrates multiple services (unpublish + move) and belongs in the facade.
/// </para>
/// <para>
/// <strong>Versioning Policy:</strong> This interface follows additive-only changes.
/// New methods may be added with default implementations. Existing methods will not
/// be removed or have signatures changed without a 2 major version deprecation period.
/// </para>
/// <para>
/// <strong>Version History:</strong>
/// <list type="bullet">
///   <item><description>v1.0 (Phase 4): Initial interface with Move, Copy, Sort, RecycleBin operations</description></item>
/// </list>
/// </para>
/// </remarks>
/// <since>1.0</since>
public interface IContentMoveOperationService : IService
{
    // Note: #region blocks kept for consistency with existing Umbraco interface patterns

    #region Move Operations

    /// <summary>
    /// Moves content to a new parent.
    /// </summary>
    /// <param name="content">The content to move.</param>
    /// <param name="parentId">The target parent id, or -1 for root.</param>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The operation result.</returns>
    /// <remarks>
    /// If parentId is the recycle bin (-20), this method delegates to MoveToRecycleBin
    /// behavior (should be called via ContentService facade instead).
    /// Fires <see cref="Notifications.ContentMovingNotification"/> (cancellable) before move
    /// and <see cref="Notifications.ContentMovedNotification"/> after successful move.
    /// </remarks>
    OperationResult Move(IContent content, int parentId, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Recycle Bin Operations

    /// <summary>
    /// Empties the content recycle bin.
    /// </summary>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The operation result.</returns>
    /// <remarks>
    /// Fires <see cref="Notifications.ContentEmptyingRecycleBinNotification"/> (cancellable) before emptying
    /// and <see cref="Notifications.ContentEmptiedRecycleBinNotification"/> after successful empty.
    /// Content with active relations may be skipped if DisableDeleteWhenReferenced is configured.
    /// </remarks>
    OperationResult EmptyRecycleBin(int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Empties the content recycle bin asynchronously.
    /// </summary>
    /// <param name="userId">The user key performing the operation.</param>
    /// <returns>The operation result.</returns>
    Task<OperationResult> EmptyRecycleBinAsync(Guid userId);

    /// <summary>
    /// Checks whether there is content in the recycle bin.
    /// </summary>
    /// <returns>True if the recycle bin has content; otherwise false.</returns>
    bool RecycleBinSmells();

    /// <summary>
    /// Gets paged content from the recycle bin.
    /// </summary>
    /// <param name="pageIndex">Zero-based page index.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="totalRecords">Output: total number of records in recycle bin.</param>
    /// <param name="filter">Optional filter query.</param>
    /// <param name="ordering">Optional ordering (defaults to Path).</param>
    /// <returns>Paged content from the recycle bin.</returns>
    IEnumerable<IContent> GetPagedContentInRecycleBin(
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IContent>? filter = null,
        Ordering? ordering = null);

    #endregion

    #region Copy Operations

    /// <summary>
    /// Copies content to a new parent, including all descendants.
    /// </summary>
    /// <param name="content">The content to copy.</param>
    /// <param name="parentId">The target parent id.</param>
    /// <param name="relateToOriginal">Whether to create a relation to the original.</param>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The copied content, or null if cancelled.</returns>
    /// <remarks>
    /// Fires <see cref="Notifications.ContentCopyingNotification"/> (cancellable) before each copy
    /// and <see cref="Notifications.ContentCopiedNotification"/> after each successful copy.
    /// The copy is not published regardless of the original's published state.
    /// </remarks>
    IContent? Copy(IContent content, int parentId, bool relateToOriginal, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Copies content to a new parent.
    /// </summary>
    /// <param name="content">The content to copy.</param>
    /// <param name="parentId">The target parent id.</param>
    /// <param name="relateToOriginal">Whether to create a relation to the original.</param>
    /// <param name="recursive">Whether to copy descendants recursively.</param>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The copied content, or null if cancelled.</returns>
    IContent? Copy(IContent content, int parentId, bool relateToOriginal, bool recursive, int userId = Constants.Security.SuperUserId);

    #endregion

    #region Sort Operations

    /// <summary>
    /// Sorts content items by updating their SortOrder.
    /// </summary>
    /// <param name="items">The content items in desired order.</param>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The operation result.</returns>
    /// <remarks>
    /// Fires <see cref="Notifications.ContentSortingNotification"/> (cancellable) and
    /// <see cref="Notifications.ContentSavingNotification"/> (cancellable) before sorting.
    /// Fires <see cref="Notifications.ContentSavedNotification"/>,
    /// <see cref="Notifications.ContentSortedNotification"/>, and
    /// <see cref="Notifications.ContentPublishedNotification"/> (if any were published) after.
    /// </remarks>
    OperationResult Sort(IEnumerable<IContent> items, int userId = Constants.Security.SuperUserId);

    /// <summary>
    /// Sorts content items by id in the specified order.
    /// </summary>
    /// <param name="ids">The content ids in desired order.</param>
    /// <param name="userId">The user performing the operation.</param>
    /// <returns>The operation result.</returns>
    OperationResult Sort(IEnumerable<int>? ids, int userId = Constants.Security.SuperUserId);

    #endregion
}
