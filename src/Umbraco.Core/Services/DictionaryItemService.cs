using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

internal sealed class DictionaryItemService : RepositoryService, IDictionaryItemService
{
    private readonly IDictionaryRepository _dictionaryRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ILanguageService _languageService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    public DictionaryItemService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDictionaryRepository dictionaryRepository,
        IAuditRepository auditRepository,
        ILanguageService languageService,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _dictionaryRepository = dictionaryRepository;
        _auditRepository = auditRepository;
        _languageService = languageService;
        _userIdKeyResolver = userIdKeyResolver;
    }

    /// <inheritdoc />
    public Task<IDictionaryItem?> GetAsync(Guid id)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IDictionaryItem? item = _dictionaryRepository.Get(id);
            return Task.FromResult(item);
        }
    }

    /// <inheritdoc />
    public Task<IDictionaryItem?> GetAsync(string key)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IDictionaryItem? item = _dictionaryRepository.Get(key);
            return Task.FromResult(item);
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<IDictionaryItem>> GetManyAsync(params Guid[] ids)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IEnumerable<IDictionaryItem> items = _dictionaryRepository.GetMany(ids).ToArray();
            return Task.FromResult(items);
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<IDictionaryItem>> GetManyAsync(params string[] keys)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IEnumerable<IDictionaryItem> items = _dictionaryRepository.GetManyByKeys(keys).ToArray();
            return Task.FromResult(items);
        }
    }

    /// <summary>
    /// Gets the dictionary items in a paged manner.
    /// Currently implements the paging in memory on the itenkey property because the underlying repository does not support paging yet
    /// </summary>
    public async Task<PagedModel<IDictionaryItem>> GetPagedAsync(Guid? parentId, int skip, int take)
    {
        using ICoreScope coreScope = ScopeProvider.CreateCoreScope(autoComplete: true);

        if (take == 0)
        {
            return parentId is null
                ? new PagedModel<IDictionaryItem>(await CountRootAsync(), [])
                : new PagedModel<IDictionaryItem>(await CountChildrenAsync(parentId.Value), []);
        }

        IDictionaryItem[] items = (parentId is null
            ? await GetAtRootAsync()
            : await GetChildrenAsync(parentId.Value)).ToArray();

        return new PagedModel<IDictionaryItem>(
            items.Length,
            items.OrderBy(i => i.ItemKey)
                .Skip(skip)
                .Take(take));
    }

    /// <inheritdoc />
    public Task<IEnumerable<IDictionaryItem>> GetChildrenAsync(Guid parentId)
        => Task.FromResult<IEnumerable<IDictionaryItem>>(GetByQueryAsync(Query<IDictionaryItem>().Where(x => x.ParentId == parentId)));

    public Task<int> CountChildrenAsync(Guid parentId)
        => Task.FromResult(CountByQueryAsync(Query<IDictionaryItem>().Where(x => x.ParentId == parentId)));

    /// <inheritdoc />
    public Task<IEnumerable<IDictionaryItem>> GetDescendantsAsync(Guid? parentId, string? filter = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IDictionaryItem[] items = _dictionaryRepository.GetDictionaryItemDescendants(parentId, filter).ToArray();
            return Task.FromResult<IEnumerable<IDictionaryItem>>(items);
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<IDictionaryItem>> GetAtRootAsync()
        => Task.FromResult<IEnumerable<IDictionaryItem>>(GetByQueryAsync(Query<IDictionaryItem>().Where(x => x.ParentId == null)));

    public Task<int> CountRootAsync()
        => Task.FromResult(CountByQueryAsync(Query<IDictionaryItem>().Where(x => x.ParentId == null)));

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(string key)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IDictionaryItem? item = _dictionaryRepository.Get(key);
            return Task.FromResult(item != null);
        }
    }

    /// <inheritdoc/>
    public async Task<Attempt<IDictionaryItem, DictionaryItemOperationStatus>> CreateAsync(IDictionaryItem dictionaryItem, Guid userKey)
    {
        if (dictionaryItem.Id != 0)
        {
            return Attempt.FailWithStatus(DictionaryItemOperationStatus.InvalidId, dictionaryItem);
        }

        return await SaveAsync(
            dictionaryItem,
            () =>
            {
                if (_dictionaryRepository.Get(dictionaryItem.Key) != null)
                {
                    return DictionaryItemOperationStatus.DuplicateKey;
                }

                return DictionaryItemOperationStatus.Success;
            },
            AuditType.New,
            "Create DictionaryItem",
            userKey);
    }

    /// <inheritdoc />
    public async Task<Attempt<IDictionaryItem, DictionaryItemOperationStatus>> UpdateAsync(
        IDictionaryItem dictionaryItem, Guid userKey)
        => await SaveAsync(
            dictionaryItem,
            () =>
            {
                // is there an item to update?
                if (_dictionaryRepository.Exists(dictionaryItem.Id) == false)
                {
                    return DictionaryItemOperationStatus.ItemNotFound;
                }

                return DictionaryItemOperationStatus.Success;
            },
            AuditType.Save,
            "Update DictionaryItem",
            userKey);

    /// <inheritdoc />
    public async Task<Attempt<IDictionaryItem?, DictionaryItemOperationStatus>> DeleteAsync(Guid id, Guid userKey)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IDictionaryItem? dictionaryItem = _dictionaryRepository.Get(id);
            if (dictionaryItem == null)
            {
                return Attempt.FailWithStatus<IDictionaryItem?, DictionaryItemOperationStatus>(DictionaryItemOperationStatus.ItemNotFound, null);
            }

            EventMessages eventMessages = EventMessagesFactory.Get();
            var deletingNotification = new DictionaryItemDeletingNotification(dictionaryItem, eventMessages);
            if (await scope.Notifications.PublishCancelableAsync(deletingNotification))
            {
                scope.Complete();
                return Attempt.FailWithStatus<IDictionaryItem?, DictionaryItemOperationStatus>(DictionaryItemOperationStatus.CancelledByNotification, dictionaryItem);
            }

            _dictionaryRepository.Delete(dictionaryItem);
            scope.Notifications.Publish(
                new DictionaryItemDeletedNotification(dictionaryItem, eventMessages)
                    .WithStateFrom(deletingNotification));

            var currentUserId = await _userIdKeyResolver.GetAsync(userKey);
            Audit(AuditType.Delete, "Delete DictionaryItem", currentUserId, dictionaryItem.Id, nameof(DictionaryItem));

            scope.Complete();
            return Attempt.SucceedWithStatus<IDictionaryItem?, DictionaryItemOperationStatus>(DictionaryItemOperationStatus.Success, dictionaryItem);
        }
    }

    /// <inheritdoc/>
    public async Task<Attempt<IDictionaryItem, DictionaryItemOperationStatus>> MoveAsync(
        IDictionaryItem dictionaryItem,
        Guid? parentId,
        Guid userKey)
    {
        // same parent? then just ignore this operation, assume success.
        if (dictionaryItem.ParentId == parentId)
        {
            return Attempt.SucceedWithStatus(DictionaryItemOperationStatus.Success, dictionaryItem);
        }

        // cannot move a dictionary item underneath itself
        if (dictionaryItem.Key == parentId)
        {
            return Attempt.FailWithStatus(DictionaryItemOperationStatus.InvalidParent, dictionaryItem);
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IDictionaryItem? parent = parentId.HasValue ? _dictionaryRepository.Get(parentId.Value) : null;

            // validate parent if applicable
            if (parentId.HasValue && parent == null)
            {
                return Attempt.FailWithStatus(DictionaryItemOperationStatus.ParentNotFound, dictionaryItem);
            }

            // ensure we don't move a dictionary item underneath one of its own descendants
            if (parent != null)
            {
                IEnumerable<IDictionaryItem> descendants = _dictionaryRepository.GetDictionaryItemDescendants(dictionaryItem.Key);
                if (descendants.Any(item => item.Key == parent.Key))
                {
                    return Attempt.FailWithStatus(DictionaryItemOperationStatus.InvalidParent, dictionaryItem);
                }
            }

            dictionaryItem.ParentId = parentId;

            EventMessages eventMessages = EventMessagesFactory.Get();
            var moveEventInfo = new MoveEventInfo<IDictionaryItem>(dictionaryItem, string.Empty, parent?.Id ?? Constants.System.Root, parentId);
            var movingNotification = new DictionaryItemMovingNotification(moveEventInfo, eventMessages);
            if (await scope.Notifications.PublishCancelableAsync(movingNotification))
            {
                scope.Complete();
                return Attempt.FailWithStatus(DictionaryItemOperationStatus.CancelledByNotification, dictionaryItem);
            }

            _dictionaryRepository.Save(dictionaryItem);

            scope.Notifications.Publish(
                new DictionaryItemMovedNotification(moveEventInfo, eventMessages).WithStateFrom(movingNotification));

            var currentUserId = await _userIdKeyResolver.GetAsync(userKey);
            Audit(AuditType.Move, "Move DictionaryItem", currentUserId, dictionaryItem.Id, nameof(DictionaryItem));
            scope.Complete();

            return Attempt.SucceedWithStatus(DictionaryItemOperationStatus.Success, dictionaryItem);
        }
    }

    private int CountByQueryAsync(IQuery<IDictionaryItem> query)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            var items = _dictionaryRepository.Count(query);
            return items;
        }
    }

    private async Task<Attempt<IDictionaryItem, DictionaryItemOperationStatus>> SaveAsync(
        IDictionaryItem dictionaryItem,
        Func<DictionaryItemOperationStatus> operationValidation,
        AuditType auditType,
        string auditMessage,
        Guid userKey)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            DictionaryItemOperationStatus status = operationValidation();
            if (status != DictionaryItemOperationStatus.Success)
            {
                return Attempt.FailWithStatus(status, dictionaryItem);
            }

            // validate the parent
            if (HasValidParent(dictionaryItem) == false)
            {
                return Attempt.FailWithStatus(DictionaryItemOperationStatus.ParentNotFound, dictionaryItem);
            }

            // do we have an item key collision (item keys must be unique)?
            if (HasItemKeyCollision(dictionaryItem))
            {
                return Attempt.FailWithStatus(DictionaryItemOperationStatus.DuplicateItemKey, dictionaryItem);
            }

            // ensure valid languages for all translations
            ILanguage[] allLanguages = (await _languageService.GetAllAsync()).ToArray();
            RemoveInvalidTranslations(dictionaryItem, allLanguages);

            EventMessages eventMessages = EventMessagesFactory.Get();
            var savingNotification = new DictionaryItemSavingNotification(dictionaryItem, eventMessages);
            if (await scope.Notifications.PublishCancelableAsync(savingNotification))
            {
                scope.Complete();
                return Attempt.FailWithStatus(DictionaryItemOperationStatus.CancelledByNotification, dictionaryItem);
            }

            _dictionaryRepository.Save(dictionaryItem);

            scope.Notifications.Publish(
                new DictionaryItemSavedNotification(dictionaryItem, eventMessages).WithStateFrom(savingNotification));

            var currentUserId = await _userIdKeyResolver.GetAsync(userKey);
            Audit(auditType, auditMessage, currentUserId, dictionaryItem.Id, nameof(DictionaryItem));
            scope.Complete();

            return Attempt.SucceedWithStatus(DictionaryItemOperationStatus.Success, dictionaryItem);
        }
    }

    private IDictionaryItem[] GetByQueryAsync(IQuery<IDictionaryItem> query)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IDictionaryItem[] items = _dictionaryRepository.Get(query).ToArray();
            return items;
        }
    }

    private void Audit(AuditType type, string message, int userId, int objectId, string? entityType) =>
        _auditRepository.Save(new AuditItem(objectId, type, userId, entityType, message));

    private bool HasValidParent(IDictionaryItem dictionaryItem)
        => dictionaryItem.ParentId.HasValue == false || _dictionaryRepository.Get(dictionaryItem.ParentId.Value) != null;

    private static void RemoveInvalidTranslations(IDictionaryItem dictionaryItem, IEnumerable<ILanguage> allLanguages)
    {
        IDictionaryTranslation[] translationsAsArray = dictionaryItem.Translations.ToArray();
        if (translationsAsArray.Any() == false)
        {
            return;
        }

        var allLanguageIsoCodes = allLanguages.Select(language => language.IsoCode).ToArray();
        dictionaryItem.Translations = translationsAsArray.Where(translation => allLanguageIsoCodes.Contains(translation.LanguageIsoCode)).ToArray();
    }

    private bool HasItemKeyCollision(IDictionaryItem dictionaryItem)
    {
        IDictionaryItem? itemKeyCollision = _dictionaryRepository.Get(dictionaryItem.ItemKey);
        return itemKeyCollision != null && itemKeyCollision.Key != dictionaryItem.Key;
    }
}
