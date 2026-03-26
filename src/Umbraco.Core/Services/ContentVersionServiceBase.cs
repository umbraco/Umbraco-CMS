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

namespace Umbraco.Cms.Core.Services;

internal abstract class ContentVersionServiceBase<TContent>
    where TContent : class, IPublishableContentBase
{
    private readonly IAuditService _auditService;
    private readonly IContentVersionCleanupPolicy _contentVersionCleanupPolicy;
    private readonly IContentVersionRepository _contentVersionRepository;
    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly ILanguageRepository _languageRepository;
    private readonly IEntityService _entityService;
    private readonly IPublishableContentService<TContent> _contentService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly ILogger<ContentVersionServiceBase<TContent>> _logger;
    private readonly IOptionsMonitor<ContentSettings> _contentSettings;
    private readonly ICoreScopeProvider _scopeProvider;

    protected abstract UmbracoObjectTypes ItemObjectType { get; }

    public ContentVersionServiceBase(
        ILogger<ContentVersionServiceBase<TContent>> logger,
        IContentVersionRepository contentVersionRepository,
        IContentVersionCleanupPolicy contentVersionCleanupPolicy,
        ICoreScopeProvider scopeProvider,
        IEventMessagesFactory eventMessagesFactory,
        IAuditService auditService,
        ILanguageRepository languageRepository,
        IEntityService entityService,
        IPublishableContentService<TContent> contentService,
        IUserIdKeyResolver userIdKeyResolver,
        IOptionsMonitor<ContentSettings> contentSettings)
    {
        _logger = logger;
        _contentVersionRepository = contentVersionRepository;
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

    protected abstract DeletingVersionsNotification<TContent> DeletingVersionsNotification(int id, EventMessages messages, int specificVersion);

    protected abstract DeletedVersionsNotification<TContent> DeletedVersionsNotification(int id, EventMessages messages, int specificVersion);

    /// <inheritdoc />
    public IReadOnlyCollection<ContentVersionMeta> PerformContentVersionCleanup(DateTime asAtDate) =>

        // Media - ignored
        // Members - ignored
        CleanupItemVersions(asAtDate);

    public ContentVersionMeta? Get(int versionId)
    {
        using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _contentVersionRepository.Get(versionId);
        }
    }

    public Task<Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus>> GetPagedContentVersionsAsync(Guid contentId, string? culture, int skip, int take)
    {
        IEntitySlim? entity = _entityService.Get(contentId, ItemObjectType);
        if (entity is null)
        {
            return Task.FromResult(Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus>.Fail(ContentVersionOperationStatus.ContentNotFound));
        }

        if (PaginationConverter.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize) == false)
        {
            return Task.FromResult(Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus>.Fail(ContentVersionOperationStatus.InvalidSkipTake));
        }

        IEnumerable<ContentVersionMeta> versions =
            HandleGetPagedContentVersions(
                entity.Id,
                pageNumber,
                pageSize,
                out var total,
                culture);

        return Task.FromResult(Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus>.Succeed(
            ContentVersionOperationStatus.Success, new PagedModel<ContentVersionMeta>(total, versions)));
    }

    public Task<Attempt<TContent?, ContentVersionOperationStatus>> GetAsync(Guid versionId)
    {
        TContent? version = _contentService.GetVersion(versionId.ToInt());
        if (version is null)
        {
            return Task.FromResult(Attempt<TContent?, ContentVersionOperationStatus>.Fail(ContentVersionOperationStatus.NotFound));
        }

        return Task.FromResult(Attempt<TContent?, ContentVersionOperationStatus>.Succeed(ContentVersionOperationStatus.Success, version));
    }

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
            return _contentVersionRepository.GetPagedItemsByContentId(contentId, pageIndex, pageSize, out totalRecords, languageId);
        }
    }

    private void HandleSetPreventCleanup(int versionId, bool preventCleanup, int userId)
    {
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            scope.WriteLock(Constants.Locks.ContentTree);
            _contentVersionRepository.SetPreventCleanup(versionId, preventCleanup);

            ContentVersionMeta? version = _contentVersionRepository.Get(versionId);

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

    private IReadOnlyCollection<ContentVersionMeta> CleanupItemVersions(DateTime asAtDate)
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
                _contentVersionRepository.GetCleanupPolicies();
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
                _contentVersionRepository.GetContentVersionsEligibleForCleanup(olderThan, fetchLimit);

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

                if (scope.Notifications.PublishCancelable(DeletingVersionsNotification(version.ContentId, messages, version.VersionId)))
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
                _contentVersionRepository.DeleteVersions(groupEnumerated.Select(x => x.VersionId));

                foreach (ContentVersionMeta version in groupEnumerated)
                {
                    EventMessages messages = _eventMessagesFactory.Get();

                    scope.Notifications.Publish(DeletedVersionsNotification(version.ContentId, messages, version.VersionId));
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
            ItemObjectType.GetName(),
            message,
            parameters);
    }
}
