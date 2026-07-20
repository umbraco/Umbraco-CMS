using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for managing dictionary items (localized text entries).
/// </summary>
internal sealed class DictionaryItemService : RepositoryService, IDictionaryItemService
{
    private readonly IDictionaryRepository _dictionaryRepository;
    private readonly IAuditService _auditService;
    private readonly ILanguageService _languageService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryItemService"/> class.
    /// </summary>
    /// <param name="provider">The core scope provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="dictionaryRepository">The dictionary repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="languageService">The language service.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    public DictionaryItemService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDictionaryRepository dictionaryRepository,
        IAuditService auditService,
        ILanguageService languageService,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _dictionaryRepository = dictionaryRepository;
        _auditService = auditService;
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
    /// </summary>
    /// <param name="parentId">The parent dictionary item ID, or null for root items.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <returns>A paged model containing dictionary items.</returns>
    /// <remarks>Currently implements the paging in memory on the item key property because the underlying repository does not support paging yet.</remarks>
    public async Task<PagedModel<IDictionaryItem>> GetPagedAsync(Guid? parentId, int skip, int take)
    {
        using ICoreScope coreScope = ScopeProvider.CreateCoreScope(autoComplete: true);

        if (take == 0)
        {
            return parentId is null
                ? new PagedModel<IDictionaryItem>(await CountRootAsync(), Enumerable.Empty<IDictionaryItem>())
                : new PagedModel<IDictionaryItem>(await CountChildrenAsync(parentId.Value), Enumerable.Empty<IDictionaryItem>());
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
    public async Task<IEnumerable<IDictionaryItem>> GetChildrenAsync(Guid parentId)
        => await GetByQueryAsync(Query<IDictionaryItem>().Where(x => x.ParentId == parentId));

    /// <summary>
    /// Counts the number of child dictionary items under a specified parent.
    /// </summary>
    /// <param name="parentId">The parent dictionary item ID.</param>
    /// <returns>The number of child dictionary items.</returns>
    public async Task<int> CountChildrenAsync(Guid parentId)
        => await CountByQueryAsync(Query<IDictionaryItem>().Where(x => x.ParentId == parentId));

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
    public async Task<IEnumerable<IDictionaryItem>> GetAtRootAsync()
        => await GetByQueryAsync(Query<IDictionaryItem>().Where(x => x.ParentId == null));

    /// <summary>
    /// Counts the number of root dictionary items.
    /// </summary>
    /// <returns>The number of root dictionary items.</returns>
    public async Task<int> CountRootAsync()
        => await CountByQueryAsync(Query<IDictionaryItem>().Where(x => x.ParentId == null));

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
    {
        // Create and update dates aren't tracked for dictionary items. They exist on IDictionaryItem due to the
        // inheritance from IEntity, but we don't store them.
        // However we have logic in ServerEventSender that will provide SignalR events for created and update operations,
        // where these dates are used to distinguish between the two (whether or not the entity has an identity cannot
        // be used here, as these events fire after persistence when the identity is known for both creates and updates).
        // So ensure we set something that can be distinguished here.
        if (dictionaryItem.CreateDate == default)
        {
            // Set such that it's prior to the update date, but not the default date which will be considered
            // uninitialized and get reset to the current date at the repository.
            dictionaryItem.CreateDate = DateTime.MinValue.AddHours(1);
        }

        if (dictionaryItem.UpdateDate == default)
        {
            // TODO (V17): To align with updates of system dates, this needs to change to DateTime.UtcNow.
            dictionaryItem.UpdateDate = DateTime.UtcNow;
        }

        return await SaveAsync(
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
    }

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

            await AuditAsync(AuditType.Delete, "Delete DictionaryItem", userKey, dictionaryItem.Id, nameof(DictionaryItem));

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

            await AuditAsync(AuditType.Move, "Move DictionaryItem", userKey, dictionaryItem.Id, nameof(DictionaryItem));
            scope.Complete();

            return Attempt.SucceedWithStatus(DictionaryItemOperationStatus.Success, dictionaryItem);
        }
    }

    /// <summary>
    /// Counts dictionary items matching the specified query.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <returns>The count of matching dictionary items.</returns>
    private Task<int> CountByQueryAsync(IQuery<IDictionaryItem> query)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            var items = _dictionaryRepository.Count(query);
            return Task.FromResult(items);
        }
    }

    /// <summary>
    /// Saves a dictionary item with validation and notification handling.
    /// </summary>
    /// <param name="dictionaryItem">The dictionary item to save.</param>
    /// <param name="operationValidation">A function that validates the operation and returns the status.</param>
    /// <param name="auditType">The type of audit entry to create.</param>
    /// <param name="auditMessage">The audit message.</param>
    /// <param name="userKey">The unique identifier of the user performing the save.</param>
    /// <returns>An attempt containing the saved dictionary item and operation status.</returns>
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

            await AuditAsync(auditType, auditMessage, userKey, dictionaryItem.Id, nameof(DictionaryItem));
            scope.Complete();

            return Attempt.SucceedWithStatus(DictionaryItemOperationStatus.Success, dictionaryItem);
        }
    }

    /// <summary>
    /// Gets dictionary items matching the specified query.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <returns>A collection of dictionary items matching the query.</returns>
    private Task<IEnumerable<IDictionaryItem>> GetByQueryAsync(IQuery<IDictionaryItem> query)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IDictionaryItem[] items = _dictionaryRepository.Get(query).ToArray();
            return Task.FromResult<IEnumerable<IDictionaryItem>>(items);
        }
    }

    /// <summary>
    /// Creates an audit entry for a dictionary item operation.
    /// </summary>
    /// <param name="type">The type of audit entry.</param>
    /// <param name="message">The audit message.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <param name="objectId">The ID of the affected object.</param>
    /// <param name="entityType">The type of entity being audited.</param>
    private async Task AuditAsync(AuditType type, string message, Guid userKey, int objectId, string? entityType) =>
        await _auditService.AddAsync(
            type,
            userKey,
            objectId,
            entityType,
            message);

    /// <summary>
    /// Validates that the dictionary item has a valid parent.
    /// </summary>
    /// <param name="dictionaryItem">The dictionary item to validate.</param>
    /// <returns><c>true</c> if the parent is valid or no parent is specified; otherwise, <c>false</c>.</returns>
    private bool HasValidParent(IDictionaryItem dictionaryItem)
        => dictionaryItem.ParentId.HasValue == false || _dictionaryRepository.Get(dictionaryItem.ParentId.Value) != null;

    /// <summary>
    /// Removes translations for languages that no longer exist.
    /// </summary>
    /// <param name="dictionaryItem">The dictionary item to clean up.</param>
    /// <param name="allLanguages">All available languages.</param>
    private void RemoveInvalidTranslations(IDictionaryItem dictionaryItem, IEnumerable<ILanguage> allLanguages)
    {
        IDictionaryTranslation[] translationsAsArray = dictionaryItem.Translations.ToArray();
        if (translationsAsArray.Any() == false)
        {
            return;
        }

        var allLanguageIsoCodes = allLanguages.Select(language => language.IsoCode).ToArray();
        dictionaryItem.Translations = translationsAsArray.Where(translation => allLanguageIsoCodes.Contains(translation.LanguageIsoCode)).ToArray();
    }

    /// <summary>
    /// Checks whether another dictionary item exists with the same item key.
    /// </summary>
    /// <param name="dictionaryItem">The dictionary item to check.</param>
    /// <returns><c>true</c> if a collision exists; otherwise, <c>false</c>.</returns>
    private bool HasItemKeyCollision(IDictionaryItem dictionaryItem)
    {
        IDictionaryItem? itemKeyCollision = _dictionaryRepository.Get(dictionaryItem.ItemKey);
        return itemKeyCollision != null && itemKeyCollision.Key != dictionaryItem.Key;
    }
}
