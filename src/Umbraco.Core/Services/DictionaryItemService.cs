﻿using Microsoft.Extensions.Logging;
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

    public DictionaryItemService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDictionaryRepository dictionaryRepository,
        IAuditRepository auditRepository,
        ILanguageService languageService)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _dictionaryRepository = dictionaryRepository;
        _auditRepository = auditRepository;
        _languageService = languageService;
    }

    /// <inheritdoc />
    public async Task<IDictionaryItem?> GetAsync(Guid id)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IDictionaryItem? item = _dictionaryRepository.Get(id);
            return await Task.FromResult(item);
        }
    }

    /// <inheritdoc />
    public async Task<IDictionaryItem?> GetAsync(string key)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IDictionaryItem? item = _dictionaryRepository.Get(key);
            return await Task.FromResult(item);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IDictionaryItem>> GetManyAsync(params Guid[] ids)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IEnumerable<IDictionaryItem> items = _dictionaryRepository.GetMany(ids).ToArray();
            return await Task.FromResult(items);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IDictionaryItem>> GetManyAsync(params string[] keys)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IEnumerable<IDictionaryItem> items = _dictionaryRepository.GetManyByKeys(keys).ToArray();
            return await Task.FromResult(items);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IDictionaryItem>> GetChildrenAsync(Guid parentId)
        => await GetByQueryAsync(Query<IDictionaryItem>().Where(x => x.ParentId == parentId));

    /// <inheritdoc />
    public async Task<IEnumerable<IDictionaryItem>> GetDescendantsAsync(Guid? parentId)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IDictionaryItem[] items = _dictionaryRepository.GetDictionaryItemDescendants(parentId).ToArray();
            return await Task.FromResult(items);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<IDictionaryItem>> GetAtRootAsync()
        => await GetByQueryAsync(Query<IDictionaryItem>().Where(x => x.ParentId == null));

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string key)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IDictionaryItem? item = _dictionaryRepository.Get(key);
            return await Task.FromResult(item != null);
        }
    }

    /// <inheritdoc/>
    public async Task<Attempt<IDictionaryItem, DictionaryItemOperationStatus>> CreateAsync(IDictionaryItem dictionaryItem, int userId = Constants.Security.SuperUserId)
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
            userId);
    }

    /// <inheritdoc />
    public async Task<Attempt<IDictionaryItem, DictionaryItemOperationStatus>> UpdateAsync(
        IDictionaryItem dictionaryItem, int userId = Constants.Security.SuperUserId)
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
            userId);

    /// <inheritdoc />
    public async Task<Attempt<IDictionaryItem?, DictionaryItemOperationStatus>> DeleteAsync(Guid id, int userId = Constants.Security.SuperUserId)
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

            Audit(AuditType.Delete, "Delete DictionaryItem", userId, dictionaryItem.Id, nameof(DictionaryItem));

            scope.Complete();
            return await Task.FromResult(Attempt.SucceedWithStatus<IDictionaryItem?, DictionaryItemOperationStatus>(DictionaryItemOperationStatus.Success, dictionaryItem));
        }
    }

    private async Task<Attempt<IDictionaryItem, DictionaryItemOperationStatus>> SaveAsync(
        IDictionaryItem dictionaryItem,
        Func<DictionaryItemOperationStatus> operationValidation,
        AuditType auditType,
        string auditMessage,
        int userId)
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

            Audit(auditType, auditMessage, userId, dictionaryItem.Id, nameof(DictionaryItem));
            scope.Complete();

            return await Task.FromResult(Attempt.SucceedWithStatus(DictionaryItemOperationStatus.Success, dictionaryItem));
        }
    }

    private async Task<IEnumerable<IDictionaryItem>> GetByQueryAsync(IQuery<IDictionaryItem> query)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IDictionaryItem[] items = _dictionaryRepository.Get(query).ToArray();
            return await Task.FromResult(items);
        }
    }

    private void Audit(AuditType type, string message, int userId, int objectId, string? entityType) =>
        _auditRepository.Save(new AuditItem(objectId, type, userId, entityType, message));

    private bool HasValidParent(IDictionaryItem dictionaryItem)
        => dictionaryItem.ParentId.HasValue == false || _dictionaryRepository.Get(dictionaryItem.ParentId.Value) != null;

    private void RemoveInvalidTranslations(IDictionaryItem dictionaryItem, IEnumerable<ILanguage> allLanguages)
    {
        IDictionaryTranslation[] translationsAsArray = dictionaryItem.Translations.ToArray();
        if (translationsAsArray.Any() == false)
        {
            return;
        }

        var allLanguageIsoCodes = allLanguages.Select(language => language.IsoCode).ToArray();
        dictionaryItem.Translations = translationsAsArray.Where(translation => allLanguageIsoCodes.Contains(translation.IsoCode)).ToArray();
    }

    private bool HasItemKeyCollision(IDictionaryItem dictionaryItem)
    {
        IDictionaryItem? itemKeyCollision = _dictionaryRepository.Get(dictionaryItem.ItemKey);
        return itemKeyCollision != null && itemKeyCollision.Key != dictionaryItem.Key;
    }
}
