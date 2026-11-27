using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Implements the media service.
/// </summary>
public class MediaService : RepositoryService, IMediaService
{
    private readonly IAsyncMediaRepository _mediaRepository;
    private readonly IMediaTypeRepository _mediaTypeRepository;
    private readonly IAuditService _auditService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly IIdKeyMap _idKeyMap;
    private readonly MediaFileManager _mediaFileManager;
    private readonly IMediaPathScheme _mediaPathScheme;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ILogger<MediaService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="mediaRepository">The media repository.</param>
    /// <param name="mediaTypeRepository">The media type repository.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    /// <param name="idKeyMap">The ID key map.</param>
    /// <param name="mediaFileManager">The media file manager, for the file-system members.</param>
    /// <param name="mediaPathScheme">The media path scheme, which decides whether version 7 Guid keys can be saved.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    public MediaService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IAuditService auditService,
        IAsyncMediaRepository mediaRepository,
        IMediaTypeRepository mediaTypeRepository,
        IUserIdKeyResolver userIdKeyResolver,
        IIdKeyMap idKeyMap,
        MediaFileManager mediaFileManager,
        IMediaPathScheme mediaPathScheme,
        IShortStringHelper shortStringHelper)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _auditService = auditService;
        _mediaRepository = mediaRepository;
        _mediaTypeRepository = mediaTypeRepository;
        _userIdKeyResolver = userIdKeyResolver;
        _idKeyMap = idKeyMap;
        _mediaFileManager = mediaFileManager;
        _mediaPathScheme = mediaPathScheme;
        _shortStringHelper = shortStringHelper;
        _logger = loggerFactory.CreateLogger<MediaService>();
    }

    #region Count

    /// <inheritdoc />
    public async Task<int> CountAsync(string? mediaTypeAlias, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        int result = mediaTypeAlias is null
            ? await _mediaRepository.CountAsync(cancellationToken)
            : await _mediaRepository.CountAsync(mediaTypeAlias, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<int> CountChildrenAsync(Guid? parentKey, string? mediaTypeAlias, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        int result = mediaTypeAlias is null
            ? await _mediaRepository.CountChildrenAsync(parentKey, cancellationToken)
            : await _mediaRepository.CountChildrenAsync(parentKey, mediaTypeAlias, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<int> CountDescendantsAsync(Guid? parentKey, string? mediaTypeAlias, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        int result = mediaTypeAlias is null
            ? await _mediaRepository.CountDescendantsAsync(parentKey, cancellationToken)
            : await _mediaRepository.CountDescendantsAsync(parentKey, mediaTypeAlias, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<bool> HasChildrenAsync(Guid key, CancellationToken cancellationToken) =>
        await CountChildrenAsync(key, null, cancellationToken) > 0;

    #endregion

    #region Get

    /// <inheritdoc />
    public async Task<IMedia?> GetByIdAsync(Guid key, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        IMedia? result = await _mediaRepository.GetAsync(key, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IMedia>> GetByIdsAsync(IEnumerable<Guid> keys, CancellationToken cancellationToken)
    {
        Guid[] keysA = [.. keys];
        if (keysA.Length == 0)
        {
            return [];
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        IEnumerable<IMedia> items = await _mediaRepository.GetManyAsync(keysA, cancellationToken);
        var index = items.ToDictionary(x => x.Key, x => x);
        IEnumerable<IMedia> result = keysA.Select(x => index.GetValueOrDefault(x)).WhereNotNull();
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IMedia>> GetByIdsAsync(IEnumerable<Guid> keys, string[]? propertyAliases, CancellationToken cancellationToken)
    {
        Guid[] keysA = [.. keys.Distinct()];
        if (keysA.Length == 0)
        {
            return [];
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        IEnumerable<IMedia> items = await _mediaRepository.GetManyAsync(keysA, propertyAliases, cancellationToken);
        var index = items.ToDictionary(x => x.Key, x => x);
        IEnumerable<IMedia> result = keysA.Select(x => index.GetValueOrDefault(x)).WhereNotNull();
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<PagedModel<IMedia>> GetPagedOfTypeAsync(Guid mediaTypeKey, int skip, int take, Ordering? ordering, CancellationToken cancellationToken)
    {
        ordering ??= Ordering.By("sortOrder");

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        PagedModel<IMedia> result = await _mediaRepository.GetPagedOfContentTypesAsync([mediaTypeKey], skip, take, ordering, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<PagedModel<IMedia>> GetPagedOfTypesAsync(Guid[] mediaTypeKeys, int skip, int take, Ordering? ordering, CancellationToken cancellationToken)
    {
        ordering ??= Ordering.By("sortOrder");

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        PagedModel<IMedia> result = await _mediaRepository.GetPagedOfContentTypesAsync(mediaTypeKeys, skip, take, ordering, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IMedia>> GetRootMediaAsync(CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        IEnumerable<IMedia> result = await _mediaRepository.GetRootContentAsync(cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<PagedModel<IMedia>> GetPagedMediaInRecycleBinAsync(int skip, int take, Ordering? ordering, CancellationToken cancellationToken)
    {
        ordering ??= Ordering.By("Path");

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        PagedModel<IMedia> result = await _mediaRepository.GetPagedRecycleBinAsync(skip, take, ordering, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<PagedModel<IMedia>> GetByLevelAsync(int level, int skip, int take, Ordering? ordering, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        PagedModel<IMedia> result = await _mediaRepository.GetByLevelAsync(level, skip, take, ordering, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<PagedModel<IMedia>> GetAncestorsAsync(Guid key, int skip, int take, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        PagedModel<IMedia> result = await _mediaRepository.GetAncestorsAsync(key, skip, take, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public Task<PagedModel<IMedia>> GetAncestorsAsync(IMedia media, int skip, int take, CancellationToken cancellationToken) =>
        GetAncestorsAsync(media.Key, skip, take, cancellationToken);

    /// <inheritdoc />
    public async Task<PagedModel<IMedia>> GetChildrenAsync(Guid? parentKey, int skip, int take, string[]? propertyAliases, Ordering? ordering, CancellationToken cancellationToken)
    {
        ordering ??= Ordering.By("sortOrder");

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        PagedModel<IMedia> result = await _mediaRepository.GetChildrenAsync(parentKey, skip, take, propertyAliases, ordering, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<PagedModel<IMedia>> GetDescendantsAsync(Guid ancestorKey, int skip, int take, Ordering? ordering, CancellationToken cancellationToken, bool includeTrashed = true)
    {
        ordering ??= Ordering.By("Path");

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        PagedModel<IMedia> result = await _mediaRepository.GetDescendantsAsync(ancestorKey, skip, take, ordering, cancellationToken, includeTrashed);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<IMedia?> GetParentAsync(Guid key, CancellationToken cancellationToken)
    {
        IMedia? media = await GetByIdAsync(key, cancellationToken);
        if (media is null || media.ParentId == Constants.System.Root || media.ParentId == Constants.System.RecycleBinMedia)
        {
            return null;
        }

        Guid? parentKey = media.ParentKey;
        return parentKey is null
            ? null
            : await GetByIdAsync(parentKey.Value, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IMedia?> GetParentAsync(IMedia? media, CancellationToken cancellationToken)
    {
        if (media?.ParentId == Constants.System.Root || media?.ParentId == Constants.System.RecycleBinMedia ||
            media is null)
        {
            return null;
        }

        // media is caller-supplied and not guaranteed to have gone through a read path that populated
        // ParentKey (e.g. a freshly constructed, unsaved entity), so resolve it here when it is unknown.
        // Everywhere else ParentKey is expected to already be populated.
        if (media.TryGetParentKey(out Guid? parentKey) is false)
        {
            Attempt<Guid> parentKeyAttempt = await _idKeyMap.GetKeyForIdAsync(media.ParentId, UmbracoObjectTypes.Media);
            parentKey = parentKeyAttempt.Success ? parentKeyAttempt.Result : null;
        }

        return parentKey is null
            ? null
            : await GetByIdAsync(parentKey.Value, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> RecycleBinSmellsAsync(CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        bool result = await _mediaRepository.RecycleBinSmellsAsync(cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<IMedia?> GetMediaByPathAsync(string mediaPath, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        IMedia? result = await _mediaRepository.GetMediaByPathAsync(mediaPath, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<IMedia?> GetVersionAsync(int versionId, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        IMedia? result = await _mediaRepository.GetVersionAsync(versionId, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IMedia>> GetVersionsAsync(Guid key, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
        IEnumerable<IMedia> result = await _mediaRepository.GetAllVersionsAsync(key, cancellationToken);
        scope.Complete();
        return result;
    }

    #endregion

    #region Create

    /// <inheritdoc />
    public async Task<IMedia> CreateAsync(string name, Guid? parentKey, string mediaTypeAlias, Guid userKey, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IMediaType mediaType = await GetMediaTypeAsync(scope, mediaTypeAlias, cancellationToken);
        IMedia media = await CreateAsync(name, parentKey, mediaType, userKey, cancellationToken);

        scope.Complete();
        return media;
    }

    /// <inheritdoc />
    public async Task<IMedia> CreateAsync(string name, Guid? parentKey, IMediaType mediaType, Guid userKey, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(mediaType);

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        IMedia? parent = null;
        if (parentKey.HasValue)
        {
            parent = await GetByIdAsync(parentKey.Value, cancellationToken);
            if (parent is null)
            {
                throw new ArgumentException("No media with that key.", nameof(parentKey));
            }
        }

        IMedia media = NewMedia(name, parent, mediaType, userId);

        scope.Complete();
        return media;
    }

    /// <inheritdoc />
    public async Task<IMedia> CreateAsync(string name, IMedia parent, string mediaTypeAlias, Guid userKey, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(parent);

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        int userId = await _userIdKeyResolver.GetAsync(userKey);
        IMediaType mediaType = await GetMediaTypeAsync(scope, mediaTypeAlias, cancellationToken);

        IMedia media = NewMedia(name, parent, mediaType, userId);

        scope.Complete();
        return media;
    }

    /// <inheritdoc />
    public async Task<IMedia> CreateAndSaveAsync(string name, Guid? parentKey, string mediaTypeAlias, Guid userKey, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // locking the media tree secures media types too
        scope.WriteLock(Constants.Locks.MediaTree);

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        IMediaType mediaType = await GetMediaTypeAsync(scope, mediaTypeAlias, cancellationToken);

        IMedia? parent = null;
        if (parentKey.HasValue)
        {
            parent = await GetByIdAsync(parentKey.Value, cancellationToken); // + locks
            if (parent is null)
            {
                throw new ArgumentException("No media with that key.", nameof(parentKey)); // causes rollback
            }
        }

        IMedia media = NewMedia(name, parent, mediaType, userId);

        Attempt<ContentSaveOperationStatus> saveResult = await SaveAsync(media, userKey, cancellationToken);
        ThrowIfSaveRejected(media, saveResult);

        scope.Complete();

        return media;
    }

    /// <inheritdoc />
    public async Task<IMedia> CreateAndSaveAsync(string name, IMedia parent, string mediaTypeAlias, Guid userKey, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(parent);

        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // locking the media tree secures media types too
        scope.WriteLock(Constants.Locks.MediaTree);

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        IMediaType mediaType = await GetMediaTypeAsync(scope, mediaTypeAlias, cancellationToken);

        IMedia media = NewMedia(name, parent, mediaType, userId);

        Attempt<ContentSaveOperationStatus> saveResult = await SaveAsync(media, userKey, cancellationToken);
        ThrowIfSaveRejected(media, saveResult);

        scope.Complete();
        return media;
    }

    private static IMedia NewMedia(string name, IMedia? parent, IMediaType mediaType, int userId)
    {
        Models.Media media = parent is not null
            ? new Models.Media(name, parent, mediaType)
            : new Models.Media(name, Constants.System.Root, mediaType);
        media.CreatorId = userId;
        media.WriterId = userId;
        return media;
    }

    // Saving rejects an invalid name or key with a status; creating-and-saving surfaces it as an exception so the
    // caller is never handed back an item that was silently left unsaved.
    private static void ThrowIfSaveRejected(IMedia media, Attempt<ContentSaveOperationStatus> saveResult)
    {
        switch (saveResult.Result)
        {
            case ContentSaveOperationStatus.InvalidName:
                throw new InvalidOperationException(
                    $"Media with the name {media.Name} cannot be empty or more than {Constants.Validation.MaxNameLength} characters in length.");
            case ContentSaveOperationStatus.InvalidKey:
                throw new InvalidOperationException(
                    $"Media with the name {media.Name} has a version 7 Guid key, which the registered media path scheme cannot use.");
            case ContentSaveOperationStatus.DuplicateKey:
                throw new InvalidOperationException(
                    $"Media with the name {media.Name} has a key that already exists.");
        }
    }

    #endregion

    #region Save

    /// <inheritdoc />
    public async Task<Attempt<ContentSaveOperationStatus>> SaveAsync(IMedia media, Guid userKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(media.Name) || media.Name.Length > Constants.Validation.MaxNameLength)
        {
            return Attempt.Fail(ContentSaveOperationStatus.InvalidName);
        }

        if (media.Key.Version == 7 && _mediaPathScheme.SupportsGuid7 is false)
        {
            _logger.LogWarning(
                "The registered implementation of IMediaPathScheme cannot be used with media keys using version 7 GUIDs due to an increased risk of collisions in the generated file paths. " +
                "Please use version 4 GUIDs created via Guid.NewGuid() or implement and register a different IMediaPathScheme.");
            return Attempt.Fail(ContentSaveOperationStatus.InvalidKey);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.MediaTree);

        var savingNotification = new MediaSavingNotification(media, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return Attempt.Fail(ContentSaveOperationStatus.CancelledByNotification);
        }

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        if (media.HasIdentity is false)
        {
            if (await _mediaRepository.ExistsAsync(media.Key, cancellationToken))
            {
                scope.Complete();
                return Attempt.Fail(ContentSaveOperationStatus.DuplicateKey);
            }

            media.CreatorId = userId;
        }

        media.WriterId = userId;

        await _mediaRepository.SaveAsync(media, cancellationToken);

        scope.Notifications.Publish(new MediaSavedNotification(media, eventMessages).WithStateFrom(savingNotification));
        scope.Notifications.Publish(new MediaTreeChangeNotification(media, TreeChangeTypes.RefreshNode, eventMessages));

        await AuditAsync(AuditType.Save, userId, media.Id);

        scope.Complete();

        return Attempt.Succeed(ContentSaveOperationStatus.Success);
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentSaveOperationStatus>> SaveAsync(IEnumerable<IMedia> medias, Guid userKey, CancellationToken cancellationToken)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();
        IMedia[] mediasA = [.. medias];

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.MediaTree);

        var savingNotification = new MediaSavingNotification(mediasA, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return Attempt.Fail(ContentSaveOperationStatus.CancelledByNotification);
        }

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        foreach (IMedia media in mediasA)
        {
            if (media.HasIdentity is false)
            {
                media.CreatorId = userId;
            }

            media.WriterId = userId;

            await _mediaRepository.SaveAsync(media, cancellationToken);
        }

        scope.Notifications.Publish(new MediaSavedNotification(mediasA, eventMessages).WithStateFrom(savingNotification));
        scope.Notifications.Publish(new MediaTreeChangeNotification(mediasA, TreeChangeTypes.RefreshNode, eventMessages));

        await AuditAsync(AuditType.Save, userId, Constants.System.Root, $"Saved multiple media items (#{mediasA.Length})");

        scope.Complete();

        return Attempt.Succeed(ContentSaveOperationStatus.Success);
    }

    #endregion

    #region Delete

    /// <inheritdoc />
    public async Task<Attempt<ContentDeleteOperationStatus>> DeleteAsync(IMedia media, Guid userKey, CancellationToken cancellationToken)
    {
        int userId = await _userIdKeyResolver.GetAsync(userKey);
        EventMessages eventMessages = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.MediaTree);

        if (await scope.Notifications.PublishCancelableAsync(new MediaDeletingNotification(media, eventMessages)))
        {
            scope.Complete();
            return Attempt.Fail(ContentDeleteOperationStatus.CancelledByNotification);
        }

        await DeleteLockedAsync(scope, media, eventMessages, cancellationToken);

        scope.Notifications.Publish(new MediaTreeChangeNotification(media, TreeChangeTypes.Remove, eventMessages));
        await AuditAsync(AuditType.Delete, userId, media.Id);

        scope.Complete();

        return Attempt.Succeed(ContentDeleteOperationStatus.Success);
    }

    // Deletes a media item and its descendants, deepest first, publishing one deleted notification per item so
    // the handlers that remove media files see every file.
    private async Task DeleteLockedAsync(ICoreScope scope, IMedia media, EventMessages evtMsgs, CancellationToken cancellationToken)
    {
        async Task DoDeleteAsync(IMedia m)
        {
            await _mediaRepository.DeleteAsync(m, cancellationToken);
            scope.Notifications.Publish(new MediaDeletedNotification(m, evtMsgs));
        }

        const int pageSize = 500;
        var total = long.MaxValue;
        while (total > 0)
        {
            // get descendants - ordered from deepest to shallowest
            PagedModel<IMedia> descendantsPage = await GetDescendantsAsync(media.Key, 0, pageSize, Ordering.By("Path", Direction.Descending), cancellationToken);
            total = descendantsPage.Total;
            foreach (IMedia descendant in descendantsPage.Items)
            {
                await DoDeleteAsync(descendant);
            }
        }

        await DoDeleteAsync(media);
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentVersionOperationStatus>> DeleteVersionsAsync(Guid key, DateTime versionDate, Guid userKey, CancellationToken cancellationToken)
    {
        int userId = await _userIdKeyResolver.GetAsync(userKey);
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.MediaTree);

        var deletingVersionsNotification = new MediaDeletingVersionsNotification(key, evtMsgs, dateToRetain: versionDate);
        if (await scope.Notifications.PublishCancelableAsync(deletingVersionsNotification))
        {
            scope.Complete();
            return Attempt.Fail(ContentVersionOperationStatus.CancelledByNotification);
        }

        await _mediaRepository.DeleteVersionsAsync(key, versionDate, cancellationToken);

        scope.Notifications.Publish(
            new MediaDeletedVersionsNotification(key, evtMsgs, dateToRetain: versionDate).WithStateFrom(deletingVersionsNotification));
        await AuditAsync(AuditType.Delete, userId, Constants.System.Root, "Delete Media by version date");

        scope.Complete();
        return Attempt.Succeed(ContentVersionOperationStatus.Success);
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentVersionOperationStatus>> DeleteVersionAsync(Guid key, int versionId, bool deletePriorVersions, Guid userKey, CancellationToken cancellationToken)
    {
        int userId = await _userIdKeyResolver.GetAsync(userKey);
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.MediaTree);

        var deletingVersionsNotification = new MediaDeletingVersionsNotification(key, evtMsgs, specificVersion: versionId);
        if (await scope.Notifications.PublishCancelableAsync(deletingVersionsNotification))
        {
            scope.Complete();
            return Attempt.Fail(ContentVersionOperationStatus.CancelledByNotification);
        }

        if (deletePriorVersions)
        {
            IMedia? version = await GetVersionAsync(versionId, cancellationToken);
            await DeleteVersionsAsync(key, version?.UpdateDate ?? DateTime.UtcNow, userKey, cancellationToken);
        }

        IMedia? media = await _mediaRepository.GetAsync(key, cancellationToken);

        // don't delete the current version
        if (media is null || media.VersionId != versionId)
        {
            await _mediaRepository.DeleteVersionAsync(versionId, cancellationToken);
        }

        scope.Notifications.Publish(
            new MediaDeletedVersionsNotification(key, evtMsgs, specificVersion: versionId).WithStateFrom(deletingVersionsNotification));
        await AuditAsync(AuditType.Delete, userId, Constants.System.Root, "Delete Media by version");

        scope.Complete();
        return Attempt.Succeed(ContentVersionOperationStatus.Success);
    }

    #endregion

    #region Move, RecycleBin

    /// <inheritdoc />
    public async Task<Attempt<ContentMoveToRecycleBinOperationStatus>> MoveToRecycleBinAsync(IMedia media, Guid userKey, CancellationToken cancellationToken)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();
        var moves = new List<(IMedia, string)>();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.MediaTree);

        var originalPath = media.Path;
        var moveEventInfo = new MoveToRecycleBinEventInfo<IMedia>(media, originalPath);

        var movingToRecycleBinNotification = new MediaMovingToRecycleBinNotification(moveEventInfo, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(movingToRecycleBinNotification))
        {
            scope.Complete();
            return Attempt.Fail(ContentMoveToRecycleBinOperationStatus.CancelledByNotification);
        }

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        await PerformMoveLockedAsync(media, Constants.System.RecycleBinMedia, null, userId, moves, true, cancellationToken);
        scope.Notifications.Publish(
            new MediaTreeChangeNotification(media, TreeChangeTypes.RefreshBranch, eventMessages));

        MoveToRecycleBinEventInfo<IMedia>[] moveInfo = [.. moves.Select(x => new MoveToRecycleBinEventInfo<IMedia>(x.Item1, x.Item2))];

        scope.Notifications.Publish(
            new MediaMovedToRecycleBinNotification(moveInfo, eventMessages).WithStateFrom(
                movingToRecycleBinNotification));

        await AuditAsync(AuditType.Move, userId, media.Id, $"Moved to recycle bin from parent {originalPath.GetParentIdFromPath()}");

        scope.Complete();

        return Attempt.Succeed(ContentMoveToRecycleBinOperationStatus.Success);
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentMoveOperationStatus>> MoveAsync(IMedia media, Guid? parentKey, bool includeDescendants, Guid userKey, CancellationToken cancellationToken)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        // if moving to the recycle bin then use the proper method
        if (parentKey == Constants.System.RecycleBinMediaKey)
        {
            if (media.ParentId == Constants.System.RecycleBinMedia)
            {
                return Attempt.Succeed(ContentMoveOperationStatus.Success);
            }

            Attempt<ContentMoveToRecycleBinOperationStatus> recycleBinResult = await MoveToRecycleBinAsync(media, userKey, cancellationToken);
            return recycleBinResult.Success
                ? Attempt.Succeed(ContentMoveOperationStatus.Success)
                : Attempt.Fail(ContentMoveOperationStatus.CancelledByNotification);
        }

        if (parentKey is null && media.ParentId == Constants.System.Root)
        {
            return Attempt.Succeed(ContentMoveOperationStatus.Success);
        }

        int userId = await _userIdKeyResolver.GetAsync(userKey);
        var moves = new List<(IMedia, string)>();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.MediaTree);

        IMedia? parent = parentKey.HasValue ? await GetByIdAsync(parentKey.Value, cancellationToken) : null;
        if (parentKey.HasValue && parent is null)
        {
            scope.Complete();
            return Attempt.Fail(ContentMoveOperationStatus.ParentNotFound);
        }

        if (parent?.Trashed is true)
        {
            scope.Complete();
            return Attempt.Fail(ContentMoveOperationStatus.ParentTrashed);
        }

        int parentId = parent?.Id ?? Constants.System.Root;

        // A null ParentKey means either "at the root" or "not populated", so the int id is the unambiguous comparison.
        if (media.ParentId == parentId)
        {
            scope.Complete();
            return Attempt.Succeed(ContentMoveOperationStatus.Success);
        }

        var moveEventInfo = new MoveEventInfo<IMedia>(media, media.Path, parentKey);

        var movingNotification = new MediaMovingNotification(moveEventInfo, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(movingNotification))
        {
            scope.Complete();
            return Attempt.Fail(ContentMoveOperationStatus.CancelledByNotification);
        }

        // if media was trashed, and since we're not moving to the recycle bin,
        // indicate that the trashed status should be changed to false, else just
        // leave it unchanged
        var trashed = media.Trashed ? false : (bool?)null;

        // when restoring a single item out of the recycle bin without its descendants, those descendants stay
        // trashed and are re-homed under the recycle bin root - see PerformMoveLockedAsync
        var leaveDescendantsInRecycleBin = includeDescendants is false && media.Trashed;

        await PerformMoveLockedAsync(media, parentId, parent, userId, moves, trashed, cancellationToken, includeDescendants);

        if (leaveDescendantsInRecycleBin)
        {
            // The single RefreshBranch above cannot reconcile the descendants left in the bin (they are no longer
            // descendants of the restored item), so also refresh the re-homed direct children. The navigation
            // reconciler then moves them - and their sub-trees - back under the recycle bin root.
            IMedia[] rehomedChildren = [.. moves
                .Select(x => x.Item1)
                .Where(x => x.ParentId == Constants.System.RecycleBinMedia)];
            scope.Notifications.Publish(
                new MediaTreeChangeNotification(media.Yield().Concat(rehomedChildren), TreeChangeTypes.RefreshBranch, eventMessages));
        }
        else
        {
            scope.Notifications.Publish(
                new MediaTreeChangeNotification(media, TreeChangeTypes.RefreshBranch, eventMessages));
        }

        // changes
        MoveEventInfo<IMedia>[] moveInfo = [.. moves.Select(x => new MoveEventInfo<IMedia>(x.Item1, x.Item2, x.Item1.ParentKey))];

        scope.Notifications.Publish(
            new MediaMovedNotification(moveInfo, eventMessages).WithStateFrom(movingNotification));

        await AuditAsync(AuditType.Move, userId, media.Id);

        scope.Complete();
        return Attempt.Succeed(ContentMoveOperationStatus.Success);
    }

    // MUST be called from within a write lock on Constants.Locks.MediaTree.
    // trash indicates whether we are trashing, un-trashing, or not changing anything.
    private async Task PerformMoveLockedAsync(IMedia media, int parentId, IMedia? parent, int userId, List<(IMedia Media, string OriginalPath)> moves, bool? trash, CancellationToken cancellationToken, bool includeDescendants = true)
    {
        media.WriterId = userId;
        media.ParentId = parentId;
        media.ParentKey = parentId switch
        {
            Constants.System.Root => null,
            Constants.System.RecycleBinMedia => Constants.System.RecycleBinMediaKey,
            _ => parent?.Key,
        };

        var levelDelta = 1 - media.Level + (parent?.Level ?? 0);
        var originalLevel = media.Level;
        var originalPath = media.Path;
        moves.Add((media, originalPath));

        // Fetch descendants by the media item's key before saving its own new parent below - GetDescendantsAsync
        // matches descendants via the item's NodeId embedded in their own Path strings, so this must happen
        // before those descendant rows are mutated by the moves later in this method.
        var descendants = new List<IMedia>();
        const int pageSize = 500;
        var page = 0;
        var total = long.MaxValue;
        while (page * pageSize < total)
        {
            PagedModel<IMedia> descendantsPage = await GetDescendantsAsync(media.Key, page++ * pageSize, pageSize, Ordering.By("Path"), cancellationToken);
            descendants.AddRange(descendantsPage.Items);
            total = descendantsPage.Total;
        }

        await PerformMoveMediaLockedAsync(media, userId, trash, cancellationToken);

        var paths = new Dictionary<int, string>
        {
            [media.Id] = (parent is null
                ? parentId == Constants.System.RecycleBinMedia ? Constants.System.RootString + "," + Constants.System.RecycleBinMediaString : Constants.System.RootString
                : parent.Path) + "," + media.Id,
        };

        // When restoring a single item out of the recycle bin without its descendants, the descendants must stay
        // trashed: the item's direct children are re-homed to the recycle bin root (and the rest of the subtree keeps
        // its relative structure), so nothing is orphaned and it can still be restored on its own later.
        var leaveDescendantsInRecycleBin = includeDescendants is false
            && parentId != Constants.System.RecycleBinMedia
            && originalPath.Contains(Constants.System.RecycleBinMediaString);

        foreach (IMedia descendant in descendants)
        {
            moves.Add((descendant, descendant.Path));

            if (leaveDescendantsInRecycleBin)
            {
                await LeaveDescendantInRecycleBinLockedAsync(descendant, media.Id, originalLevel, userId, paths, cancellationToken);
            }
            else
            {
                await PerformMoveDescendantLockedAsync(descendant, levelDelta, userId, trash, paths, cancellationToken);
            }
        }
    }

    // Re-homes a descendant of a restored item within the recycle bin: the restored item's direct children become
    // top-level recycle bin items, while deeper descendants keep their relative structure below their (now re-homed)
    // ancestor. The trashed state is left untouched so these items remain in the recycle bin.
    private async Task LeaveDescendantInRecycleBinLockedAsync(IMedia descendant, int restoredItemId, int originalLevel, int userId, Dictionary<int, string> paths, CancellationToken cancellationToken)
    {
        var isDirectChild = descendant.ParentId == restoredItemId;
        descendant.Path = paths[descendant.Id] = isDirectChild
            ? Constants.System.RecycleBinMediaPathPrefix + descendant.Id
            : paths[descendant.ParentId] + "," + descendant.Id;
        descendant.Level -= originalLevel;
        if (isDirectChild)
        {
            descendant.ParentId = Constants.System.RecycleBinMedia;
            descendant.ParentKey = Constants.System.RecycleBinMediaKey;
        }

        await PerformMoveMediaLockedAsync(descendant, userId, null, cancellationToken);
    }

    // Moves a descendant along with the item being moved, updating its path and level (parentId is unchanged).
    private async Task PerformMoveDescendantLockedAsync(IMedia descendant, int levelDelta, int userId, bool? trash, Dictionary<int, string> paths, CancellationToken cancellationToken)
    {
        descendant.Path = paths[descendant.Id] = paths[descendant.ParentId] + "," + descendant.Id;
        descendant.Level += levelDelta;
        await PerformMoveMediaLockedAsync(descendant, userId, trash, cancellationToken);
    }

    private async Task PerformMoveMediaLockedAsync(IMedia media, int userId, bool? trash, CancellationToken cancellationToken)
    {
        if (trash.HasValue)
        {
            ((ContentBase)media).Trashed = trash.Value;
        }

        media.WriterId = userId;
        await _mediaRepository.SaveAsync(media, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentEmptyRecycleBinOperationStatus>> EmptyRecycleBinAsync(Guid userKey, CancellationToken cancellationToken)
    {
        var deleted = new List<IMedia>();
        EventMessages eventMessages = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.MediaTree);

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        // emptying the recycle bin means deleting whatever is in there - do it properly!
        PagedModel<IMedia> mediasPage = await GetChildrenAsync(Constants.System.RecycleBinMediaKey, 0, int.MaxValue, propertyAliases: null, ordering: null, cancellationToken);
        IMedia[] medias = [.. mediasPage.Items];

        var emptyingRecycleBinNotification = new MediaEmptyingRecycleBinNotification(medias, eventMessages);
        var deletingMediaNotification = new MediaDeletingNotification(medias, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(emptyingRecycleBinNotification)
            || await scope.Notifications.PublishCancelableAsync(deletingMediaNotification))
        {
            scope.Complete();
            return Attempt.Fail(ContentEmptyRecycleBinOperationStatus.CancelledByNotification);
        }

        foreach (IMedia media in medias)
        {
            await DeleteLockedAsync(scope, media, eventMessages, cancellationToken);
            deleted.Add(media);
        }

        scope.Notifications.Publish(
            new MediaEmptiedRecycleBinNotification(deleted, eventMessages).WithStateFrom(
                emptyingRecycleBinNotification));
        scope.Notifications.Publish(
            new MediaTreeChangeNotification(deleted, TreeChangeTypes.Remove, eventMessages));
        await AuditAsync(AuditType.Delete, userId, Constants.System.RecycleBinMedia, "Recycle bin emptied");

        scope.Complete();

        return Attempt.Succeed(ContentEmptyRecycleBinOperationStatus.Success);
    }

    #endregion

    #region Sort

    /// <inheritdoc />
    public async Task<Attempt<ContentSortOperationStatus>> SortAsync(IReadOnlyList<Guid> orderedKeys, Guid userKey, CancellationToken cancellationToken)
    {
        if (orderedKeys.Count == 0)
        {
            return Attempt.Fail(ContentSortOperationStatus.NoOperation);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.MediaTree);

        // Reload within the lock so sorting operates on fully-loaded entities. Callers may pass
        // partially-loaded media (e.g. loaded without property data), and saving those directly would
        // wipe the property data (#23120).
        // GetByIdsAsync returns items in the requested order, preserving the caller's ordering that drives the sort.
        IMedia[] itemsA = [.. (await GetByIdsAsync(orderedKeys, cancellationToken))];

        // The sorting flag lets the handlers that extract file metadata on save skip a save that changes nothing
        // but the order.
        var savingNotification = new MediaSavingNotification(itemsA, eventMessages) { IsSorting = true };
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return Attempt.Fail(ContentSortOperationStatus.CancelledByNotification);
        }

        int userId = await _userIdKeyResolver.GetAsync(userKey);

        var saved = new List<IMedia>();
        var sortOrder = 0;

        foreach (IMedia media in itemsA)
        {
            // if the current sort order equals that of the media we don't
            // need to update it, so just increment the sort order and continue.
            if (media.SortOrder == sortOrder)
            {
                sortOrder++;
                continue;
            }

            // else update
            media.SortOrder = sortOrder++;
            media.WriterId = userId;

            // save
            saved.Add(media);
            await _mediaRepository.SaveAsync(media, cancellationToken);
            await AuditAsync(AuditType.Sort, userId, media.Id, "Sorting media performed by user");
        }

        scope.Notifications.Publish(
            new MediaSavedNotification(itemsA, eventMessages).WithStateFrom(savingNotification));

        scope.Notifications.Publish(
            new MediaTreeChangeNotification(saved, TreeChangeTypes.RefreshNode, eventMessages));

        scope.Complete();
        return Attempt.Succeed(ContentSortOperationStatus.Success);
    }

    /// <inheritdoc />
    public async Task<Attempt<ContentSortChildrenOperationStatus>> SortChildrenAsync(Guid? parentKey, IReadOnlyList<Guid> orderedChildKeys, Guid userKey, CancellationToken cancellationToken)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();
        if (orderedChildKeys.Count == 0)
        {
            return Attempt.Fail(ContentSortChildrenOperationStatus.NoOperation);
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.MediaTree);

        await _mediaRepository.UpdateSortOrderAsync(orderedChildKeys, cancellationToken);

        // Sort order lives in umbracoNode; neither the published cache nor the media repository cache keeps
        // a separate serialized copy of it, so refreshing the affected branch (which invalidates both and has
        // them reload from umbracoNode) is enough to pick up the new order without re-saving each child.
        int parentId;
        if (parentKey.HasValue)
        {
            IMedia? parent = await GetByIdAsync(parentKey.Value, cancellationToken);
            parentId = parent?.Id ?? Constants.System.Root;
            if (parent is not null)
            {
                scope.Notifications.Publish(new MediaTreeChangeNotification(parent, TreeChangeTypes.RefreshBranch, evtMsgs));
            }
        }
        else
        {
            parentId = Constants.System.Root;
            IEnumerable<IMedia> roots = await GetByIdsAsync(orderedChildKeys, cancellationToken);
            scope.Notifications.Publish(new MediaTreeChangeNotification(roots, TreeChangeTypes.RefreshNode, evtMsgs));
        }

        int userId = await _userIdKeyResolver.GetAsync(userKey);
        await AuditAsync(AuditType.Sort, userId, parentId);

        scope.Complete();
        return Attempt.Succeed(ContentSortChildrenOperationStatus.Success);
    }

    #endregion

    #region Media Types

    /// <inheritdoc />
    public Task<Attempt<ContentDeleteOfTypesOperationStatus>> DeleteOfTypeAsync(Guid mediaTypeKey, Guid userKey, CancellationToken cancellationToken) =>
        DeleteOfTypesAsync([mediaTypeKey], userKey, cancellationToken);

    /// <inheritdoc />
    public async Task<Attempt<ContentDeleteOfTypesOperationStatus>> DeleteOfTypesAsync(IEnumerable<Guid> mediaTypeKeys, Guid userKey, CancellationToken cancellationToken)
    {
        var changes = new List<TreeChange<IMedia>>();
        var moves = new List<(IMedia, string)>();
        Guid[] mediaTypeKeysArray = [.. mediaTypeKeys];
        EventMessages eventMessages = EventMessagesFactory.Get();

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.MediaTree);

        foreach (Guid mediaTypeKey in mediaTypeKeysArray)
        {
            if (await _mediaTypeRepository.ExistsAsync(mediaTypeKey, cancellationToken) is false)
            {
                scope.Complete();
                return Attempt.Fail(ContentDeleteOfTypesOperationStatus.NotFound);
            }
        }

        var medias = new List<IMedia>();
        const int pageSize = 500;
        var page = 0;
        var total = long.MaxValue;
        while (page * pageSize < total)
        {
            PagedModel<IMedia> mediasPage = await GetPagedOfTypesAsync(mediaTypeKeysArray, page++ * pageSize, pageSize, ordering: null, cancellationToken);
            medias.AddRange(mediasPage.Items);
            total = mediasPage.Total;
        }

        if (await scope.Notifications.PublishCancelableAsync(new MediaDeletingNotification(medias, eventMessages)))
        {
            scope.Complete();
            return Attempt.Fail(ContentDeleteOfTypesOperationStatus.CancelledByNotification);
        }

        int userId = await _userIdKeyResolver.GetAsync(userKey);
        HashSet<Guid> deletedTypeKeys = [.. mediaTypeKeysArray];

        // deepest first (by parent id), so that we cannot move a media of the deleted type to the recycle bin
        // and then delete it
        foreach (IMedia media in medias.OrderByDescending(x => x.ParentId))
        {
            // Every descendant of a surviving type ends up in the recycle bin, at any depth: one directly under a
            // deleted item is moved on its own and carries its subtree along, so a survivor deeper down rides with
            // its surviving ancestor instead of being moved a second time. The ordering above keeps that true
            // except when an ancestor of a deleted type has been moved under a newer node than one of its
            // deleted-type descendants; the end state is still right, the moved notification then lists a
            // survivor twice.
            List<IMedia> descendants = await GetAllDescendantsAsync(media, cancellationToken);
            var typeKeyById = descendants.ToDictionary(descendant => descendant.Id, descendant => descendant.ContentType.Key);
            typeKeyById[media.Id] = media.ContentType.Key;

            foreach (IMedia descendant in descendants)
            {
                if (deletedTypeKeys.Contains(descendant.ContentType.Key)
                    || deletedTypeKeys.Contains(typeKeyById[descendant.ParentId]) is false)
                {
                    continue;
                }

                // see MoveToRecycleBinAsync
                await PerformMoveLockedAsync(descendant, Constants.System.RecycleBinMedia, null, userId, moves, true, cancellationToken);
                changes.Add(new TreeChange<IMedia>(media, TreeChangeTypes.RefreshBranch));
            }

            // delete media
            // triggers the deleted event (and handles the files)
            await DeleteLockedAsync(scope, media, eventMessages, cancellationToken);
            changes.Add(new TreeChange<IMedia>(media, TreeChangeTypes.Remove));
        }

        MoveToRecycleBinEventInfo<IMedia>[] moveInfos = [.. moves.Select(x => new MoveToRecycleBinEventInfo<IMedia>(x.Item1, x.Item2))];
        if (moveInfos.Length > 0)
        {
            scope.Notifications.Publish(new MediaMovedToRecycleBinNotification(moveInfos, eventMessages));
        }

        scope.Notifications.Publish(new MediaTreeChangeNotification(changes, eventMessages));

        await AuditAsync(AuditType.Delete, userId, Constants.System.Root, $"Delete Media of types {string.Join(",", mediaTypeKeysArray)}");

        scope.Complete();
        return Attempt.Succeed(ContentDeleteOfTypesOperationStatus.Success);
    }

    private async Task<List<IMedia>> GetAllDescendantsAsync(IMedia media, CancellationToken cancellationToken)
    {
        var descendants = new List<IMedia>();
        const int pageSize = 500;
        var page = 0;
        var total = long.MaxValue;
        while (page * pageSize < total)
        {
            PagedModel<IMedia> descendantsPage = await GetDescendantsAsync(media.Key, page++ * pageSize, pageSize, Ordering.By("Path"), cancellationToken);
            descendants.AddRange(descendantsPage.Items);
            total = descendantsPage.Total;
        }

        return descendants;
    }

    private async Task<IMediaType> GetMediaTypeAsync(ICoreScope scope, string mediaTypeAlias, CancellationToken cancellationToken)
    {
        if (mediaTypeAlias is null)
        {
            throw new ArgumentNullException(nameof(mediaTypeAlias));
        }

        if (string.IsNullOrWhiteSpace(mediaTypeAlias))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(mediaTypeAlias));
        }

        scope.ReadLock(Constants.Locks.MediaTypes);

        IMediaType? mediaType = await _mediaTypeRepository.GetAsync(mediaTypeAlias, cancellationToken)
                                ??
                                // causes rollback
                                throw new ArgumentException("No media type with that alias.", nameof(mediaTypeAlias));

        return mediaType;
    }

    #endregion

    #region Others

    /// <inheritdoc />
    public async Task<ContentDataIntegrityReport> CheckDataIntegrityAsync(ContentDataIntegrityReportOptions options, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        scope.WriteLock(Constants.Locks.MediaTree);

        ContentDataIntegrityReport report = await _mediaRepository.CheckDataIntegrityAsync(options, cancellationToken);

        if (report.FixedIssues.Count > 0)
        {
            // The event args needs a media item so we'll make a fake one with enough properties to not cause a null ref
            var root = new Models.Media("root", -1, new MediaType(_shortStringHelper, -1)) { Id = -1, Key = Guid.Empty };
            scope.Notifications.Publish(new MediaTreeChangeNotification(root, TreeChangeTypes.RefreshAll, EventMessagesFactory.Get()));
        }

        scope.Complete();

        return report;
    }

    private async Task AuditAsync(AuditType type, int userId, int objectId, string? message = null, string? parameters = null)
    {
        // A media operation must never fail because its audit metadata could not be attributed - a
        // user id with no matching key (e.g. unknown/imported media) skips the audit entry rather
        // than aborting the caller.
        Attempt<Guid> userKeyAttempt = await _userIdKeyResolver.TryGetAsync(userId);
        if (userKeyAttempt.Success is false)
        {
            _logger.LogWarning("Could not resolve a user key for user id {UserId} - skipping the {AuditType} audit entry for {ObjectId}.", userId, type, objectId);
            return;
        }

        await _auditService.AddAsync(
            type,
            userKeyAttempt.Result,
            objectId,
            UmbracoObjectTypes.Media.GetName(),
            message,
            parameters);
    }

    #endregion

    #region File Management

    /// <inheritdoc />
    public Stream GetMediaFileContentStream(string filepath)
    {
        if (_mediaFileManager.FileSystem.FileExists(filepath) == false)
        {
            return Stream.Null;
        }

        try
        {
            return _mediaFileManager.FileSystem.OpenFile(filepath);
        }
        catch
        {
            return Stream.Null; // deal with race conds
        }
    }

    /// <inheritdoc />
    public void SetMediaFileContent(string filepath, Stream content)
    {
        _mediaFileManager.FileSystem.AddFile(filepath, content, true);
    }

    /// <inheritdoc />
    public void DeleteMediaFile(string filepath)
    {
        _mediaFileManager.FileSystem.DeleteFile(filepath);
    }

    /// <inheritdoc />
    public long GetMediaFileSize(string filepath)
    {
        return _mediaFileManager.FileSystem.GetSize(filepath);
    }

    #endregion
}
