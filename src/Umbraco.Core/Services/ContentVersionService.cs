using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
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

internal sealed class ContentVersionService : IContentVersionService
{
    private readonly IAuditRepository _auditRepository;
    private readonly IContentVersionCleanupPolicy _contentVersionCleanupPolicy;
    private readonly IDocumentVersionRepository _documentVersionRepository;
    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly ILanguageRepository _languageRepository;
    private readonly IEntityService _entityService;
    private readonly IContentService _contentService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly ILogger<ContentVersionService> _logger;
    private readonly ICoreScopeProvider _scopeProvider;

    public ContentVersionService(
        ILogger<ContentVersionService> logger,
        IDocumentVersionRepository documentVersionRepository,
        IContentVersionCleanupPolicy contentVersionCleanupPolicy,
        ICoreScopeProvider scopeProvider,
        IEventMessagesFactory eventMessagesFactory,
        IAuditRepository auditRepository,
        ILanguageRepository languageRepository,
        IEntityService entityService,
        IContentService contentService,
        IUserIdKeyResolver userIdKeyResolver)
    {
        _logger = logger;
        _documentVersionRepository = documentVersionRepository;
        _contentVersionCleanupPolicy = contentVersionCleanupPolicy;
        _scopeProvider = scopeProvider;
        _eventMessagesFactory = eventMessagesFactory;
        _auditRepository = auditRepository;
        _languageRepository = languageRepository;
        _entityService = entityService;
        _contentService = contentService;
        _userIdKeyResolver = userIdKeyResolver;
    }

    [Obsolete("Use the non obsolete constructor instead. Scheduled to be removed in v15")]
    public ContentVersionService(
        ILogger<ContentVersionService> logger,
        IDocumentVersionRepository documentVersionRepository,
        IContentVersionCleanupPolicy contentVersionCleanupPolicy,
        ICoreScopeProvider scopeProvider,
        IEventMessagesFactory eventMessagesFactory,
        IAuditRepository auditRepository,
        ILanguageRepository languageRepository)
    {
        _logger = logger;
        _documentVersionRepository = documentVersionRepository;
        _contentVersionCleanupPolicy = contentVersionCleanupPolicy;
        _scopeProvider = scopeProvider;
        _eventMessagesFactory = eventMessagesFactory;
        _auditRepository = auditRepository;
        _languageRepository = languageRepository;
        _entityService = StaticServiceProvider.Instance.GetRequiredService<IEntityService>();
        _contentService = StaticServiceProvider.Instance.GetRequiredService<IContentService>();
        _userIdKeyResolver = StaticServiceProvider.Instance.GetRequiredService<IUserIdKeyResolver>();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<ContentVersionMeta> PerformContentVersionCleanup(DateTime asAtDate) =>

        // Media - ignored
        // Members - ignored
        CleanupDocumentVersions(asAtDate);

    /// <inheritdoc />
    [Obsolete("Use the async version instead. Scheduled to be removed in v15")]
    public IEnumerable<ContentVersionMeta>? GetPagedContentVersions(int contentId, long pageIndex, int pageSize, out long totalRecords, string? culture = null)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        return HandleGetPagedContentVersions(contentId, pageIndex, pageSize, out totalRecords, culture);
    }

    public ContentVersionMeta? Get(int versionId)
    {
        using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(Constants.Locks.ContentTree);
            return _documentVersionRepository.Get(versionId);
        }
    }

    /// <inheritdoc />
    [Obsolete("Use the async version instead. Scheduled to be removed in v15")]
    public void SetPreventCleanup(int versionId, bool preventCleanup, int userId = Constants.Security.SuperUserId)
        => HandleSetPreventCleanup(versionId, preventCleanup, userId);

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

        IEnumerable<ContentVersionMeta>? versions =
            HandleGetPagedContentVersions(
                document.Id,
                pageNumber,
                pageSize,
                out var total,
                culture);

        if (versions is null)
        {
            return Task.FromResult(Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus>.Fail(ContentVersionOperationStatus.NotFound));
        }

        return Task.FromResult(Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus>.Succeed(
            ContentVersionOperationStatus.Success, new PagedModel<ContentVersionMeta>(total, versions)));
    }

    public Task<Attempt<IContent?, ContentVersionOperationStatus>> GetAsync(Guid versionId)
    {
        IContent? version = _contentService.GetVersion(versionId.ToInt());
        if (version is null)
        {
            return Task.FromResult(Attempt<IContent?, ContentVersionOperationStatus>.Fail(ContentVersionOperationStatus
                .NotFound));
        }

        return Task.FromResult(Attempt<IContent?, ContentVersionOperationStatus>.Succeed(ContentVersionOperationStatus.Success, version));
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

    private IEnumerable<ContentVersionMeta>? HandleGetPagedContentVersions(int contentId, long pageIndex,
        int pageSize, out long totalRecords, string? culture = null)
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

        /* Why so many scopes?
         *
         * We could just work out the set to delete at SQL infra level which was the original plan, however we agreed that really we should fire
         * ContentService.DeletingVersions so people can hook & cancel if required.
         *
         * On first time run of cleanup on a site with a lot of history there may be a lot of historic ContentVersions to remove e.g. 200K for our.umbraco.com.
         * If we weren't supporting SQL CE we could do TVP, or use temp tables to bulk delete with joins to our list of version ids to nuke.
         * (much nicer, we can kill 100k in sub second time-frames).
         *
         * However we are supporting SQL CE, so the easiest thing to do is use the Umbraco InGroupsOf helper to create a query with 2K args of version
         * ids to delete at a time.
         *
         * This is already done at the repository level, however if we only had a single scope at service level we're still locking
         * the ContentVersions table (and other related tables) for a couple of minutes which makes the back office unusable.
         *
         * As a quick fix, we can also use InGroupsOf at service level, create a scope per group to give other connections a chance
         * to grab the locks and execute their queries.
         *
         * This makes the back office a tiny bit sluggish during first run but it is usable for loading tree and publishing content.
         *
         * There are optimizations we can do, we could add a bulk delete for SqlServerSyntaxProvider which differs in implementation
         * and fallback to this naive approach only for SQL CE, however we agreed it is not worth the effort as this is a one time pain,
         * subsequent runs shouldn't have huge numbers of versions to cleanup.
         *
         * tl;dr lots of scopes to enable other connections to use the DB whilst we work.
         */
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            IReadOnlyCollection<ContentVersionMeta>? allHistoricVersions =
                _documentVersionRepository.GetDocumentVersionsEligibleForCleanup();

            if (allHistoricVersions is null)
            {
                scope.Complete();
                return Array.Empty<ContentVersionMeta>();
            }
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                _logger.LogDebug("Discovered {count} candidate(s) for ContentVersion cleanup", allHistoricVersions.Count);
            }
            versionsToDelete = new List<ContentVersionMeta>(allHistoricVersions.Count);

            IEnumerable<ContentVersionMeta> filteredContentVersions =
                _contentVersionCleanupPolicy.Apply(asAtDate, allHistoricVersions);

            foreach (ContentVersionMeta version in filteredContentVersions)
            {
                EventMessages messages = _eventMessagesFactory.Get();

                if (scope.Notifications.PublishCancelable(
                        new ContentDeletingVersionsNotification(version.ContentId, messages, version.VersionId)))
                {
                    if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                    {
                        _logger.LogDebug("Delete cancelled for ContentVersion [{versionId}]", version.VersionId);
                    }
                    continue;
                }

                versionsToDelete.Add(version);
            }

            scope.Complete();
        }

        if (!versionsToDelete.Any())
        {
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                _logger.LogDebug("No remaining ContentVersions for cleanup");
            }
            return Array.Empty<ContentVersionMeta>();
        }

        _logger.LogDebug("Removing {count} ContentVersion(s)", versionsToDelete.Count);

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

        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            Audit(AuditType.Delete, Constants.Security.SuperUserId, -1, $"Removed {versionsToDelete.Count} ContentVersion(s) according to cleanup policy");

            scope.Complete();
        }

        return versionsToDelete;
    }

    private void Audit(AuditType type, int userId, int objectId, string? message = null, string? parameters = null)
    {
        var entry = new AuditItem(
            objectId,
            type,
            userId,
            UmbracoObjectTypes.Document.GetName(),
            message,
            parameters);

        _auditRepository.Save(entry);
    }
}
