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

/// <summary>
/// Generic base class for content type services that provides CRUD operations, validation,
/// composition handling, and container management for content types.
/// </summary>
/// <typeparam name="TRepository">The type of the content type repository.</typeparam>
/// <typeparam name="TItem">The type of content type.</typeparam>
public abstract class ContentTypeServiceBase<TRepository, TItem> : ContentTypeServiceBase, IContentTypeBaseService<TItem>
    where TRepository : IContentTypeRepositoryBase<TItem>
    where TItem : class, IContentTypeComposition
{
    private readonly IAuditService _auditService;
    private readonly IEntityContainerRepository _containerRepository;
    private readonly IEntityRepository _entityRepository;
    private readonly IEventAggregator _eventAggregator;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly ContentTypeFilterCollection _contentTypeFilters;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeServiceBase{TRepository, TItem}"/> class.
    /// </summary>
    /// <param name="provider">The core scope provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="repository">The content type repository.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="containerRepository">The entity container repository.</param>
    /// <param name="entityRepository">The entity repository.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    /// <param name="contentTypeFilters">The content type filter collection.</param>
    protected ContentTypeServiceBase(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        TRepository repository,
        IAuditService auditService,
        IEntityContainerRepository containerRepository,
        IEntityRepository entityRepository,
        IEventAggregator eventAggregator,
        IUserIdKeyResolver userIdKeyResolver,
        ContentTypeFilterCollection contentTypeFilters)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        Repository = repository;
        _auditService = auditService;
        _containerRepository = containerRepository;
        _entityRepository = entityRepository;
        _eventAggregator = eventAggregator;
        _userIdKeyResolver = userIdKeyResolver;
        _contentTypeFilters = contentTypeFilters;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeServiceBase{TRepository, TItem}"/> class.
    /// </summary>
    /// <param name="provider">The core scope provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="repository">The content type repository.</param>
    /// <param name="auditRepository">The audit repository.</param>
    /// <param name="containerRepository">The entity container repository.</param>
    /// <param name="entityRepository">The entity repository.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="userIdKeyResolver">The user ID key resolver.</param>
    /// <param name="contentTypeFilters">The content type filter collection.</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
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
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            repository,
            StaticServiceProvider.Instance.GetRequiredService<IAuditService>(),
            containerRepository,
            entityRepository,
            eventAggregator,
            userIdKeyResolver,
            contentTypeFilters)
    {
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
    /// <param name="item">The content type item being saved.</param>
    /// <param name="eventMessages">The event messages.</param>
    /// <returns>The saving notification.</returns>
    protected abstract SavingNotification<TItem> GetSavingNotification(TItem item, EventMessages eventMessages);

    /// <summary>
    /// Gets a saving notification for multiple content type items.
    /// </summary>
    /// <param name="items">The content type items being saved.</param>
    /// <param name="eventMessages">The event messages.</param>
    /// <returns>The saving notification.</returns>
    protected abstract SavingNotification<TItem> GetSavingNotification(IEnumerable<TItem> items, EventMessages eventMessages);

    /// <summary>
    /// Gets a saved notification for a single content type item.
    /// </summary>
    /// <param name="item">The content type item that was saved.</param>
    /// <param name="eventMessages">The event messages.</param>
    /// <returns>The saved notification.</returns>
    protected abstract SavedNotification<TItem> GetSavedNotification(TItem item, EventMessages eventMessages);

    /// <summary>
    /// Gets a saved notification for multiple content type items.
    /// </summary>
    /// <param name="items">The content type items that were saved.</param>
    /// <param name="eventMessages">The event messages.</param>
    /// <returns>The saved notification.</returns>
    protected abstract SavedNotification<TItem> GetSavedNotification(IEnumerable<TItem> items, EventMessages eventMessages);

    /// <summary>
    /// Gets a deleting notification for a single content type item.
    /// </summary>
    /// <param name="item">The content type item being deleted.</param>
    /// <param name="eventMessages">The event messages.</param>
    /// <returns>The deleting notification.</returns>
    protected abstract DeletingNotification<TItem> GetDeletingNotification(TItem item, EventMessages eventMessages);

    /// <summary>
    /// Gets a deleting notification for multiple content type items.
    /// </summary>
    /// <param name="items">The content type items being deleted.</param>
    /// <param name="eventMessages">The event messages.</param>
    /// <returns>The deleting notification.</returns>
    protected abstract DeletingNotification<TItem> GetDeletingNotification(IEnumerable<TItem> items, EventMessages eventMessages);

    /// <summary>
    /// Gets a deleted notification for multiple content type items.
    /// </summary>
    /// <param name="items">The content type items that were deleted.</param>
    /// <param name="eventMessages">The event messages.</param>
    /// <returns>The deleted notification.</returns>
    protected abstract DeletedNotification<TItem> GetDeletedNotification(IEnumerable<TItem> items, EventMessages eventMessages);

    /// <summary>
    /// Gets a moving notification for a content type item.
    /// </summary>
    /// <param name="moveInfo">The move event information.</param>
    /// <param name="eventMessages">The event messages.</param>
    /// <returns>The moving notification.</returns>
    protected abstract MovingNotification<TItem> GetMovingNotification(MoveEventInfo<TItem> moveInfo, EventMessages eventMessages);

    /// <summary>
    /// Gets a moved notification for multiple content type items.
    /// </summary>
    /// <param name="moveInfo">The collection of move event information.</param>
    /// <param name="eventMessages">The event messages.</param>
    /// <returns>The moved notification.</returns>
    protected abstract MovedNotification<TItem> GetMovedNotification(IEnumerable<MoveEventInfo<TItem>> moveInfo, EventMessages eventMessages);

    /// <summary>
    /// Gets a content type changed notification.
    /// </summary>
    /// <param name="changes">The collection of content type changes.</param>
    /// <param name="eventMessages">The event messages.</param>
    /// <returns>The content type changed notification.</returns>
    protected abstract ContentTypeChangeNotification<TItem> GetContentTypeChangedNotification(IEnumerable<ContentTypeChange<TItem>> changes, EventMessages eventMessages);

    /// <summary>
    /// Gets a content type refreshed notification that is published within the transaction.
    /// </summary>
    /// <param name="changes">The collection of content type changes.</param>
    /// <param name="eventMessages">The event messages.</param>
    /// <returns>The content type refreshed notification.</returns>
    /// <remarks>
    /// This notification is identical to GetContentTypeChangedNotification, however it needs to be a different notification type
    /// because it's published within the transaction. The purpose of this notification being published within the transaction
    /// is so that listeners can perform database operations from within the same transaction and guarantee data consistency
    /// so that if anything goes wrong the entire transaction can be rolled back. This is used by Nucache.
    /// </remarks>
    protected abstract ContentTypeRefreshNotification<TItem> GetContentTypeRefreshedNotification(IEnumerable<ContentTypeChange<TItem>> changes, EventMessages eventMessages);

    #endregion

    #region Validation

    /// <inheritdoc />
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

    /// <summary>
    /// Validates a content type composition for conflicts within a lock context.
    /// </summary>
    /// <param name="compositionContentType">The content type to validate.</param>
    /// <exception cref="InvalidCompositionException">Thrown when the composition is invalid.</exception>
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

    /// <summary>
    /// Composes the content type changes for the specified content types by analyzing what properties
    /// have changed and determining the impact on related content types.
    /// </summary>
    /// <param name="contentTypes">The content types to analyze for changes.</param>
    /// <returns>A collection of content type changes indicating which types were affected and how.</returns>
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

    /// <summary>
    /// Adds a content type change to the collection, merging change types if the content type already exists in the collection.
    /// </summary>
    /// <param name="changes">The collection of changes to add to.</param>
    /// <param name="contentType">The content type that changed.</param>
    /// <param name="changeTypes">The types of changes that occurred.</param>
    /// <remarks>Ensures the changes collection contains no duplicates by merging change types.</remarks>
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
    IContentTypeComposition? IContentTypeBaseService.Get(int id)
    {
        return Get(id);
    }

    /// <inheritdoc />
    public TItem? Get(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return Repository.Get(id);
    }

    /// <inheritdoc />
    public TItem? Get(string alias)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return Repository.Get(alias);
    }

    /// <inheritdoc />
    public TItem? Get(Guid id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return Repository.Get(id);
    }

    /// <inheritdoc />
    public Task<TItem?> GetAsync(Guid guid) => Task.FromResult(Get(guid));

    /// <inheritdoc />
    public IEnumerable<TItem> GetAll()
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return Repository.GetMany(Array.Empty<Guid>());
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public IEnumerable<TItem> GetChildren(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        IQuery<TItem> query = Query<TItem>().Where(x => x.ParentId == id);
        return Repository.Get(query);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public bool HasChildren(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        IQuery<TItem> query = Query<TItem>().Where(x => x.ParentId == id);
        var count = Repository.Count(query);
        return count > 0;
    }

    /// <inheritdoc />
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
    /// Determines whether the content item with the specified path exists underneath a list view content item.
    /// </summary>
    /// <param name="contentPath">The path of the content item to check.</param>
    /// <returns><c>true</c> if the content item exists underneath a list view; otherwise, <c>false</c>.</returns>
    public bool HasContainerInPath(string contentPath)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        // can use same repo for both content and media
        return Repository.HasContainerInPath(contentPath);
    }

    /// <summary>
    /// Checks whether any of the specified content items exist underneath a list view content item.
    /// </summary>
    /// <param name="ids">The IDs of the content items to check.</param>
    /// <returns><c>true</c> if any of the content items exist underneath a list view; otherwise, <c>false</c>.</returns>
    public bool HasContainerInPath(params int[] ids)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        // can use same repo for both content and media
        return Repository.HasContainerInPath(ids);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public IEnumerable<TItem> GetComposedOf(int id, IEnumerable<TItem> all) =>
        all.Where(x => x.ContentTypeComposition.Any(y => y.Id == id));

    /// <inheritdoc />
    public IEnumerable<TItem> GetComposedOf(int id)
    {
        // GetAll is cheap, repository has a full dataset cache policy
        // TODO: still, because it uses the cache, race conditions!
        IEnumerable<TItem> allContentTypes = GetAll();
        return GetComposedOf(id, allContentTypes);
    }

    /// <inheritdoc />
    public int Count()
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return Repository.Count(Query<TItem>());
    }

    /// <inheritdoc />
    public bool HasContentNodes(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds);
        return Repository.HasContentNodes(id);
    }

    #endregion

    #region Save

    /// <inheritdoc />
    public async Task SaveAsync(TItem item, Guid performingUserKey)
    {
        var userId = await _userIdKeyResolver.GetAsync(performingUserKey);
        Save(item, userId);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<Attempt<ContentTypeOperationStatus>> CreateAsync(TItem item, Guid performingUserKey) => await InternalSaveAsync(item, performingUserKey);

    /// <inheritdoc />
    public async Task<Attempt<ContentTypeOperationStatus>> UpdateAsync(TItem item, Guid performingUserKey) => await InternalSaveAsync(item, performingUserKey);

    /// <summary>
    /// Internal implementation of the save operation with validation and notifications.
    /// </summary>
    /// <param name="item">The content type to save.</param>
    /// <param name="performingUserKey">The unique identifier of the user performing the operation.</param>
    /// <returns>An attempt indicating the operation status.</returns>
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

    /// <summary>
    /// Validates common properties of a content type.
    /// </summary>
    /// <param name="item">The content type to validate.</param>
    /// <returns>An attempt indicating the validation result.</returns>
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

        Delete(item, performingUserId);

        scope.Complete();
        return ContentTypeOperationStatus.Success;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <summary>
    /// Deletes content items of the specified types.
    /// </summary>
    /// <param name="typeIds">The type IDs whose content should be deleted.</param>
    protected abstract void DeleteItemsOfTypes(IEnumerable<int> typeIds);

    /// <summary>
    /// Determines whether the specified content type can be deleted.
    /// </summary>
    /// <param name="item">The content type to check.</param>
    /// <returns><c>true</c> if the content type can be deleted; otherwise, <c>false</c>.</returns>
    protected virtual bool CanDelete(TItem item) => true;

    #endregion

    #region Copy

    /// <summary>
    /// Copies the specified content type to a new content type with the specified alias and name.
    /// </summary>
    /// <param name="original">The original content type to copy.</param>
    /// <param name="alias">The alias for the new content type.</param>
    /// <param name="name">The name for the new content type.</param>
    /// <param name="parentId">The parent identifier for the new content type. Use -1 for root.</param>
    /// <returns>The newly created content type copy.</returns>
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

    /// <summary>
    /// Copies the specified content type to a new content type with the specified alias, name, and parent.
    /// </summary>
    /// <param name="original">The original content type to copy.</param>
    /// <param name="alias">The alias for the new content type.</param>
    /// <param name="name">The name for the new content type.</param>
    /// <param name="parent">The parent content type for the new content type. Use null for root.</param>
    /// <returns>The newly created content type copy.</returns>
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

    /// <summary>
    /// Copies a content type to a specified container.
    /// </summary>
    /// <param name="copying">The content type to copy.</param>
    /// <param name="containerId">The identifier of the target container. Use -1 for root.</param>
    /// <returns>An attempt result containing the operation status and the copied content type.</returns>
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

                var copyingb = copying;
                copy = (TItem)copyingb.DeepCloneWithResetIdentities(alias);

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

        // using obsolete method for version control while it still exists
        Attempt<OperationResult<MoveOperationStatusType, TItem>?> result = Copy(toCopy, containerId.Value);

        return MapStatusTypeToAttempt(result.Result?.Entity, result.Result?.Result);
    }

    /// <summary>
    /// Gets the container ID from a container key, or returns the root ID if no key is specified.
    /// </summary>
    /// <param name="containerKey">The container key, or <c>null</c> for root.</param>
    /// <returns>The container ID, or <c>null</c> if the container was not found.</returns>
    private int? GetContainerOrRootId(Guid? containerKey)
    {
        if (containerKey is null)
        {
            return Constants.System.Root;
        }

        EntityContainer? container = GetContainer(containerKey.Value);
        return container?.Id;
    }

    /// <summary>
    /// Maps a move operation status type to a content type structure operation status attempt.
    /// </summary>
    /// <param name="item">The content type item.</param>
    /// <param name="resultStatus">The move operation result status.</param>
    /// <returns>An attempt with the mapped status.</returns>
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

    /// <summary>
    /// Moves a content type to a specified container.
    /// </summary>
    /// <param name="moving">The content type to move.</param>
    /// <param name="containerId">The identifier of the target container. Use -1 for root.</param>
    /// <returns>An attempt result containing the operation status.</returns>
    [Obsolete("Please use MoveAsync. Will be removed in V16.")]
    public Attempt<OperationResult<MoveOperationStatusType>?> Move(TItem moving, int containerId)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();
        if (moving.ParentId == containerId)
        {
            return OperationResult.Attempt.Succeed(MoveOperationStatusType.Success, eventMessages);
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

    /// <inheritdoc/>
    public async Task<Attempt<IEnumerable<Guid>, ContentTypeOperationStatus>> GetAllowedParentKeysAsync(Guid key)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        TItem? content = Get(key);
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
    /// <param name="key">The unique identifier of the key for which to retrieve allowed parent keys.</param>
    protected virtual Task<IEnumerable<Guid>> PerformGetAllowedParentKeysAsync(Guid key) => Task.FromResult(Repository.GetAllowedParentKeys(key));

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

    /// <summary>
    /// Creates a new entity container for organizing content types.
    /// </summary>
    /// <param name="parentId">The parent container identifier. Use -1 for root.</param>
    /// <param name="key">The unique key for the container.</param>
    /// <param name="name">The name of the container.</param>
    /// <param name="userId">The identifier of the user creating the container.</param>
    /// <returns>An attempt result containing the operation status and the created container.</returns>
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

    /// <summary>
    /// Saves an entity container.
    /// </summary>
    /// <param name="container">The container to save.</param>
    /// <param name="userId">The identifier of the user saving the container.</param>
    /// <returns>An attempt result containing the operation status.</returns>
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

    /// <summary>
    /// Gets an entity container by its integer identifier.
    /// </summary>
    /// <param name="containerId">The integer identifier of the container.</param>
    /// <returns>The entity container if found; otherwise, null.</returns>
    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Will be removed in V16.")]
    public EntityContainer? GetContainer(int containerId)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds); // also for containers

        return _containerRepository.Get(containerId);
    }

    /// <summary>
    /// Gets an entity container by its GUID identifier.
    /// </summary>
    /// <param name="containerId">The GUID identifier of the container.</param>
    /// <returns>The entity container if found; otherwise, null.</returns>
    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Will be removed in V16.")]
    public EntityContainer? GetContainer(Guid containerId)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds); // also for containers

        return _containerRepository.Get(containerId);
    }

    /// <summary>
    /// Gets entity containers by their integer identifiers.
    /// </summary>
    /// <param name="containerIds">The array of container identifiers.</param>
    /// <returns>A collection of entity containers.</returns>
    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Will be removed in V16.")]
    public IEnumerable<EntityContainer> GetContainers(int[] containerIds)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds); // also for containers

        return _containerRepository.GetMany(containerIds);
    }

    /// <summary>
    /// Gets the ancestor containers of the specified content type item.
    /// </summary>
    /// <param name="item">The content type item to get ancestor containers for.</param>
    /// <returns>A collection of ancestor entity containers.</returns>
    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Will be removed in V16.")]
    public IEnumerable<EntityContainer> GetContainers(TItem item)
    {
        var ancestorIds = item.Path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out var asInt) ? asInt : int.MinValue)
            .Where(x => x != int.MinValue && x != item.Id)
            .ToArray();

        return GetContainers(ancestorIds);
    }

    /// <summary>
    /// Gets entity containers by name and level.
    /// </summary>
    /// <param name="name">The name of the containers to find.</param>
    /// <param name="level">The level of the containers in the hierarchy.</param>
    /// <returns>A collection of entity containers matching the criteria.</returns>
    [Obsolete($"Please use {nameof(IContentTypeContainerService)} or {nameof(IMediaTypeContainerService)} for all content or media type container operations. Will be removed in V16.")]
    public IEnumerable<EntityContainer> GetContainers(string name, int level)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(ReadLockIds); // also for containers

        return _containerRepository.Get(name, level);
    }

    /// <summary>
    /// Deletes an entity container.
    /// </summary>
    /// <param name="containerId">The identifier of the container to delete.</param>
    /// <param name="userId">The identifier of the user deleting the container.</param>
    /// <returns>An attempt result containing the operation status.</returns>
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

    /// <summary>
    /// Renames an entity container.
    /// </summary>
    /// <param name="id">The identifier of the container to rename.</param>
    /// <param name="name">The new name for the container.</param>
    /// <param name="userId">The identifier of the user renaming the container.</param>
    /// <returns>An attempt result containing the operation status and the renamed container.</returns>
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

    /// <summary>
    /// Creates an audit entry synchronously.
    /// </summary>
    /// <param name="type">The type of audit entry.</param>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <param name="objectId">The ID of the object being audited.</param>
    private void Audit(AuditType type, int userId, int objectId) =>
        AuditAsync(type, userId, objectId).GetAwaiter().GetResult();

    /// <summary>
    /// Creates an audit entry asynchronously.
    /// </summary>
    /// <param name="type">The type of audit entry.</param>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <param name="objectId">The ID of the object being audited.</param>
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
