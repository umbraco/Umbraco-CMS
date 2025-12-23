// src/Umbraco.Core/Services/ContentVersionOperationService.cs
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Implements content version operations (retrieving versions, rollback, deleting versions).
/// </summary>
public class ContentVersionOperationService : ContentServiceBase, IContentVersionOperationService
{
    private readonly ILogger<ContentVersionOperationService> _logger;
    // v1.2 Fix (Issue 3.3): Added IContentCrudService for proper save with notifications
    private readonly IContentCrudService _crudService;

    public ContentVersionOperationService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDocumentRepository documentRepository,
        IAuditService auditService,
        IUserIdKeyResolver userIdKeyResolver,
        IContentCrudService crudService)  // v1.2: Added for Rollback save operation
        : base(provider, loggerFactory, eventMessagesFactory, documentRepository, auditService, userIdKeyResolver)
    {
        _logger = loggerFactory.CreateLogger<ContentVersionOperationService>();
        _crudService = crudService;
    }

    #region Version Retrieval

    /// <inheritdoc />
    public IContent? GetVersion(int versionId)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return DocumentRepository.GetVersion(versionId);
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetVersions(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return DocumentRepository.GetAllVersions(id);
    }

    /// <inheritdoc />
    public IEnumerable<IContent> GetVersionsSlim(int id, int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        return DocumentRepository.GetAllVersionsSlim(id, skip, take);
    }

    /// <inheritdoc />
    public IEnumerable<int> GetVersionIds(int id, int maxRows)
    {
        // v1.3 Fix (Issue 3.1): Added input validation to match interface documentation.
        // The interface documents ArgumentOutOfRangeException for maxRows <= 0.
        if (maxRows <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRows), maxRows, "Value must be greater than zero.");
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        // v1.1 Fix (Issue 2.3): Added ReadLock for consistency with other read operations.
        // The original ContentService.GetVersionIds did not acquire a ReadLock, which was
        // inconsistent with GetVersion, GetVersions, and GetVersionsSlim.
        scope.ReadLock(Constants.Locks.ContentTree);
        return DocumentRepository.GetVersionIds(id, maxRows);
    }

    #endregion

    #region Rollback

    /// <inheritdoc />
    public OperationResult Rollback(int id, int versionId, string culture = "*", int userId = Constants.Security.SuperUserId)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        // v1.1 Fix (Issue 2.1): Use a single scope for the entire operation to eliminate
        // TOCTOU race condition. Previously used separate read and write scopes which
        // could allow concurrent modification between reading content and writing changes.
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // Read operations - acquire read lock first
        scope.ReadLock(Constants.Locks.ContentTree);
        IContent? content = DocumentRepository.Get(id);
        // v1.1 Fix: Use DocumentRepository.GetVersion directly instead of calling
        // this.GetVersion() which would create a nested scope
        IContent? version = DocumentRepository.GetVersion(versionId);

        // Null checks - cannot rollback if content or version is missing, or if trashed
        if (content == null || version == null || content.Trashed)
        {
            scope.Complete();
            return new OperationResult(OperationResultType.FailedCannot, evtMsgs);
        }

        var rollingBackNotification = new ContentRollingBackNotification(content, evtMsgs);
        if (scope.Notifications.PublishCancelable(rollingBackNotification))
        {
            scope.Complete();
            return OperationResult.Cancel(evtMsgs);
        }

        // Copy the changes from the version
        content.CopyFrom(version, culture);

        // v1.2 Fix (Issue 2.1): Use CrudService.Save to preserve ContentSaving/ContentSaved notifications.
        // The original ContentService.Rollback called Save(content, userId) which fires these notifications.
        // Using DocumentRepository.Save directly would bypass validation, audit trail, and cache invalidation.
        // v1.3 Fix (Issue 3.2): Removed explicit WriteLock - CrudService.Save handles its own locking internally.
        // v1.3 Fix (Issue 3.4): Fixed return type from OperationResult<OperationResultType> to OperationResult.
        OperationResult saveResult = _crudService.Save(content, userId);
        if (!saveResult.Success)
        {
            _logger.LogError("User '{UserId}' was unable to rollback content '{ContentId}' to version '{VersionId}'", userId, id, versionId);
            scope.Complete();
            return new OperationResult(OperationResultType.Failed, evtMsgs);
        }

        // Only publish success notification if save succeeded
        scope.Notifications.Publish(
            new ContentRolledBackNotification(content, evtMsgs).WithStateFrom(rollingBackNotification));

        // Logging & Audit
        _logger.LogInformation("User '{UserId}' rolled back content '{ContentId}' to version '{VersionId}'", userId, content.Id, version.VersionId);
        Audit(AuditType.RollBack, userId, content.Id, $"Content '{content.Name}' was rolled back to version '{version.VersionId}'");

        scope.Complete();

        return OperationResult.Succeed(evtMsgs);
    }

    #endregion

    #region Version Deletion

    /// <inheritdoc />
    public void DeleteVersions(int id, DateTime versionDate, int userId = Constants.Security.SuperUserId)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        var deletingVersionsNotification = new ContentDeletingVersionsNotification(id, evtMsgs, dateToRetain: versionDate);
        if (scope.Notifications.PublishCancelable(deletingVersionsNotification))
        {
            scope.Complete();
            return;
        }

        scope.WriteLock(Constants.Locks.ContentTree);
        DocumentRepository.DeleteVersions(id, versionDate);

        scope.Notifications.Publish(
            new ContentDeletedVersionsNotification(id, evtMsgs, dateToRetain: versionDate).WithStateFrom(deletingVersionsNotification));
        Audit(AuditType.Delete, userId, Constants.System.Root, "Delete (by version date)");

        scope.Complete();
    }

    /// <inheritdoc />
    public void DeleteVersion(int id, int versionId, bool deletePriorVersions, int userId = Constants.Security.SuperUserId)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // v1.2 Fix (Issue 3.1): Acquire WriteLock once at the start instead of multiple times.
        // This simplifies the code and avoids the read→write lock upgrade pattern.
        scope.WriteLock(Constants.Locks.ContentTree);

        var deletingVersionsNotification = new ContentDeletingVersionsNotification(id, evtMsgs, versionId);
        if (scope.Notifications.PublishCancelable(deletingVersionsNotification))
        {
            scope.Complete();
            return;
        }

        // v1.2 Fix (Issue 2.2): Preserve original double-notification behavior for deletePriorVersions.
        // The original implementation called DeleteVersions() which fired its own notifications.
        // We inline the notification firing to maintain backward compatibility.
        // v1.3 Fix (Issue 3.6): Clarification - if prior versions deletion is cancelled, we still
        // proceed with deleting the specific version. This matches original ContentService behavior.
        if (deletePriorVersions)
        {
            IContent? versionContent = DocumentRepository.GetVersion(versionId);
            DateTime cutoffDate = versionContent?.UpdateDate ?? DateTime.UtcNow;

            // Publish notifications for prior versions (matching original behavior)
            var priorVersionsNotification = new ContentDeletingVersionsNotification(id, evtMsgs, dateToRetain: cutoffDate);
            if (!scope.Notifications.PublishCancelable(priorVersionsNotification))
            {
                DocumentRepository.DeleteVersions(id, cutoffDate);
                scope.Notifications.Publish(
                    new ContentDeletedVersionsNotification(id, evtMsgs, dateToRetain: cutoffDate)
                        .WithStateFrom(priorVersionsNotification));

                // v1.3 Fix (Issue 3.3): Add audit entry for prior versions deletion.
                // The original DeleteVersions() method created its own audit entry.
                Audit(AuditType.Delete, userId, Constants.System.Root, "Delete (by version date)");
            }
        }

        IContent? c = DocumentRepository.Get(id);

        // Don't delete the current or published version
        if (c?.VersionId != versionId && c?.PublishedVersionId != versionId)
        {
            DocumentRepository.DeleteVersion(versionId);
        }

        scope.Notifications.Publish(
            new ContentDeletedVersionsNotification(id, evtMsgs, versionId).WithStateFrom(deletingVersionsNotification));
        Audit(AuditType.Delete, userId, Constants.System.Root, "Delete (by version)");

        scope.Complete();
    }

    #endregion
}
