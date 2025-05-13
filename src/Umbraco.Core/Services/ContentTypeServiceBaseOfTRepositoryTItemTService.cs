using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public abstract class ContentTypeServiceBase<TRepository, TItem> : ContentTypeServiceBase, IContentTypeBaseService<TItem>
    where TRepository : IContentTypeRepositoryBase<TItem>
    where TItem : class, IContentTypeComposition
{
    private readonly IAuditRepository _auditRepository;
    private readonly IEntityContainerRepository _containerRepository;
    private readonly IEntityRepository _entityRepository;
    private readonly IEventAggregator _eventAggregator;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly ContentTypeFilterCollection _contentTypeFilters;

    protected ContentTypeServiceBase(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        TRepository repository,
        IAuditRepository auditRepository,
        IEntityContainerRepository containerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator,
        IUserIdKeyResolver userIdKeyResolver,
        ContentTypeFilterCollection contentTypeFilters)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        Repository = repository;
        _auditRepository = auditRepository;
        _containerRepository = containerRepository;
        _entityRepository = entityRepository;
        _eventAggregator = eventAggregator;
        _userIdKeyResolver = userIdKeyResolver;
        _contentTypeFilters = contentTypeFilters;
    }

    [Obsolete("Use the ctor specifying all dependencies instead")]
    protected ContentTypeServiceBase(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        TRepository repository,
        IAuditRepository auditRepository,
        IEntityContainerRepository containerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            repository,
            auditRepository,
            containerRepository,
            entityRepository,
            eventAggregator,
            StaticServiceProvider.Instance.GetRequiredService<IUserIdKeyResolver>())
    {
    }

    [Obsolete("Use the ctor specifying all dependencies instead")]
    protected ContentTypeServiceBase(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        TRepository repository,
        IAuditRepository auditRepository,
        IEntityContainerRepository containerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator,
        IUserIdKeyResolver userIdKeyResolver)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            repository,
            auditRepository,
            containerRepository,
            entityRepository,
            eventAggregator,
            userIdKeyResolver,
            StaticServiceProvider.Instance.GetRequiredService<ContentTypeFilterCollection>())
    {
    }

    protected TRepository Repository { get; }
    protected abstract int[] WriteLockIds { get; }
    protected abstract int[] ReadLockIds { get; }

    #region Notifications

    protected abstract SavingNotification<TItem> GetSavingNotification(TItem item, EventMessages eventMessages);
    protected abstract SavingNotification<TItem> GetSavingNotification(IEnumerable<TItem> items, EventMessages eventMessages);

    protected abstract SavedNotification<TItem> GetSavedNotification(TItem item, EventMessages eventMessages);
    protected abstract SavedNotification<TItem> GetSavedNotification(IEnumerable<TItem> items, EventMessages eventMessages);

    protected abstract DeletingNotification<TItem> GetDeletingNotification(TItem item, EventMessages eventMessages);
    protected abstract DeletingNotification<TItem> GetDeletingNotification(IEnumerable<TItem> items, EventMessages eventMessages);

    protected abstract DeletedNotification<TItem> GetDeletedNotification(IEnumerable<TItem> items, EventMessages eventMessages);

    protected abstract MovingNotification<TItem> GetMovingNotification(MoveEventInfo<TItem> moveInfo, EventMessages eventMessages);

    protected abstract MovedNotification<TItem> GetMovedNotification(IEnumerable<MoveEventInfo<TItem>> moveInfo, EventMessages eventMessages);

    protected abstract ContentTypeChangeNotification<TItem> GetContentTypeChangedNotification(IEnumerable<ContentTypeChange<TItem>> changes, EventMessages eventMessages);

    // This notification is identical to GetTypeChangeNotification, however it needs to be a different notification type because it's published within the transaction
    /// The purpose of this notification being published within the transaction is so that listeners can perform database
    /// operations from within the same transaction and guarantee data consistency so that if anything goes wrong
    /// the entire transaction can be rolled back. This is used by Nucache.
    protected abstract ContentTypeRefreshNotification<TItem> GetContentTypeRefreshedNotification(IEnumerable<ContentTypeChange<TItem>> changes, EventMessages eventMessages);

    #endregion

    #region Validation

    public Attempt<string[]?> ValidateComposition(TItem? compo)
    {
        try
        {
            using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds);
                ValidateLocked(compo!);
            }

            return Attempt<string[]?>.Succeed();
        }
        catch (InvalidCompositionException ex)
        {
            return Attempt.Fail(ex.PropertyTypeAliases, ex);
        }
    }

    protected void ValidateLocked(TItem compositionContentType)
    {
        // performs business-level validation of the composition
        // should ensure that it is absolutely safe to save the composition

        // eg maybe a property has been added, with an alias that's OK (no conflict with ancestors)
        // but that cannot be used (conflict with descendants)

        IContentTypeComposition[] allContentTypes = Repository.GetMany(Array.Empty<int>()).Cast<IContentTypeComposition>().ToArray();

        IEnumerable<string> compositionAliases = compositionContentType.CompositionAliases();
        IEnumerable<IContentTypeComposition> compositions = allContentTypes.Where(x => compositionAliases.Any(y => x.Alias.Equals(y)));
        var propertyTypeAliases = compositionContentType.PropertyTypes.Select(x => x.Alias).ToArray();
        var propertyGroupAliases = compositionContentType.PropertyGroups.ToDictionary(x => x.Alias, x => x.Type, StringComparer.InvariantCultureIgnoreCase);
        IEnumerable<IContentTypeComposition> indirectReferences = allContentTypes.Where(x => x.ContentTypeComposition.Any(y => y.Id == compositionContentType.Id));
        var comparer = new DelegateEqualityComparer<IContentTypeComposition>((x, y) => x?.Id == y?.Id, x => x.Id);
        var dependencies = new HashSet<IContentTypeComposition>(compositions, comparer);

        var stack = new Stack<IContentTypeComposition>();
        foreach (IContentTypeComposition indirectReference in indirectReferences)
        {
            stack.Push(indirectReference); // push indirect references to a stack, so we can add recursively
        }

        while (stack.Count > 0)
        {
            IContentTypeComposition indirectReference = stack.Pop();
            dependencies.Add(indirectReference);

            // get all compositions for the current indirect reference
            IEnumerable<IContentTypeComposition> directReferences = indirectReference.ContentTypeComposition;
            foreach (IContentTypeComposition directReference in directReferences)
            {
                if (directReference.Id == compositionContentType.Id || directReference.Alias.Equals(compositionContentType.Alias))
                {
                    continue;
                }

                dependencies.Add(directReference);

                // a direct reference has compositions of its own - these also need to be taken into account
                IEnumerable<string> directReferenceGraph = directReference.CompositionAliases();
                foreach (IContentTypeComposition c in allContentTypes.Where(x => directReferenceGraph.Any(y => x.Alias.Equals(y, StringComparison.InvariantCultureIgnoreCase))))
                {
                    dependencies.Add(c);
                }
            }

            // recursive lookup of indirect references
            foreach (IContentTypeComposition c in allContentTypes.Where(x => x.ContentTypeComposition.Any(y => y.Id == indirectReference.Id)))
            {
                stack.Push(c);
            }
        }

        var duplicatePropertyTypeAliases = new List<string>();
        var invalidPropertyGroupAliases = new List<string>();

        foreach (IContentTypeComposition dependency in dependencies)
        {
            if (dependency.Id == compositionContentType.Id)
            {
                continue;
            }

            IContentTypeComposition? contentTypeDependency = allContentTypes.FirstOrDefault(x => x.Alias.Equals(dependency.Alias, StringComparison.InvariantCultureIgnoreCase));
            if (contentTypeDependency == null)
            {
                continue;
            }

            duplicatePropertyTypeAliases.AddRange(contentTypeDependency.PropertyTypes.Select(x => x.Alias).Intersect(propertyTypeAliases, StringComparer.InvariantCultureIgnoreCase));
            invalidPropertyGroupAliases.AddRange(contentTypeDependency.PropertyGroups.Where(x => propertyGroupAliases.TryGetValue(x.Alias, out PropertyGroupType type) && type != x.Type).Select(x => x.Alias));
        }

        if (duplicatePropertyTypeAliases.Count > 0 || invalidPropertyGroupAliases.Count > 0)

        {
            throw new InvalidCompositionException(compositionContentType.Alias, null, duplicatePropertyTypeAliases.Distinct().ToArray(), invalidPropertyGroupAliases.Distinct().ToArray());
        }
    }

    #endregion

    #region Composition

    internal IEnumerable<ContentTypeChange<TItem>> ComposeContentTypeChanges(params TItem[] contentTypes)
    {
        // find all content types impacted by the changes,
        // - content type alias changed
        // - content type property removed, or alias changed
        // - content type composition removed (not testing if composition had properties...)
        // - content type variation changed
        // - property type variation changed
        //
        // because these are the changes that would impact the raw content data

        // note
        // this is meant to run *after* uow.Commit() so must use WasPropertyDirty() everywhere
        // instead of IsPropertyDirty() since dirty properties have been reset already

        var changes = new List<ContentTypeChange<TItem>>();

        foreach (TItem contentType in contentTypes)
        {
            var dirty = (IRememberBeingDirty)contentType;

            // skip new content types
            // TODO: This used to be WasPropertyDirty("HasIdentity") but i don't think that actually worked for detecting new entities this does seem to work properly
            var isNewContentType = dirty.WasPropertyDirty("Id");
            if (isNewContentType)
            {
                AddChange(changes, contentType, ContentTypeChangeTypes.Create);
                continue;
            }

            // alias change?
            var hasAliasChanged = dirty.WasPropertyDirty("Alias");

            // existing property alias change?
            var hasAnyPropertyChangedAlias = contentType.PropertyTypes.Any(propertyType =>
            {
                // skip new properties
                // TODO: This used to be WasPropertyDirty("HasIdentity") but i don't think that actually worked for detecting new entities this does seem to work properly
                var isNewProperty = propertyType.WasPropertyDirty("Id");
                if (isNewProperty)
                {
                    return false;
                }

                // alias change?
                return propertyType.WasPropertyDirty("Alias");
            });

            // removed properties?
            var hasAnyPropertyBeenRemoved = dirty.WasPropertyDirty("HasPropertyTypeBeenRemoved");

            // removed compositions?
            var hasAnyCompositionBeenRemoved = dirty.WasPropertyDirty("HasCompositionTypeBeenRemoved");

            // variation changed?
            var hasContentTypeVariationChanged = dirty.WasPropertyDirty("Variations");

            // property variation change?
            var hasAnyPropertyVariationChanged = contentType.WasPropertyTypeVariationChanged();

            // main impact on properties?
            var hasPropertyMainImpact = hasContentTypeVariationChanged || hasAnyPropertyVariationChanged
                                                                       || hasAnyCompositionBeenRemoved || hasAnyPropertyBeenRemoved || hasAnyPropertyChangedAlias;

            if (hasAliasChanged || hasPropertyMainImpact)
            {
                // add that one, as a main change
                AddChange(changes, contentType, ContentTypeChangeTypes.RefreshMain);

                if (hasPropertyMainImpact)
                {
                    foreach (TItem c in GetComposedOf(contentType.Id))
                    {
                        AddChange(changes, c, ContentTypeChangeTypes.RefreshMain);
                    }
                }
            }
            else
            {
                // add that one, as an other change
                AddChange(changes, contentType, ContentTypeChangeTypes.RefreshOther);
            }
        }

        return changes;
    }

    // ensures changes contains no duplicates
    private static void AddChange(ICollection<ContentTypeChange<TItem>> changes, TItem contentType, ContentTypeChangeTypes changeTypes)
    {
        ContentTypeChange<TItem>? change = changes.FirstOrDefault(x => x.Item == contentType);
        if (change == null)
        {
            changes.Add(new ContentTypeChange<TItem>(contentType, changeTypes));
            return;
        }

        change.ChangeTypes |= changeTypes;
    }

    #endregion

    #region Get, Has, Is, Count

    IContentTypeComposition? IContentTypeBaseService.Get(int id)
    {
        return Get(id);
    }

    public TItem? Get(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return Repository.Get(id);
    }

    public TItem? Get(string alias)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return Repository.Get(alias);
    }

    public TItem? Get(Guid id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return Repository.Get(id);
    }

    /// <inheritdoc />
    public Task<TItem?> GetAsync(Guid guid) => Task.FromResult(Get(guid));

    public IEnumerable<TItem> GetAll()
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return Repository.GetMany(Array.Empty<Guid>());
    }

    public IEnumerable<TItem> GetMany(params int[] ids)
    {
        if (ids.Any() is false)
        {
            return Enumerable.Empty<TItem>();
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return Repository.GetMany(ids);
    }

    public IEnumerable<TItem> GetMany(IEnumerable<Guid>? ids)
    {
        if (ids is null || ids.Any() is false)
        {
            return Enumerable.Empty<TItem>();
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(ReadLockIds);
            return Repository.GetMany(ids.ToArray());
        }
    }

    public IEnumerable<TItem> GetChildren(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        IQuery<TItem> query = Query<TItem>().Where(x => x.ParentId == id);
        return Repository.Get(query);
    }

    public IEnumerable<TItem> GetChildren(Guid id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        TItem? found = Get(id);
        if (found == null)
        {
            return Enumerable.Empty<TItem>();
        }

        IQuery<TItem> query = Query<TItem>().Where(x => x.ParentId == found.Id);
        return Repository.Get(query);
    }

    public bool HasChildren(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        IQuery<TItem> query = Query<TItem>().Where(x => x.ParentId == id);
        var count = Repository.Count(query);
        return count > 0;
    }

    public bool HasChildren(Guid id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            scope.ReadLock(ReadLockIds);
            TItem? found = Get(id);
            if (found == null)
            {
                return false;
            }

            IQuery<TItem> query = Query<TItem>().Where(x => x.ParentId == found.Id);
            var count = Repository.Count(query);
            return count > 0;
        }
    }

    /// <summary>
    /// Given the path of a content item, this will return true if the content item exists underneath a list view content item
    /// </summary>
    /// <param name="contentPath"></param>
    /// <returns></returns>
    public bool HasContainerInPath(string contentPath)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        // can use same repo for both content and media
        return Repository.HasContainerInPath(contentPath);
    }

    public bool HasContainerInPath(params int[] ids)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        // can use same repo for both content and media
        return Repository.HasContainerInPath(ids);
    }

    public IEnumerable<TItem> GetDescendants(int id, bool andSelf)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);

        var descendants = new List<TItem>();
        if (andSelf)
        {
            TItem? self = Repository.Get(id);
            if (self is not null)
            {
                descendants.Add(self);
            }
        }

        var ids = new Stack<int>();
        ids.Push(id);

        while (ids.Count > 0)
        {
            var i = ids.Pop();
            IQuery<TItem> query = Query<TItem>().Where(x => x.ParentId == i);
            TItem[]? result = Repository.Get(query).ToArray();

            if (result is not null)
            {
                foreach (TItem c in result)
                {
                    descendants.Add(c);
                    ids.Push(c.Id);
                }
            }
        }

        return descendants.ToArray();
    }

    public IEnumerable<TItem> GetComposedOf(int id, IEnumerable<TItem> all) =>
        all.Where(x => x.ContentTypeComposition.Any(y => y.Id == id));

    public IEnumerable<TItem> GetComposedOf(int id)
    {
        // GetAll is cheap, repository has a full dataset cache policy
        // TODO: still, because it uses the cache, race conditions!
        IEnumerable<TItem> allContentTypes = GetAll();
        return GetComposedOf(id, allContentTypes);
    }

    public int Count()
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return Repository.Count(Query<TItem>());
    }

    public bool HasContentNodes(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return Repository.HasContentNodes(id);
    }

    #endregion

    #region Save

    public async Task SaveAsync(TItem item, Guid performingUserKey)
    {
        var userId = await _userIdKeyResolver.GetAsync(performingUserKey);
        Save(item, userId);
    }

    public void Save(TItem? item, int userId = Constants.Security.SuperUserId)
    {
        if (item is null)
        {
            return;
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        EventMessages eventMessages = EventMessagesFactory.Get();
        SavingNotification<TItem> savingNotification = GetSavingNotification(item, eventMessages);
        if (scope.Notifications.PublishCancelable(savingNotification))
        {
            scope.Complete();
            return;
        }

        if (string.IsNullOrWhiteSpace(item.Name))
        {
            throw new ArgumentException("Cannot save item with empty name.");
        }

        if (item.Name != null && item.Name.Length > 255)
        {
            throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
        }

        scope.WriteLock(WriteLockIds);

        // validate the DAG transform, within the lock
        ValidateLocked(item); // throws if invalid

        item.CreatorId = userId;
        if (item.Description == string.Empty)
        {
            item.Description = null;
        }

        Repository.Save(item); // also updates content/media/member items

        // figure out impacted content types
        ContentTypeChange<TItem>[] changes = ComposeContentTypeChanges(item).ToArray();

        // Publish this in scope, see comment at GetContentTypeRefreshedNotification for more info.
        _eventAggregator.Publish(GetContentTypeRefreshedNotification(changes, eventMessages));

        scope.Notifications.Publish(GetContentTypeChangedNotification(changes, eventMessages));

        SavedNotification<TItem> savedNotification = GetSavedNotification(item, eventMessages);
        savedNotification.WithStateFrom(savingNotification);
        scope.Notifications.Publish(savedNotification);

        Audit(AuditType.Save, userId, item.Id);
        scope.Complete();
    }

    public void Save(IEnumerable<TItem> items, int userId = Constants.Security.SuperUserId)
    {
        TItem[] itemsA = items.ToArray();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            EventMessages eventMessages = EventMessagesFactory.Get();
            SavingNotification<TItem> savingNotification = GetSavingNotification(itemsA, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return;
            }

            scope.WriteLock(WriteLockIds);

            // all-or-nothing, validate them all first
            foreach (TItem contentType in itemsA)
            {
                ValidateLocked(contentType); // throws if invalid
            }
            foreach (TItem contentType in itemsA)
            {
                contentType.CreatorId = userId;
                if (contentType.Description == string.Empty)
                {
                    contentType.Description = null;
                }

                Repository.Save(contentType);
            }

            // figure out impacted content types
            ContentTypeChange<TItem>[] changes = ComposeContentTypeChanges(itemsA).ToArray();

            // Publish this in scope, see comment at GetContentTypeRefreshedNotification for more info.
            _eventAggregator.Publish(GetContentTypeRefreshedNotification(changes, eventMessages));

            scope.Notifications.Publish(GetContentTypeChangedNotification(changes, eventMessages));

            SavedNotification<TItem> savedNotification = GetSavedNotification(itemsA, eventMessages);
            savedNotification.WithStateFrom(savingNotification);
            scope.Notifications.Publish(savedNotification);

            Audit(AuditType.Save, userId, -1);
            scope.Complete();
        }
    }

    public async Task<Attempt<ContentTypeOperationStatus>> CreateAsync(TItem item, Guid performingUserKey) => await InternalSaveAsync(item, performingUserKey);

    public async Task<Attempt<ContentTypeOperationStatus>> UpdateAsync(TItem item, Guid performingUserKey) => await InternalSaveAsync(item, performingUserKey);

    private async Task<Attempt<ContentTypeOperationStatus>> InternalSaveAsync(TItem item, Guid performingUserKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        EventMessages eventMessages = EventMessagesFactory.Get();

        Attempt<ContentTypeOperationStatus> validationAttempt = ValidateCommon(item);
        if (validationAttempt.Success is false)
        {
            return Attempt.Fail(validationAttempt.Result);
        }

        SavingNotification<TItem> savingNotification = GetSavingNotification(item, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return Attempt.Fail(ContentTypeOperationStatus.CancelledByNotification);
        }

        scope.WriteLock(WriteLockIds);

        // validate the DAG transform, within the lock
        ValidateLocked(item); // throws if invalid

        int userId = await _userIdKeyResolver.GetAsync(performingUserKey);
        item.CreatorId = userId;
        if (item.Description == string.Empty)
        {
            item.Description = null;
        }

        Repository.Save(item); // also updates content/media/member items

        // figure out impacted content types
        ContentTypeChange<TItem>[] changes = ComposeContentTypeChanges(item).ToArray();

        // Publish this in scope, see comment at GetContentTypeRefreshedNotification for more info.
        await _eventAggregator.PublishAsync(GetContentTypeRefreshedNotification(changes, eventMessages));

        scope.Notifications.Publish(GetContentTypeChangedNotification(changes, eventMessages));

        SavedNotification<TItem> savedNotification = GetSavedNotification(item, eventMessages);
        savedNotification.WithStateFrom(savingNotification);
        scope.Notifications.Publish(savedNotification);

        Audit(AuditType.Save, userId, item.Id);
        scope.Complete();

        return Attempt.Succeed(ContentTypeOperationStatus.Success);
    }

    private Attempt<ContentTypeOperationStatus> ValidateCommon(TItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Name))
        {
            return Attempt.Fail(ContentTypeOperationStatus.NameCannotBeEmpty);
        }

        if (item.Name.Length > 255)
        {
            return Attempt.Fail(ContentTypeOperationStatus.NameTooLong);
        }

        return Attempt.Succeed(ContentTypeOperationStatus.Success);
    }

    #endregion

    #region Delete

    public async Task<ContentTypeOperationStatus> DeleteAsync(Guid key, Guid performingUserKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        int performingUserId = await _userIdKeyResolver.GetAsync(performingUserKey);

        TItem? item = await GetAsync(key);

        if (item is null)
        {
            return ContentTypeOperationStatus.NotFound;
        }

        if (CanDelete(item) is false)
        {
            return ContentTypeOperationStatus.NotAllowed;
        }

        Delete(item, performingUserId);

        scope.Complete();
        return ContentTypeOperationStatus.Success;
    }

    public void Delete(TItem item, int userId = Constants.Security.SuperUserId)
    {
        if (CanDelete(item) is false)
        {
            throw new InvalidOperationException("The item was not allowed to be deleted");
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            EventMessages eventMessages = EventMessagesFactory.Get();
            DeletingNotification<TItem> deletingNotification = GetDeletingNotification(item, eventMessages);
            if (scope.Notifications.PublishCancelable(deletingNotification))
            {
                scope.Complete();
                return;
            }

            scope.WriteLock(WriteLockIds);

            // all descendants are going to be deleted
            TItem[] descendantsAndSelf = GetDescendants(item.Id, true)
                .ToArray();
            TItem[] deleted = descendantsAndSelf;

            // all impacted (through composition) probably lose some properties
            // don't try to be too clever here, just report them all
            // do this before anything is deleted
            TItem[] changed = descendantsAndSelf.SelectMany(xx => GetComposedOf(xx.Id))
                .Distinct()
                .Except(descendantsAndSelf)
                .ToArray();

            // delete content
            DeleteItemsOfTypes(descendantsAndSelf.Select(x => x.Id));

            // Next find all other document types that have a reference to this content type
            IEnumerable<TItem> referenceToAllowedContentTypes = GetAll().Where(q => q.AllowedContentTypes?.Any(p => p.Key == item.Key) ?? false);
            foreach (TItem reference in referenceToAllowedContentTypes)
            {
                reference.AllowedContentTypes = reference.AllowedContentTypes?.Where(p => p.Key != item.Key);
                var changedRef = new List<ContentTypeChange<TItem>>() { new ContentTypeChange<TItem>(reference, ContentTypeChangeTypes.RefreshMain) };
                // Fire change event
                scope.Notifications.Publish(GetContentTypeChangedNotification(changedRef, eventMessages));
            }

            // finally delete the content type
            // - recursively deletes all descendants
            // - deletes all associated property data
            //  (contents of any descendant type have been deleted but
            //   contents of any composed (impacted) type remain but
            //   need to have their property data cleared)
            Repository.Delete(item);

            ContentTypeChange<TItem>[] changes = descendantsAndSelf.Select(x => new ContentTypeChange<TItem>(x, ContentTypeChangeTypes.Remove))
                .Concat(changed.Select(x => new ContentTypeChange<TItem>(x, ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther)))
                .ToArray();

            // Publish this in scope, see comment at GetContentTypeRefreshedNotification for more info.
            _eventAggregator.Publish(GetContentTypeRefreshedNotification(changes, eventMessages));

            scope.Notifications.Publish(GetContentTypeChangedNotification(changes, eventMessages));

            DeletedNotification<TItem> deletedNotification = GetDeletedNotification(deleted.DistinctBy(x => x.Id), eventMessages);
            deletedNotification.WithStateFrom(deletingNotification);
            scope.Notifications.Publish(deletedNotification);

            Audit(AuditType.Delete, userId, item.Id);
            scope.Complete();
        }
    }

    public void Delete(IEnumerable<TItem> items, int userId = Constants.Security.SuperUserId)
    {
        TItem[] itemsA = items.ToArray();
        if (itemsA.All(CanDelete) is false)
        {
            throw new InvalidOperationException("One or more items were not allowed to be deleted");
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            EventMessages eventMessages = EventMessagesFactory.Get();
            DeletingNotification<TItem> deletingNotification = GetDeletingNotification(itemsA, eventMessages);
            if (scope.Notifications.PublishCancelable(deletingNotification))
            {
                scope.Complete();
                return;
            }

            scope.WriteLock(WriteLockIds);

            // all descendants are going to be deleted
            TItem[] allDescendantsAndSelf = itemsA.SelectMany(xx => GetDescendants(xx.Id, true)).DistinctBy(x => x.Id).ToArray();
            TItem[] deleted = allDescendantsAndSelf;

            // all impacted (through composition) probably lose some properties
            // don't try to be too clever here, just report them all
            // do this before anything is deleted
            TItem[] changed = allDescendantsAndSelf.SelectMany(x => GetComposedOf(x.Id))
                .Distinct()
                .Except(allDescendantsAndSelf)
                .ToArray();

            // delete content
            DeleteItemsOfTypes(allDescendantsAndSelf.Select(x => x.Id));

            // finally delete the content types
            // (see notes in overload)
            foreach (TItem item in itemsA)
            {
                Repository.Delete(item);
            }

            ContentTypeChange<TItem>[] changes = allDescendantsAndSelf.Select(x => new ContentTypeChange<TItem>(x, ContentTypeChangeTypes.Remove))
                .Concat(changed.Select(x => new ContentTypeChange<TItem>(x, ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther)))
                .ToArray();

            // Publish this in scope, see comment at GetContentTypeRefreshedNotification for more info.
            _eventAggregator.Publish(GetContentTypeRefreshedNotification(changes, eventMessages));

            scope.Notifications.Publish(GetContentTypeChangedNotification(changes, eventMessages));

            DeletedNotification<TItem> deletedNotification = GetDeletedNotification(deleted.DistinctBy(x => x.Id), eventMessages);
            deletedNotification.WithStateFrom(deletingNotification);
            scope.Notifications.Publish(deletedNotification);

            Audit(AuditType.Delete, userId, -1);
            scope.Complete();
        }
    }

    protected abstract void DeleteItemsOfTypes(IEnumerable<int> typeIds);

    protected virtual bool CanDelete(TItem item) => true;

    #endregion

    #region Copy

    [Obsolete("Please use CopyAsync. Will be removed in V15.")]
    public TItem Copy(TItem original, string alias, string name, int parentId = -1)
    {
        TItem? parent = null;
        if (parentId > 0)
        {
            parent = Get(parentId);
            if (parent == null)
            {
                throw new InvalidOperationException("Could not find parent with id " + parentId);
            }
        }
        return Copy(original, alias, name, parent);
    }

    [Obsolete("Please use CopyAsync. Will be removed in V15.")]
    public TItem Copy(TItem original, string alias, string name, TItem? parent)
    {
        if (original == null)
        {
            throw new ArgumentNullException(nameof(original));
        }

        if (alias == null)
        {
            throw new ArgumentNullException(nameof(alias));
        }

        if (string.IsNullOrWhiteSpace(alias))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(alias));
        }

        if (parent != null && parent.HasIdentity == false)
        {
            throw new InvalidOperationException("Parent must have an identity.");
        }

        // this is illegal
        //var originalb = (ContentTypeCompositionBase)original;
        // but we *know* it has to be a ContentTypeCompositionBase anyways
        var originalb = (ContentTypeCompositionBase) (object) original;
        var clone = (TItem) (object) originalb.DeepCloneWithResetIdentities(alias);

        clone.Name = name;

        //remove all composition that is not it's current alias
        var compositionAliases = clone.CompositionAliases().Except(new[] { alias }).ToList();
        foreach (var a in CollectionsMarshal.AsSpan(compositionAliases))
        {
            clone.RemoveContentType(a);
        }

        //if a parent is specified set it's composition and parent
        if (parent != null)
        {
            //add a new parent composition
            clone.AddContentType(parent);
            clone.ParentId = parent.Id;
        }
        else
        {
            //set to root
            clone.ParentId = -1;
        }

        Save(clone);
        return clone;
    }

    [Obsolete("Please use CopyAsync. Will be removed in V16.")]
    public Attempt<OperationResult<MoveOperationStatusType, TItem>?> Copy(TItem copying, int containerId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        TItem copy;
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(WriteLockIds);

            try
            {
                if (containerId > 0)
                {
                    EntityContainer? container = _containerRepository?.Get(containerId);
                    if (container == null)
                    {
                        throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound); // causes rollback
                    }
                }

                var alias = Repository.GetUniqueAlias(copying.Alias);

                // this is illegal
                //var copyingb = (ContentTypeCompositionBase) copying;
                // but we *know* it has to be a ContentTypeCompositionBase anyways

                // TODO: Fix back to only calling the copyingb.DeepCloneWithResetIdentities when
                // when ContentTypeBase.DeepCloneWithResetIdentities is overrideable.
                if (copying is IMediaType mediaTypeToCope)
                {
                    copy = (TItem)mediaTypeToCope.DeepCloneWithResetIdentities(alias);
                }
                else
                {
                    var copyingb = (ContentTypeCompositionBase) (object)copying;
                    copy = (TItem) (object) copyingb.DeepCloneWithResetIdentities(alias);
                }

                copy.Name = copy.Name + " (copy)"; // might not be unique

                // if it has a parent, and the parent is a content type, unplug composition
                // all other compositions remain in place in the copied content type
                if (copy.ParentId > 0)
                {
                    TItem? parent = Repository.Get(copy.ParentId);
                    if (parent != null)
                    {
                        copy.RemoveContentType(parent.Alias);
                    }
                }

                copy.ParentId = containerId;

                SavingNotification<TItem> savingNotification = GetSavingNotification(copy, eventMessages);
                if (scope.Notifications.PublishCancelable(savingNotification))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Fail(MoveOperationStatusType.FailedCancelledByEvent, eventMessages, copy);
                }

                Repository.Save(copy);

                ContentTypeChange<TItem>[] changes = ComposeContentTypeChanges(copy).ToArray();

                _eventAggregator.Publish(GetContentTypeRefreshedNotification(changes, eventMessages));
                scope.Notifications.Publish(GetContentTypeChangedNotification(changes, eventMessages));

                SavedNotification<TItem> savedNotification = GetSavedNotification(copy, eventMessages);
                savedNotification.WithStateFrom(savingNotification);
                scope.Notifications.Publish(savedNotification);

                scope.Complete();
            }
            catch (DataOperationException<MoveOperationStatusType> ex)
            {
                return OperationResult.Attempt.Fail<MoveOperationStatusType, TItem>(ex.Operation, eventMessages); // causes rollback
            }
        }

        return OperationResult.Attempt.Succeed(MoveOperationStatusType.Success, eventMessages, copy);
    }

    public async Task<Attempt<TItem?, ContentTypeStructureOperationStatus>> CopyAsync(Guid key, Guid? containerKey)
    {
        TItem? toCopy = await GetAsync(key);
        if (toCopy is null)
        {
            return Attempt.FailWithStatus(ContentTypeStructureOperationStatus.NotFound, toCopy);
        }

        var containerId = GetContainerOrRootId(containerKey);

        if (containerId is null)
        {
            return Attempt.FailWithStatus<TItem?, ContentTypeStructureOperationStatus>(ContentTypeStructureOperationStatus.ContainerNotFound, toCopy);
        }

        // using obsolete method for version control while it still exists
        Attempt<OperationResult<MoveOperationStatusType, TItem>?> result = Copy(toCopy, containerId.Value);

        return MapStatusTypeToAttempt(result.Result?.Entity, result.Result?.Result);
    }

    private int? GetContainerOrRootId(Guid? containerKey)
    {
        if (containerKey is null)
        {
            return Constants.System.Root;
        }

        EntityContainer? container = GetContainer(containerKey.Value);
        return container?.Id;
    }

    private Attempt<TItem?, ContentTypeStructureOperationStatus> MapStatusTypeToAttempt(TItem? item, MoveOperationStatusType? resultStatus) =>
        resultStatus switch
        {
            MoveOperationStatusType.Success => Attempt.SucceedWithStatus(ContentTypeStructureOperationStatus.Success, item),
            MoveOperationStatusType.FailedParentNotFound => Attempt.FailWithStatus(ContentTypeStructureOperationStatus.ContainerNotFound, item),
            MoveOperationStatusType.FailedCancelledByEvent => Attempt.FailWithStatus(ContentTypeStructureOperationStatus.CancelledByNotification, item),
            MoveOperationStatusType.FailedNotAllowedByPath => Attempt.FailWithStatus(ContentTypeStructureOperationStatus.NotAllowedByPath, item),
            _ => throw new NotImplementedException($"{nameof(ContentTypeStructureOperationStatus)} does not map to a corresponding {nameof(MoveOperationStatusType)}")
        };

    #endregion

    #region Move

    [Obsolete("Please use MoveAsync. Will be removed in V16.")]
    public Attempt<OperationResult<MoveOperationStatusType>?> Move(TItem moving, int containerId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();
        if(moving.ParentId == containerId)
        {
            return OperationResult.Attempt.Fail(MoveOperationStatusType.FailedNotAllowedByPath, eventMessages);
        }

        var moveInfo = new List<MoveEventInfo<TItem>>();
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var moveEventInfo = new MoveEventInfo<TItem>(moving, moving.Path, containerId);
            MovingNotification<TItem> movingNotification = GetMovingNotification(moveEventInfo, eventMessages);
            if (scope.Notifications.PublishCancelable(movingNotification))
            {
                scope.Complete();
                return OperationResult.Attempt.Fail(MoveOperationStatusType.FailedCancelledByEvent, eventMessages);
            }

            scope.WriteLock(WriteLockIds); // also for containers

            try
            {
                EntityContainer? container = null;
                if (containerId > 0)
                {
                    container = _containerRepository?.Get(containerId);
                    if (container == null)
                    {
                        throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound); // causes rollback
                    }
                }
                moveInfo.AddRange(Repository.Move(moving, container!));
                scope.Complete();
            }
            catch (DataOperationException<MoveOperationStatusType> ex)
            {
                scope.Complete();
                return OperationResult.Attempt.Fail(ex.Operation, eventMessages);
            }

            // note: not raising any Changed event here because moving a content type under another container
            // has no impact on the published content types - would be entirely different if we were to support
            // moving a content type under another content type.
            MovedNotification<TItem> movedNotification = GetMovedNotification(moveInfo, eventMessages);
            movedNotification.WithStateFrom(movingNotification);
            scope.Notifications.Publish(movedNotification);
        }

        return OperationResult.Attempt.Succeed(MoveOperationStatusType.Success, eventMessages);
    }

    public async Task<Attempt<TItem?, ContentTypeStructureOperationStatus>> MoveAsync(Guid key, Guid? containerKey)
    {
        TItem? toMove = await GetAsync(key);
        if (toMove is null)
        {
            return Attempt.FailWithStatus(ContentTypeStructureOperationStatus.NotFound, toMove);
        }

        var containerId = GetContainerOrRootId(containerKey);

        if (containerId is null)
        {
            return Attempt.FailWithStatus<TItem?, ContentTypeStructureOperationStatus>(ContentTypeStructureOperationStatus.ContainerNotFound, toMove);
        }

        // using obsolete method for version control while it still exists
        Attempt<OperationResult<MoveOperationStatusType>?> result = Move(toMove, containerId.Value);

        return MapStatusTypeToAttempt(toMove, result.Result?.Result);
    }

    #endregion

    #region Allowed types

    /// <inheritdoc />
    public async Task<PagedModel<TItem>> GetAllAllowedAsRootAsync(int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        // that one is special because it works across content, media and member types
        scope.ReadLock(Constants.Locks.ContentTypes, Constants.Locks.MediaTypes, Constants.Locks.MemberTypes);

        IQuery<TItem> query = ScopeProvider.CreateQuery<TItem>().Where(x => x.AllowedAsRoot);
        IEnumerable<TItem> contentTypes = Repository.Get(query).ToArray();

        foreach (IContentTypeFilter filter in _contentTypeFilters)
        {
            contentTypes = await filter.FilterAllowedAtRootAsync(contentTypes);
        }

        var pagedModel = new PagedModel<TItem>
        {
            Total = contentTypes.Count(),
            Items = contentTypes.Skip(skip).Take(take)
        };

        return pagedModel;
    }


    /// <inheritdoc />
    public async Task<Attempt<PagedModel<TItem>?, ContentTypeOperationStatus>> GetAllowedChildrenAsync(Guid key, int skip, int take)
        => await GetAllowedChildrenAsync(key, null, skip, take);

    /// <inheritdoc />
    public async Task<Attempt<PagedModel<TItem>?, ContentTypeOperationStatus>> GetAllowedChildrenAsync(Guid key, Guid? parentContentKey, int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        TItem? parent = Get(key);

        if (parent?.AllowedContentTypes is null)
        {
            return Attempt.FailWithStatus<PagedModel<TItem>?, ContentTypeOperationStatus>(ContentTypeOperationStatus.NotFound, null);
        }

        IEnumerable<ContentTypeSort> allowedContentTypes = parent.AllowedContentTypes;
        foreach (IContentTypeFilter filter in _contentTypeFilters)
        {
            allowedContentTypes = await filter.FilterAllowedChildrenAsync(allowedContentTypes, key, parentContentKey);
        }

        PagedModel<TItem> result;
        if (allowedContentTypes.Any() is false)
        {
            // no content types allowed under parent
            result = new PagedModel<TItem>
            {
                Items = Array.Empty<TItem>(),
                Total = 0,
            };
        }
        else
        {
            // Get the sorted keys. Whilst we can't guarantee the order that comes back from GetMany, we can use
            // this to sort the resulting list of allowed children.
            Guid[] sortedKeys = allowedContentTypes.OrderBy(x => x.SortOrder).Select(x => x.Key).ToArray();

            TItem[] allowedChildren = GetMany(sortedKeys).ToArray();
            result = new PagedModel<TItem>
            {
                Items = allowedChildren.OrderBy(x => sortedKeys.IndexOf(x.Key)).Take(take).Skip(skip),
                Total = allowedChildren.Length,
            };
        }

        return Attempt.SucceedWithStatus<PagedModel<TItem>?, ContentTypeOperationStatus>(ContentTypeOperationStatus.Success, result);
    }

    #endregion

    #region Containers

    protected abstract Guid ContainedObjectType { get; }

    protected Guid ContainerObjectType => EntityContainer.GetContainerObjectType(ContainedObjectType);

    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Will be removed in V16.")]
    public Attempt<OperationResult<OperationResultType, EntityContainer>?> CreateContainer(int parentId, Guid key, string name, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(WriteLockIds); // also for containers

        try
        {
            var container = new EntityContainer(ContainedObjectType)
            {
                Name = name,
                ParentId = parentId,
                CreatorId = userId,
                Key = key
            };

            var savingNotification = new EntityContainerSavingNotification(container, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return OperationResult.Attempt.Cancel(eventMessages, container);
            }

            _containerRepository?.Save(container);
            scope.Complete();

            var savedNotification = new EntityContainerSavedNotification(container, eventMessages);
            savedNotification.WithStateFrom(savingNotification);
            scope.Notifications.Publish(savedNotification);
            // TODO: Audit trail ?

            return OperationResult.Attempt.Succeed(eventMessages, container);
        }
        catch (Exception ex)
        {
            scope.Complete();
            return OperationResult.Attempt.Fail<OperationResultType, EntityContainer>(OperationResultType.FailedCancelledByEvent, eventMessages, ex);
        }
    }

    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Will be removed in V16.")]
    public Attempt<OperationResult?> SaveContainer(EntityContainer container, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        Guid containerObjectType = ContainerObjectType;
        if (container.ContainerObjectType != containerObjectType)
        {
            var ex = new InvalidOperationException("Not a container of the proper type.");
            return OperationResult.Attempt.Fail(eventMessages, ex);
        }

        if (container.HasIdentity && container.IsPropertyDirty("ParentId"))
        {
            var ex = new InvalidOperationException("Cannot save a container with a modified parent, move the container instead.");
            return OperationResult.Attempt.Fail(eventMessages, ex);
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var savingNotification = new EntityContainerSavingNotification(container, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return OperationResult.Attempt.Cancel(eventMessages);
            }

            scope.WriteLock(WriteLockIds); // also for containers

            _containerRepository?.Save(container);
            scope.Complete();

            var savedNotification = new EntityContainerSavedNotification(container, eventMessages);
            savedNotification.WithStateFrom(savingNotification);
            scope.Notifications.Publish(savedNotification);
        }

        // TODO: Audit trail ?

        return OperationResult.Attempt.Succeed(eventMessages);
    }

    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Will be removed in V16.")]
    public EntityContainer? GetContainer(int containerId)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds); // also for containers

        return _containerRepository.Get(containerId);
    }

    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Will be removed in V16.")]
    public EntityContainer? GetContainer(Guid containerId)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds); // also for containers

        return _containerRepository.Get(containerId);
    }

    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Will be removed in V16.")]
    public IEnumerable<EntityContainer> GetContainers(int[] containerIds)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds); // also for containers

        return _containerRepository.GetMany(containerIds);
    }

    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Will be removed in V16.")]
    public IEnumerable<EntityContainer> GetContainers(TItem item)
    {
        var ancestorIds = item.Path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out var asInt) ? asInt : int.MinValue)
            .Where(x => x != int.MinValue && x != item.Id)
            .ToArray();

        return GetContainers(ancestorIds);
    }

    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Will be removed in V16.")]
    public IEnumerable<EntityContainer> GetContainers(string name, int level)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds); // also for containers

        return _containerRepository.Get(name, level);
    }

    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Will be removed in V16.")]
    public Attempt<OperationResult?> DeleteContainer(int containerId, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        scope.WriteLock(WriteLockIds); // also for containers

        EntityContainer? container = _containerRepository?.Get(containerId);
        if (container == null)
        {
            return OperationResult.Attempt.NoOperation(eventMessages);
        }

        // 'container' here does not know about its children, so we need
        // to get it again from the entity repository, as a light entity
        IEntitySlim? entity = _entityRepository.Get(container.Id);
        if (entity?.HasChildren ?? false)
        {
            scope.Complete();
            return Attempt.Fail(new OperationResult(OperationResultType.FailedCannot, eventMessages));
        }

        var deletingNotification = new EntityContainerDeletingNotification(container, eventMessages);
        if (scope.Notifications.PublishCancelable(deletingNotification))
        {
            scope.Complete();
            return Attempt.Fail(new OperationResult(OperationResultType.FailedCancelledByEvent, eventMessages));
        }

        _containerRepository?.Delete(container);
        scope.Complete();

        var deletedNotification = new EntityContainerDeletedNotification(container, eventMessages);
        deletedNotification.WithStateFrom(deletingNotification);
        scope.Notifications.Publish(deletedNotification);

        return OperationResult.Attempt.Succeed(eventMessages);
        // TODO: Audit trail ?
    }

    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Will be removed in V16.")]
    public Attempt<OperationResult<OperationResultType, EntityContainer>?> RenameContainer(int id, string name, int userId = Constants.Security.SuperUserId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(WriteLockIds); // also for containers

            try
            {
                EntityContainer? container = _containerRepository?.Get(id);

                //throw if null, this will be caught by the catch and a failed returned
                if (container == null)
                {
                    throw new InvalidOperationException("No container found with id " + id);
                }

                container.Name = name;

                var renamingNotification = new EntityContainerRenamingNotification(container, eventMessages);
                if (scope.Notifications.PublishCancelable(renamingNotification))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel<EntityContainer>(eventMessages);
                }

                _containerRepository?.Save(container);
                scope.Complete();

                var renamedNotification = new EntityContainerRenamedNotification(container, eventMessages);
                renamedNotification.WithStateFrom(renamingNotification);
                scope.Notifications.Publish(renamedNotification);

                return OperationResult.Attempt.Succeed(OperationResultType.Success, eventMessages, container);
            }
            catch (Exception ex)
            {
                return OperationResult.Attempt.Fail<EntityContainer>(eventMessages, ex);
            }
        }
    }

    #endregion

    #region Audit

    private void Audit(AuditType type, int userId, int objectId)
    {
        _auditRepository.Save(new AuditItem(objectId, type, userId, ObjectTypes.GetUmbracoObjectType(ContainedObjectType).GetName()));
    }

    #endregion
}
