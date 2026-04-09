using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Services.Pagination;
using Umbraco.Extensions;

// ReSharper disable once CheckNamespace
namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for managing content versions, including retrieval, cleanup, and rollback operations.
/// </summary>
internal sealed class ContentVersionService : IContentVersionService
{
    private readonly IAuditService _auditService;
    private readonly IContentVersionCleanupPolicy _contentVersionCleanupPolicy;
    private readonly IDocumentVersionRepository _documentVersionRepository;
    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly ILanguageRepository _languageRepository;
    private readonly IEntityService _entityService;
    private readonly IContentService _contentService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly ILogger<ContentVersionService> _logger;
    private readonly IOptionsMonitor<ContentSettings> _contentSettings;
    private readonly ICoreScopeProvider _scopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentVersionService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="documentVersionRepository">The document version repository.</param>
    /// <param name="contentVersionCleanupPolicy">The content version cleanup policy.</param>
    /// <param name="scopeProvider">The scope provider.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="languageRepository">The language repository.</param>
    /// <param name="entityService">The entity service.</param>
    /// <param name="contentService">The content service.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    /// <param name="contentSettings">The content settings.</param>
    public ContentVersionService(
        ILogger<ContentVersionService> logger,
        IDocumentVersionRepository documentVersionRepository,
        IContentVersionCleanupPolicy contentVersionCleanupPolicy,
        ICoreScopeProvider scopeProvider,
        IEventMessagesFactory eventMessagesFactory,
        IAuditService auditService,
        ILanguageRepository languageRepository,
        IEntityService entityService,
        IContentService contentService,
        IUserIdKeyResolver userIdKeyResolver,
        IOptionsMonitor<ContentSettings> contentSettings)
    {
        _logger = logger;
        _documentVersionRepository = documentVersionRepository;
        _contentVersionCleanupPolicy = contentVersionCleanupPolicy;
        _scopeProvider = scopeProvider;
        _eventMessagesFactory = eventMessagesFactory;
        _auditService = auditService;
        _languageRepository = languageRepository;
        _entityService = entityService;
        _contentService = contentService;
        _userIdKeyResolver = userIdKeyResolver;
        _contentSettings = contentSettings;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<ContentVersionMeta> PerformContentVersionCleanup(DateTime asAtDate) =>

        // Media - ignored
        // Members - ignored
        CleanupDocumentVersions(asAtDate);

    /// <summary>
    /// Gets a content version by its ID.
    /// </summary>
    /// <param name="versionId">The version ID.</param>
    /// <returns>The content version metadata, or null if not found.</returns>
    public ContentVersionMeta? Get(int versionId)
    {
        using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentVersionRepository.Get(versionId);
        }
    }

    /// <inheritdoc />
    public Task<Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus>> GetPagedContentVersionsAsync(Guid contentId, string? culture, int skip, int take)
    {
        IEntitySlim? document = _entityService.Get(contentId, UmbracoObjectTypes.Document);
        if (document is null)
        {
            return Task.FromResult(Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus>.Fail(ContentVersionOperationStatus.ContentNotFound));
        }

        if (PaginationConverter.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize) == false)
        {
            return Task.FromResult(Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus>.Fail(ContentVersionOperationStatus.InvalidSkipTake));
        }

        IEnumerable<ContentVersionMeta> versions =
            HandleGetPagedContentVersions(
                document.Id,
                pageNumber,
                pageSize,
                out var total,
                culture);

        return Task.FromResult(Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus>.Succeed(
            ContentVersionOperationStatus.Success, new PagedModel<ContentVersionMeta>(total, versions)));
    }

    /// <inheritdoc />
    public Task<Attempt<IContent?, ContentVersionOperationStatus>> GetAsync(Guid versionId)
    {
        IContent? version = _contentService.GetVersion(versionId.ToInt());
        if (version is null)
        {
            return Task.FromResult(Attempt<IContent?, ContentVersionOperationStatus>.Fail(ContentVersionOperationStatus.NotFound));
        }

        return Task.FromResult(Attempt<IContent?, ContentVersionOperationStatus>.Succeed(ContentVersionOperationStatus.Success, version));
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentVersionOperationStatus>> SetPreventCleanupAsync(Guid versionId, bool preventCleanup, Guid userKey)
    {
        ContentVersionMeta? version = Get(versionId.ToInt());
        if (version is null)
        {
            return Attempt<ContentVersionOperationStatus>.Fail(ContentVersionOperationStatus.NotFound);
        }

        HandleSetPreventCleanup(version.VersionId, preventCleanup, await _userIdKeyResolver.GetAsync(userKey));

        return Attempt<ContentVersionOperationStatus>.Succeed(ContentVersionOperationStatus.Success);
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentVersionOperationStatus>> RollBackAsync(Guid versionId, string? culture, Guid userKey)
    {
        ContentVersionMeta? version = Get(versionId.ToInt());
        if (version is null)
        {
            return Attempt<ContentVersionOperationStatus>.Fail(ContentVersionOperationStatus.NotFound);
        }

        OperationResult rollBackResult = _contentService.Rollback(
            version.ContentId,
            version.VersionId,
            culture ?? "*",
            await _userIdKeyResolver.GetAsync(userKey));

        if (rollBackResult.Success)
        {
            return Attempt<ContentVersionOperationStatus>.Succeed(ContentVersionOperationStatus.Success);
        }

        switch (rollBackResult.Result)
        {
            case OperationResultType.Failed:
            case OperationResultType.FailedCannot:
            case OperationResultType.FailedExceptionThrown:
            case OperationResultType.NoOperation:
            default:
                return Attempt<ContentVersionOperationStatus>.Fail(ContentVersionOperationStatus.RollBackFailed);
            case OperationResultType.FailedCancelledByEvent:
                return Attempt<ContentVersionOperationStatus>.Fail(ContentVersionOperationStatus.RollBackCanceled);
        }
    }

    private IEnumerable<ContentVersionMeta> HandleGetPagedContentVersions(
        int contentId,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        string? culture = null)
    {
        using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
        {
            var languageId = _languageRepository.GetIdByIsoCode(culture, true);
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentVersionRepository.GetPagedItemsByContentId(contentId, pageIndex, pageSize, out totalRecords, languageId);
        }
    }

    private void HandleSetPreventCleanup(int versionId, bool preventCleanup, int userId)
    {
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);
            _documentVersionRepository.SetPreventCleanup(versionId, preventCleanup);

            ContentVersionMeta? version = _documentVersionRepository.Get(versionId);

            if (version is null)
            {
                scope.Complete();
                return;
            }

            AuditType auditType = preventCleanup
                ? AuditType.ContentVersionPreventCleanup
                : AuditType.ContentVersionEnableCleanup;

            var message = $"set preventCleanup = '{preventCleanup}' for version '{versionId}'";

            Audit(auditType, userId, version.ContentId, message, $"{version.VersionDate}");
            scope.Complete();
        }
    }

    private IReadOnlyCollection<ContentVersionMeta> CleanupDocumentVersions(DateTime asAtDate)
    {
        List<ContentVersionMeta> versionsToDelete;
        var fetchWasCapped = false;

        Configuration.Models.ContentVersionCleanupPolicySettings versionCleanupPolicy = _contentSettings.CurrentValue.ContentVersionCleanupPolicy;

        // Use the smallest KeepAllVersionsNewerThanDays across global + per-content-type overrides as the SQL date cutoff.
        // This may load some rows the C# policy will then keep, but ensures we never miss rows that should be deleted.
        int effectiveKeepAllDays = versionCleanupPolicy.KeepAllVersionsNewerThanDays;
        using (_scopeProvider.CreateCoreScope(autoComplete: true))
        {
            IReadOnlyCollection<Models.ContentVersionCleanupPolicySettings> overrides =
                _documentVersionRepository.GetCleanupPolicies();
            foreach (Models.ContentVersionCleanupPolicySettings policyOverride in overrides)
            {
                if (policyOverride.KeepAllVersionsNewerThanDays.HasValue)
                {
                    effectiveKeepAllDays = Math.Min(effectiveKeepAllDays, policyOverride.KeepAllVersionsNewerThanDays.Value);
                }
            }
        }

        DateTime olderThan = asAtDate.AddDays(-effectiveKeepAllDays);
        int? fetchLimit = versionCleanupPolicy.MaxVersionsToDeletePerRun > 0
            ? versionCleanupPolicy.MaxVersionsToDeletePerRun
            : null;

        // Multiple scopes are used intentionally so that locks are not held for the entire duration.
        // This allows other database connections to acquire locks between batches, keeping the backoffice responsive during
        // large cleanup operations.
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            IReadOnlyCollection<ContentVersionMeta> allHistoricVersions =
                _documentVersionRepository.GetDocumentVersionsEligibleForCleanup(olderThan, fetchLimit);

            if (allHistoricVersions.Count == 0)
            {
                scope.Complete();
                return [];
            }

            fetchWasCapped = fetchLimit.HasValue && allHistoricVersions.Count >= fetchLimit.Value;

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Discovered {Count} candidate(s) for ContentVersion cleanup", allHistoricVersions.Count);
            }

            versionsToDelete = new List<ContentVersionMeta>(allHistoricVersions.Count);

            IEnumerable<ContentVersionMeta> filteredContentVersions = _contentVersionCleanupPolicy.Apply(asAtDate, allHistoricVersions);

            foreach (ContentVersionMeta version in filteredContentVersions)
            {
                EventMessages messages = _eventMessagesFactory.Get();

                if (scope.Notifications.PublishCancelable(
                        new ContentDeletingVersionsNotification(version.ContentId, messages, version.VersionId)))
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Delete cancelled for ContentVersion [{VersionId}]", version.VersionId);
                    }

                    continue;
                }

                versionsToDelete.Add(version);
            }

            scope.Complete();
        }

        if (versionsToDelete.Count == 0)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("No remaining ContentVersions for cleanup");
            }

            return [];
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Removing {Count} ContentVersion(s)", versionsToDelete.Count);
        }

        foreach (IEnumerable<ContentVersionMeta> group in versionsToDelete.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            using (ICoreScope scope = _scopeProvider.CreateCoreScope())
            {
                scope.WriteLock(Constants.Locks.ContentTree);
                var groupEnumerated = group.ToList();
                _documentVersionRepository.DeleteVersions(groupEnumerated.Select(x => x.VersionId));

                foreach (ContentVersionMeta version in groupEnumerated)
                {
                    EventMessages messages = _eventMessagesFactory.Get();

                    scope.Notifications.Publish(
                        new ContentDeletedVersionsNotification(version.ContentId, messages, version.VersionId));
                }

                scope.Complete();
            }
        }

        if (fetchWasCapped)
        {
            _logger.LogInformation(
                "Reached per-run cap of {MaxVersionsToDeletePerRun}. Remaining versions will be cleaned up in subsequent runs.",
                fetchLimit);
        }

        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
#pragma warning disable CS0618 // Type or member is obsolete
            Audit(AuditType.Delete, Constants.Security.SuperUserId, -1, $"Removed {versionsToDelete.Count} ContentVersion(s) according to cleanup policy");
#pragma warning restore CS0618 // Type or member is obsolete

            scope.Complete();
        }

        return versionsToDelete;
    }

    private void Audit(AuditType type, int userId, int objectId, string? message = null, string? parameters = null) =>
        AuditAsync(type, userId, objectId, message, parameters).GetAwaiter().GetResult();

    private async Task AuditAsync(AuditType type, int userId, int objectId, string? message = null, string? parameters = null)
    {
        Guid userKey = await _userIdKeyResolver.GetAsync(userId);

        await _auditService.AddAsync(
            type,
            userKey,
            objectId,
            UmbracoObjectTypes.Document.GetName(),
            message,
            parameters);
    }
}
