using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Asynchronous generic base class for content type services that provides CRUD operations, validation,
/// composition handling, and container management for content types, running entirely against the
/// asynchronous <see cref="IAsyncContentTypeRepositoryBase{TItem}" /> contract.
/// </summary>
/// <remarks>
/// This is the async-first counterpart of <see cref="ContentTypeServiceBase{TRepository, TItem}" />. It is used by the
/// document-type service while its repository runs on EF Core; the media- and member-type services remain on the
/// synchronous base until their repositories are migrated.
/// </remarks>
/// <typeparam name="TRepository">The type of the (asynchronous) content type repository.</typeparam>
/// <typeparam name="TItem">The type of content type.</typeparam>
public abstract class AsyncContentTypeServiceBase<TRepository, TItem> : ContentTypeServiceBase, IAsyncContentTypeBaseService<TItem>
    where TRepository : IAsyncContentTypeRepositoryBase<TItem>
    where TItem : class, IContentTypeComposition
{
    private readonly IAuditService _auditService;
    private readonly IEntityContainerRepository _containerRepository;
    private readonly IEventAggregator _eventAggregator;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly ContentTypeFilterCollection _contentTypeFilters;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncContentTypeServiceBase{TRepository, TItem}"/> class.
    /// </summary>
    /// <param name="provider">The core scope provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="repository">The content type repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="containerRepository">The entity container repository.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    /// <param name="contentTypeFilters">The content type filter collection.</param>
    protected AsyncContentTypeServiceBase(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        TRepository repository,
        IAuditService auditService,
        IEntityContainerRepository containerRepository,
        IEventAggregator eventAggregator,
        IUserIdKeyResolver userIdKeyResolver,
        ContentTypeFilterCollection contentTypeFilters)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        Repository = repository;
        _auditService = auditService;
        _containerRepository = containerRepository;
        _eventAggregator = eventAggregator;
        _userIdKeyResolver = userIdKeyResolver;
        _contentTypeFilters = contentTypeFilters;
    }

    /// <summary>
    /// Gets the content type repository.
    /// </summary>
    protected TRepository Repository { get; }

    /// <summary>
    /// Gets the write lock IDs for this content type.
    /// </summary>
    protected abstract int[] WriteLockIds { get; }

    /// <summary>
    /// Gets the read lock IDs for this content type.
    /// </summary>
    protected abstract int[] ReadLockIds { get; }

    #region Notifications

    /// <summary>
    /// Gets a saving notification for a single content type item.
    /// </summary>
    protected abstract SavingNotification<TItem> GetSavingNotification(TItem item, EventMessages eventMessages);

    /// <summary>
    /// Gets a saving notification for multiple content type items.
    /// </summary>
    protected abstract SavingNotification<TItem> GetSavingNotification(IEnumerable<TItem> items, EventMessages eventMessages);

    /// <summary>
    /// Gets a saved notification for a single content type item.
    /// </summary>
    protected abstract SavedNotification<TItem> GetSavedNotification(TItem item, EventMessages eventMessages);

    /// <summary>
    /// Gets a saved notification for multiple content type items.
    /// </summary>
    protected abstract SavedNotification<TItem> GetSavedNotification(IEnumerable<TItem> items, EventMessages eventMessages);

    /// <summary>
    /// Gets a deleting notification for a single content type item.
    /// </summary>
    protected abstract DeletingNotification<TItem> GetDeletingNotification(TItem item, EventMessages eventMessages);

    /// <summary>
    /// Gets a deleting notification for multiple content type items.
    /// </summary>
    protected abstract DeletingNotification<TItem> GetDeletingNotification(IEnumerable<TItem> items, EventMessages eventMessages);

    /// <summary>
    /// Gets a deleted notification for multiple content type items.
    /// </summary>
    protected abstract DeletedNotification<TItem> GetDeletedNotification(IEnumerable<TItem> items, EventMessages eventMessages);

    /// <summary>
    /// Gets a moving notification for a content type item.
    /// </summary>
    protected abstract MovingNotification<TItem> GetMovingNotification(MoveEventInfo<TItem> moveInfo, EventMessages eventMessages);

    /// <summary>
    /// Gets a moved notification for multiple content type items.
    /// </summary>
    protected abstract MovedNotification<TItem> GetMovedNotification(IEnumerable<MoveEventInfo<TItem>> moveInfo, EventMessages eventMessages);

    /// <summary>
    /// Gets a content type changed notification.
    /// </summary>
    protected abstract ContentTypeChangeNotification<TItem> GetContentTypeChangedNotification(IEnumerable<ContentTypeChange<TItem>> changes, EventMessages eventMessages);

    /// <summary>
    /// Gets a content type refreshed notification that is published within the transaction.
    /// </summary>
    /// <remarks>
    /// This notification is identical to GetContentTypeChangedNotification, however it needs to be a different
    /// notification type because it's published within the transaction so that listeners can perform database
    /// operations within the same transaction and guarantee data consistency. This is used by Nucache.
    /// </remarks>
    protected abstract ContentTypeRefreshNotification<TItem> GetContentTypeRefreshedNotification(IEnumerable<ContentTypeChange<TItem>> changes, EventMessages eventMessages);

    #endregion

    #region Validation

    /// <inheritdoc />
    public async Task<Attempt<string[]?>> ValidateCompositionAsync(TItem? compo)
    {
        try
        {
            using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds);
                await ValidateLockedAsync(compo!);
            }

            return Attempt<string[]?>.Succeed();
        }
        catch (InvalidCompositionException ex)
        {
            return Attempt.Fail(ex.PropertyTypeAliases, ex);
        }
    }

    /// <summary>
    /// Validates a content type composition for conflicts within a lock context.
    /// </summary>
    /// <param name="compositionContentType">The content type to validate.</param>
    /// <exception cref="InvalidCompositionException">Thrown when the composition is invalid.</exception>
    protected async Task ValidateLockedAsync(TItem compositionContentType)
    {
        // performs business-level validation of the composition
        // should ensure that it is absolutely safe to save the composition

        // eg maybe a property has been added, with an alias that's OK (no conflict with ancestors)
        // but that cannot be used (conflict with descendants)

        IContentTypeComposition[] allContentTypes = (await Repository.GetManyAsync(Array.Empty<int>(), CancellationToken.None))
            .Cast<IContentTypeComposition>().ToArray();

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

    /// <summary>
    /// Composes the content type changes for the specified content types by analyzing what properties
    /// have changed and determining the impact on related content types.
    /// </summary>
    /// <param name="contentTypes">The content types to analyze for changes.</param>
    /// <returns>A collection of content type changes indicating which types were affected and how.</returns>
    private async Task<IEnumerable<ContentTypeChange<TItem>>> ComposeContentTypeChangesAsync(params TItem[] contentTypes)
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

            // Detect all granular change types independently so that structural
            // and non-structural changes can be detected in the same operation
            // (e.g. removing a property AND adding another at the same time).
            var hasAnyChange = false;

            // --- Structural changes (each includes RefreshMain automatically) ---

            if (hasAliasChanged)
            {
                AddChange(changes, contentType, ContentTypeChangeTypes.AliasChanged);
                hasAnyChange = true;
            }

            if (hasAnyPropertyChangedAlias)
            {
                AddChange(changes, contentType, ContentTypeChangeTypes.PropertyAliasChanged);
                hasAnyChange = true;
            }

            if (hasAnyPropertyBeenRemoved)
            {
                AddChange(changes, contentType, ContentTypeChangeTypes.PropertyRemoved);
                hasAnyChange = true;
            }

            if (hasAnyCompositionBeenRemoved)
            {
                AddChange(changes, contentType, ContentTypeChangeTypes.CompositionRemoved);
                hasAnyChange = true;
            }

            if (hasAnyPropertyVariationChanged)
            {
                AddChange(changes, contentType, ContentTypeChangeTypes.PropertyVariationChanged);
                hasAnyChange = true;
            }

            // Add VariationChanged flag if content type variation changed.
            // This is used by DocumentUrlService to rebuild URL cache with correct languageId.
            if (hasContentTypeVariationChanged)
            {
                AddChange(changes, contentType, ContentTypeChangeTypes.VariationChanged);
                hasAnyChange = true;
            }

            // main impact on properties? Propagate RefreshMain to composed types.
            var hasPropertyMainImpact = hasContentTypeVariationChanged || hasAnyPropertyVariationChanged
                                                                       || hasAnyCompositionBeenRemoved || hasAnyPropertyBeenRemoved || hasAnyPropertyChangedAlias;
            if (hasPropertyMainImpact)
            {
                foreach (TItem c in await GetComposedOfAsync(contentType.Id))
                {
                    AddChange(changes, c, ContentTypeChangeTypes.RefreshMain);
                }
            }

            // --- Non-structural changes (each includes RefreshOther automatically) ---

            // new properties added?
            var hasAnyPropertyBeenAdded = contentType.PropertyTypes.Any(pt => pt.WasPropertyDirty("Id"));
            if (hasAnyPropertyBeenAdded)
            {
                AddChange(changes, contentType, ContentTypeChangeTypes.PropertyAdded);
                hasAnyChange = true;
            }

            // compositions added?
            var hasAnyCompositionBeenAdded = dirty.WasPropertyDirty("HasCompositionTypeBeenAdded");
            if (hasAnyCompositionBeenAdded)
            {
                AddChange(changes, contentType, ContentTypeChangeTypes.CompositionAdded);
                hasAnyChange = true;
            }

            // Fall back to bare RefreshOther if none of the specific checks matched
            // (e.g. for changes we haven't categorized yet).
            if (!hasAnyChange)
            {
                AddChange(changes, contentType, ContentTypeChangeTypes.RefreshOther);
            }
        }

        return changes;
    }

    /// <summary>
    /// Adds a content type change to the collection, merging change types if the content type already exists in the collection.
    /// </summary>
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

    /// <inheritdoc />
    public async Task<TItem?> GetAsync(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return await Repository.GetAsync(id, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<TItem?> GetAsync(string alias)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return await Repository.GetAsync(alias, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<TItem?> GetAsync(Guid id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return await Repository.GetAsync(id, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TItem>> GetAllAsync()
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return await Repository.GetManyAsync(Array.Empty<Guid>(), CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TItem>> GetManyAsync(params int[] ids)
    {
        if (ids.Any() is false)
        {
            return Enumerable.Empty<TItem>();
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return await Repository.GetManyAsync(ids, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TItem>> GetManyAsync(IEnumerable<Guid>? ids)
    {
        if (ids is null || ids.Any() is false)
        {
            return Enumerable.Empty<TItem>();
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return await Repository.GetManyAsync(ids.ToArray(), CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TItem>> GetChildrenAsync(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return await Repository.GetByParentIdAsync(id, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TItem>> GetChildrenAsync(Guid id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);

        TItem? found = await Repository.GetAsync(id, CancellationToken.None);
        if (found == null)
        {
            return Enumerable.Empty<TItem>();
        }

        return await Repository.GetByParentIdAsync(found.Id, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<bool> HasChildrenAsync(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return await Repository.HasChildrenAsync(id, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<bool> HasChildrenAsync(Guid id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);

        TItem? found = await Repository.GetAsync(id, CancellationToken.None);
        if (found == null)
        {
            return false;
        }

        return await Repository.HasChildrenAsync(found.Id, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<bool> HasContainerInPathAsync(string contentPath)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return await Repository.HasContainerInPathAsync(contentPath, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<bool> HasContainerInPathAsync(params int[] ids)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return await Repository.HasContainerInPathAsync(ids, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TItem>> GetDescendantsAsync(int id, bool andSelf)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);

        var descendants = new List<TItem>();
        if (andSelf)
        {
            TItem? self = await Repository.GetAsync(id, CancellationToken.None);
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
            TItem[] result = (await Repository.GetByParentIdAsync(i, CancellationToken.None)).ToArray();

            foreach (TItem c in result)
            {
                descendants.Add(c);
                ids.Push(c.Id);
            }
        }

        return descendants.ToArray();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TItem>> GetComposedOfAsync(int id)
    {
        // GetAll is cheap, repository has a full dataset cache policy
        IEnumerable<TItem> allContentTypes = await GetAllAsync();
        return allContentTypes.Where(x => x.ContentTypeComposition.Any(y => y.Id == id));
    }

    /// <inheritdoc />
    public async Task<int> CountAsync()
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return await Repository.CountAsync(CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<bool> HasContentNodesAsync(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return await Repository.HasContentNodesAsync(id, CancellationToken.None);
    }

    #endregion

    #region Save

    /// <inheritdoc />
    public async Task<Attempt<ContentTypeOperationStatus>> CreateAsync(TItem item, Guid performingUserKey) => await InternalSaveAsync(item, performingUserKey);

    /// <inheritdoc />
    public async Task<Attempt<ContentTypeOperationStatus>> UpdateAsync(TItem item, Guid performingUserKey) => await InternalSaveAsync(item, performingUserKey);

    /// <summary>
    /// Internal implementation of the save operation with validation and notifications.
    /// </summary>
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
        await ValidateLockedAsync(item); // throws if invalid

        int userId = await _userIdKeyResolver.GetAsync(performingUserKey);
        item.CreatorId = userId;
        if (item.Description == string.Empty)
        {
            item.Description = null;
        }

        await Repository.SaveAsync(item, CancellationToken.None); // also updates content/media/member items

        // figure out impacted content types
        ContentTypeChange<TItem>[] changes = (await ComposeContentTypeChangesAsync(item)).ToArray();

        // Publish this in scope, see comment at GetContentTypeRefreshedNotification for more info.
        await _eventAggregator.PublishAsync(GetContentTypeRefreshedNotification(changes, eventMessages));

        scope.Notifications.Publish(GetContentTypeChangedNotification(changes, eventMessages));

        SavedNotification<TItem> savedNotification = GetSavedNotification(item, eventMessages);
        savedNotification.WithStateFrom(savingNotification);
        scope.Notifications.Publish(savedNotification);

        await AuditAsync(AuditType.Save, userId, item.Id);
        scope.Complete();

        return Attempt.Succeed(ContentTypeOperationStatus.Success);
    }

    /// <summary>
    /// Validates common properties of a content type.
    /// </summary>
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

    /// <inheritdoc />
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

        EventMessages eventMessages = EventMessagesFactory.Get();
        DeletingNotification<TItem> deletingNotification = GetDeletingNotification(item, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(deletingNotification))
        {
            scope.Complete();
            return ContentTypeOperationStatus.CancelledByNotification;
        }

        await PerformDeleteAsync(scope, item, deletingNotification, eventMessages, performingUserId);

        scope.Complete();
        return ContentTypeOperationStatus.Success;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(TItem item, Guid performingUserKey)
    {
        if (CanDelete(item) is false)
        {
            throw new InvalidOperationException("The item was not allowed to be deleted");
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        EventMessages eventMessages = EventMessagesFactory.Get();
        DeletingNotification<TItem> deletingNotification = GetDeletingNotification(item, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(deletingNotification))
        {
            scope.Complete();
            return;
        }

        int userId = await _userIdKeyResolver.GetAsync(performingUserKey);
        await PerformDeleteAsync(scope, item, deletingNotification, eventMessages, userId);

        scope.Complete();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(IEnumerable<TItem> items, Guid performingUserKey)
    {
        TItem[] itemsA = items.ToArray();
        if (itemsA.All(CanDelete) is false)
        {
            throw new InvalidOperationException("One or more items were not allowed to be deleted");
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        EventMessages eventMessages = EventMessagesFactory.Get();
        DeletingNotification<TItem> deletingNotification = GetDeletingNotification(itemsA, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(deletingNotification))
        {
            scope.Complete();
            return;
        }

        scope.WriteLock(WriteLockIds);

        // all descendants are going to be deleted
        var allDescendants = new List<TItem>();
        foreach (TItem item in itemsA)
        {
            allDescendants.AddRange(await GetDescendantsAsync(item.Id, true));
        }

        TItem[] allDescendantsAndSelf = allDescendants.DistinctBy(x => x.Id).ToArray();
        TItem[] deleted = allDescendantsAndSelf;

        // all impacted (through composition) probably lose some properties
        // don't try to be too clever here, just report them all
        // do this before anything is deleted
        var composed = new List<TItem>();
        foreach (TItem item in allDescendantsAndSelf)
        {
            composed.AddRange(await GetComposedOfAsync(item.Id));
        }

        TItem[] changed = composed.Distinct().Except(allDescendantsAndSelf).ToArray();

        // delete content
        await DeleteItemsOfTypesAsync(allDescendantsAndSelf.Select(x => x.Id));

        int userId = await _userIdKeyResolver.GetAsync(performingUserKey);

        // finally delete the content types
        // (see notes in PerformDeleteAsync)
        foreach (TItem item in itemsA)
        {
            await Repository.DeleteAsync(item, CancellationToken.None);
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

        await AuditAsync(AuditType.Delete, userId, -1);
        scope.Complete();
    }

    private async Task PerformDeleteAsync(ICoreScope scope, TItem item, DeletingNotification<TItem> deletingNotification, EventMessages eventMessages, int userId)
    {
        scope.WriteLock(WriteLockIds);

        TItem[] descendantsAndSelf = (await GetDescendantsAsync(item.Id, true)).ToArray();

        // all impacted (through composition) probably lose some properties
        // don't try to be too clever here, just report them all
        // do this before anything is deleted
        var composed = new List<TItem>();
        foreach (TItem descendant in descendantsAndSelf)
        {
            composed.AddRange(await GetComposedOfAsync(descendant.Id));
        }

        TItem[] changed = composed.Distinct().Except(descendantsAndSelf).ToArray();

        await DeleteItemsOfTypesAsync(descendantsAndSelf.Select(x => x.Id));

        // remove references to this content type from other content types
        IEnumerable<TItem> referenceToAllowedContentTypes = (await GetAllAsync()).Where(q => q.AllowedContentTypes?.Any(p => p.Key == item.Key) ?? false);
        foreach (TItem reference in referenceToAllowedContentTypes)
        {
            reference.AllowedContentTypes = reference.AllowedContentTypes?.Where(p => p.Key != item.Key);
            var changedRef = new List<ContentTypeChange<TItem>> { new ContentTypeChange<TItem>(reference, ContentTypeChangeTypes.RefreshMain) };
            scope.Notifications.Publish(GetContentTypeChangedNotification(changedRef, eventMessages));
        }

        // finally delete the content type
        // - recursively deletes all descendants
        // - deletes all associated property data
        //  (contents of any descendant type have been deleted but
        //   contents of any composed (impacted) type remain but
        //   need to have their property data cleared)
        await Repository.DeleteAsync(item, CancellationToken.None);

        ContentTypeChange<TItem>[] changes = descendantsAndSelf.Select(x => new ContentTypeChange<TItem>(x, ContentTypeChangeTypes.Remove))
            .Concat(changed.Select(x => new ContentTypeChange<TItem>(x, ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther)))
            .ToArray();

        // Publish this in scope, see comment at GetContentTypeRefreshedNotification for more info.
        _eventAggregator.Publish(GetContentTypeRefreshedNotification(changes, eventMessages));
        scope.Notifications.Publish(GetContentTypeChangedNotification(changes, eventMessages));

        DeletedNotification<TItem> deletedNotification = GetDeletedNotification(descendantsAndSelf.DistinctBy(x => x.Id), eventMessages);
        deletedNotification.WithStateFrom(deletingNotification);
        scope.Notifications.Publish(deletedNotification);

        await AuditAsync(AuditType.Delete, userId, item.Id);
    }

    /// <summary>
    /// Deletes content items of the specified types.
    /// </summary>
    /// <param name="typeIds">The type IDs whose content should be deleted.</param>
    protected abstract Task DeleteItemsOfTypesAsync(IEnumerable<int> typeIds);

    /// <summary>
    /// Determines whether the specified content type can be deleted.
    /// </summary>
    protected virtual bool CanDelete(TItem item) => true;

    #endregion

    #region Copy

    /// <inheritdoc />
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

        EventMessages eventMessages = EventMessagesFactory.Get();

        TItem copy;
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            scope.WriteLock(WriteLockIds);

            try
            {
                if (containerId.Value > 0)
                {
                    EntityContainer? container = _containerRepository?.Get(containerId.Value);
                    if (container == null)
                    {
                        throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound); // causes rollback
                    }
                }

                var alias = await Repository.GetUniqueAliasAsync(toCopy.Alias, CancellationToken.None);

                copy = (TItem)toCopy.DeepCloneWithResetIdentities(alias);

                copy.Name = copy.Name + " (copy)"; // might not be unique

                // if it has a parent, and the parent is a content type, unplug composition
                // all other compositions remain in place in the copied content type
                if (copy.ParentId > 0)
                {
                    TItem? parent = await Repository.GetAsync(copy.ParentId, CancellationToken.None);
                    if (parent != null)
                    {
                        copy.RemoveContentType(parent.Alias);
                    }
                }

                copy.ParentId = containerId.Value;

                SavingNotification<TItem> savingNotification = GetSavingNotification(copy, eventMessages);
                if (scope.Notifications.PublishCancelable(savingNotification))
                {
                    scope.Complete();
                    return MapStatusTypeToAttempt(copy, MoveOperationStatusType.FailedCancelledByEvent);
                }

                await Repository.SaveAsync(copy, CancellationToken.None);

                ContentTypeChange<TItem>[] changes = (await ComposeContentTypeChangesAsync(copy)).ToArray();

                _eventAggregator.Publish(GetContentTypeRefreshedNotification(changes, eventMessages));
                scope.Notifications.Publish(GetContentTypeChangedNotification(changes, eventMessages));

                SavedNotification<TItem> savedNotification = GetSavedNotification(copy, eventMessages);
                savedNotification.WithStateFrom(savingNotification);
                scope.Notifications.Publish(savedNotification);

                scope.Complete();
            }
            catch (DataOperationException<MoveOperationStatusType> ex)
            {
                return MapStatusTypeToAttempt(toCopy, ex.Operation); // causes rollback
            }
        }

        return MapStatusTypeToAttempt(copy, MoveOperationStatusType.Success);
    }

    /// <summary>
    /// Gets the container ID from a container key, or returns the root ID if no key is specified.
    /// </summary>
    private int? GetContainerOrRootId(Guid? containerKey)
    {
        if (containerKey is null)
        {
            return Constants.System.Root;
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds); // also for containers
        EntityContainer? container = _containerRepository.Get(containerKey.Value);
        return container?.Id;
    }

    /// <summary>
    /// Maps a move operation status type to a content type structure operation status attempt.
    /// </summary>
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

    /// <inheritdoc />
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

        EventMessages eventMessages = EventMessagesFactory.Get();
        if (toMove.ParentId == containerId.Value)
        {
            return MapStatusTypeToAttempt(toMove, MoveOperationStatusType.Success);
        }

        var moveInfo = new List<MoveEventInfo<TItem>>();
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var moveEventInfo = new MoveEventInfo<TItem>(toMove, toMove.Path, containerKey);
            MovingNotification<TItem> movingNotification = GetMovingNotification(moveEventInfo, eventMessages);
            if (await scope.Notifications.PublishCancelableAsync(movingNotification))
            {
                scope.Complete();
                return MapStatusTypeToAttempt(toMove, MoveOperationStatusType.FailedCancelledByEvent);
            }

            scope.WriteLock(WriteLockIds); // also for containers

            try
            {
                EntityContainer? container = null;
                if (containerId.Value > 0)
                {
                    container = _containerRepository?.Get(containerId.Value);
                    if (container == null)
                    {
                        throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound); // causes rollback
                    }
                }

                moveInfo.AddRange(await Repository.MoveAsync(toMove, container, CancellationToken.None));
                scope.Complete();
            }
            catch (DataOperationException<MoveOperationStatusType> ex)
            {
                scope.Complete();
                return MapStatusTypeToAttempt(toMove, ex.Operation);
            }

            // note: not raising any Changed event here because moving a content type under another container
            // has no impact on the published content types - would be entirely different if we were to support
            // moving a content type under another content type.
            MovedNotification<TItem> movedNotification = GetMovedNotification(moveInfo, eventMessages);
            movedNotification.WithStateFrom(movingNotification);
            scope.Notifications.Publish(movedNotification);
        }

        return MapStatusTypeToAttempt(toMove, MoveOperationStatusType.Success);
    }

    #endregion

    #region Allowed types

    /// <summary>
    /// Gets the content types that are candidates for being allowed at root.
    /// </summary>
    /// <remarks>
    /// Override this in derived classes to change the filtering behavior. For example,
    /// member types override this to return all member types, since members are a flat list.
    /// </remarks>
    protected virtual Task<IEnumerable<TItem>> GetAllowedAtRootCandidatesAsync()
        => Repository.GetAllowedAsRootAsync(CancellationToken.None);

    /// <inheritdoc />
    public async Task<PagedModel<TItem>> GetAllAllowedAsRootAsync(int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        // that one is special because it works across content, media and member types
        scope.ReadLock(Constants.Locks.ContentTypes, Constants.Locks.MediaTypes, Constants.Locks.MemberTypes);

        IEnumerable<TItem> contentTypes = await GetAllowedAtRootCandidatesAsync();

        foreach (IContentTypeFilter filter in _contentTypeFilters)
        {
            contentTypes = await filter.FilterAllowedAtRootAsync(contentTypes);
        }

        TItem[] materialized = contentTypes.ToArray();

        return new PagedModel<TItem>
        {
            Total = materialized.Length,
            Items = materialized.Skip(skip).Take(take)
        };
    }

    /// <inheritdoc />
    public async Task<PagedModel<TItem>> GetAllAllowedInLibraryAsync(int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);

        IEnumerable<TItem> contentTypes = await Repository.GetAllowedInLibraryAsync(CancellationToken.None);

        foreach (IContentTypeFilter filter in _contentTypeFilters)
        {
            contentTypes = await filter.FilterAllowedInLibraryAsync(contentTypes);
        }

        contentTypes = contentTypes.ToArray();

        return new PagedModel<TItem>
        {
            Total = contentTypes.Count(),
            Items = contentTypes.Skip(skip).Take(take),
        };
    }

    /// <inheritdoc />
    public async Task<Attempt<PagedModel<TItem>?, ContentTypeOperationStatus>> GetAllowedChildrenAsync(Guid key, int skip, int take)
        => await GetAllowedChildrenAsync(key, null, skip, take);

    /// <inheritdoc />
    public async Task<Attempt<PagedModel<TItem>?, ContentTypeOperationStatus>> GetAllowedChildrenAsync(Guid key, Guid? parentContentKey, int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        TItem? parent = await GetAsync(key);

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

            TItem[] allowedChildren = (await GetManyAsync(sortedKeys)).ToArray();
            result = new PagedModel<TItem>
            {
                Items = allowedChildren.OrderBy(x => sortedKeys.IndexOf(x.Key)).Take(take).Skip(skip),
                Total = allowedChildren.Length,
            };
        }

        return Attempt.SucceedWithStatus<PagedModel<TItem>?, ContentTypeOperationStatus>(ContentTypeOperationStatus.Success, result);
    }

    /// <inheritdoc/>
    public async Task<Attempt<IEnumerable<Guid>, ContentTypeOperationStatus>> GetAllowedParentKeysAsync(Guid key)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        TItem? content = await GetAsync(key);
        if (content is null)
        {
            return Attempt.FailWithStatus<IEnumerable<Guid>, ContentTypeOperationStatus>(ContentTypeOperationStatus.NotFound, []);
        }

        IEnumerable<Guid> allowedParentKeys = await PerformGetAllowedParentKeysAsync(key);

        return Attempt.SucceedWithStatus(ContentTypeOperationStatus.Success, allowedParentKeys);
    }

    /// <summary>
    /// Retrieves a collection of allowed parent keys for the specified key.
    /// </summary>
    protected virtual Task<IEnumerable<Guid>> PerformGetAllowedParentKeysAsync(Guid key)
        => Repository.GetAllowedParentKeysAsync(key, CancellationToken.None);

    #endregion

    #region Containers

    /// <summary>
    /// Gets the object type GUID for content types contained by this service.
    /// </summary>
    protected abstract Guid ContainedObjectType { get; }

    /// <summary>
    /// Gets the container object type GUID.
    /// </summary>
    protected Guid ContainerObjectType => EntityContainer.GetContainerObjectType(ContainedObjectType);

    #endregion

    #region Audit

    /// <summary>
    /// Creates an audit entry asynchronously.
    /// </summary>
    private async Task AuditAsync(AuditType type, int userId, int objectId)
    {
        Guid userKey = await _userIdKeyResolver.GetAsync(userId);

        await _auditService.AddAsync(
            type,
            userKey,
            objectId,
            ObjectTypes.GetUmbracoObjectType(ContainedObjectType).GetName());
    }

    #endregion
}
