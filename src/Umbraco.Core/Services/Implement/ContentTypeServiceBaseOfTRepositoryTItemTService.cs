using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services.Changes;

namespace Umbraco.Core.Services.Implement
{
    public abstract class ContentTypeServiceBase<TRepository, TItem, TService> : ContentTypeServiceBase<TItem, TService>, IContentTypeBaseService<TItem>
        where TRepository : IContentTypeRepositoryBase<TItem>
        where TItem : class, IContentTypeComposition
        where TService : class, IContentTypeBaseService<TItem>
    {
        private readonly IAuditRepository _auditRepository;
        private readonly IEntityContainerRepository _containerRepository;
        private readonly IEntityRepository _entityRepository;

        protected ContentTypeServiceBase(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory,
            TRepository repository, IAuditRepository auditRepository, IEntityContainerRepository containerRepository, IEntityRepository entityRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            Repository = repository;
            _auditRepository = auditRepository;
            _containerRepository = containerRepository;
            _entityRepository = entityRepository;
        }

        protected TRepository Repository { get; }
        protected abstract int[] WriteLockIds { get; }
        protected abstract int[] ReadLockIds { get; }

        #region Validation

        public Attempt<string[]> ValidateComposition(TItem compo)
        {
            try
            {
                using (var scope = ScopeProvider.CreateScope(autoComplete: true))
                {
                    scope.ReadLock(ReadLockIds);
                    ValidateLocked(compo);
                }
                return Attempt<string[]>.Succeed();
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

            var allContentTypes = Repository.GetMany(new int[0]).Cast<IContentTypeComposition>().ToArray();

            var compositionAliases = compositionContentType.CompositionAliases();
            var compositions = allContentTypes.Where(x => compositionAliases.Any(y => x.Alias.Equals(y)));
            var propertyTypeAliases = compositionContentType.PropertyTypes.Select(x => x.Alias).ToArray();
            var propertyGroupAliases = compositionContentType.PropertyGroups.ToDictionary(x => x.Alias, x => x.Type, StringComparer.InvariantCultureIgnoreCase);
            var indirectReferences = allContentTypes.Where(x => x.ContentTypeComposition.Any(y => y.Id == compositionContentType.Id));
            var comparer = new DelegateEqualityComparer<IContentTypeComposition>((x, y) => x.Id == y.Id, x => x.Id);
            var dependencies = new HashSet<IContentTypeComposition>(compositions, comparer);
            var stack = new Stack<IContentTypeComposition>();
            foreach (var indirectReference in indirectReferences)
                stack.Push(indirectReference); // push indirect references to a stack, so we can add recursively
            while (stack.Count > 0)
            {
                var indirectReference = stack.Pop();
                dependencies.Add(indirectReference);

                // get all compositions for the current indirect reference
                var directReferences = indirectReference.ContentTypeComposition;
                foreach (var directReference in directReferences)
                {
                    if (directReference.Id == compositionContentType.Id || directReference.Alias.Equals(compositionContentType.Alias)) continue;
                    dependencies.Add(directReference);
                    // a direct reference has compositions of its own - these also need to be taken into account
                    var directReferenceGraph = directReference.CompositionAliases();
                    foreach (var c in allContentTypes.Where(x => directReferenceGraph.Any(y => x.Alias.Equals(y, StringComparison.InvariantCultureIgnoreCase))))
                        dependencies.Add(c);
                }

                // recursive lookup of indirect references
                foreach (var c in allContentTypes.Where(x => x.ContentTypeComposition.Any(y => y.Id == indirectReference.Id)))
                    stack.Push(c);
            }

            var duplicatePropertyTypeAliases = new List<string>();
            var invalidPropertyGroupAliases = new List<string>();

            foreach (var dependency in dependencies)
            {
                if (dependency.Id == compositionContentType.Id) continue;
                var contentTypeDependency = allContentTypes.FirstOrDefault(x => x.Alias.Equals(dependency.Alias, StringComparison.InvariantCultureIgnoreCase));
                if (contentTypeDependency == null) continue;

                duplicatePropertyTypeAliases.AddRange(contentTypeDependency.PropertyTypes.Select(x => x.Alias).Intersect(propertyTypeAliases, StringComparer.InvariantCultureIgnoreCase));
                invalidPropertyGroupAliases.AddRange(contentTypeDependency.PropertyGroups.Where(x => propertyGroupAliases.TryGetValue(x.Alias, out var type) && type != x.Type).Select(x => x.Alias));
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

            foreach (var contentType in contentTypes)
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
                    if (!(propertyType is IRememberBeingDirty dirtyProperty))
                        throw new Exception("oops");

                    // skip new properties
                    // TODO: This used to be WasPropertyDirty("HasIdentity") but i don't think that actually worked for detecting new entities this does seem to work properly
                    var isNewProperty = dirtyProperty.WasPropertyDirty("Id");
                    if (isNewProperty) return false;

                    // alias change?
                    return dirtyProperty.WasPropertyDirty("Alias");
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
                        foreach (var c in GetComposedOf(contentType.Id))
                            AddChange(changes, c, ContentTypeChangeTypes.RefreshMain);
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
            var change = changes.FirstOrDefault(x => x.Item == contentType);
            if (change == null)
            {
                changes.Add(new ContentTypeChange<TItem>(contentType, changeTypes));
                return;
            }
            change.ChangeTypes |= changeTypes;
        }

        #endregion

        #region Get, Has, Is, Count

        IContentTypeComposition IContentTypeBaseService.Get(int id)
        {
            return Get(id);
        }

        public TItem Get(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds);
                return Repository.Get(id);
            }
        }

        public TItem Get(string alias)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds);
                return Repository.Get(alias);
            }
        }

        public TItem Get(Guid id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds);
                return Repository.Get(id);
            }
        }

        public IEnumerable<TItem> GetAll(params int[] ids)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds);
                return Repository.GetMany(ids);
            }
        }

        public IEnumerable<TItem> GetAll(IEnumerable<Guid> ids)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds);
                return Repository.GetMany(ids.ToArray());
            }
        }

        public IEnumerable<TItem> GetChildren(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds);
                var query = Query<TItem>().Where(x => x.ParentId == id);
                return Repository.Get(query);
            }
        }

        public IEnumerable<TItem> GetChildren(Guid id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds);
                var found = Get(id);
                if (found == null) return Enumerable.Empty<TItem>();
                var query = Query<TItem>().Where(x => x.ParentId == found.Id);
                return Repository.Get(query);
            }
        }

        public bool HasChildren(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds);
                var query = Query<TItem>().Where(x => x.ParentId == id);
                var count = Repository.Count(query);
                return count > 0;
            }
        }

        public bool HasChildren(Guid id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds);
                var found = Get(id);
                if (found == null) return false;
                var query = Query<TItem>().Where(x => x.ParentId == found.Id);
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                // can use same repo for both content and media
                return Repository.HasContainerInPath(contentPath);
            }
        }

        public bool HasContainerInPath(params int[] ids)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                // can use same repo for both content and media
                return Repository.HasContainerInPath(ids);
            }
        }

        public IEnumerable<TItem> GetDescendants(int id, bool andSelf)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds);

                var descendants = new List<TItem>();
                if (andSelf) descendants.Add(Repository.Get(id));
                var ids = new Stack<int>();
                ids.Push(id);

                while (ids.Count > 0)
                {
                    var i = ids.Pop();
                    var query = Query<TItem>().Where(x => x.ParentId == i);
                    var result = Repository.Get(query).ToArray();

                    foreach (var c in result)
                    {
                        descendants.Add(c);
                        ids.Push(c.Id);
                    }
                }

                return descendants.ToArray();
            }
        }

        public IEnumerable<TItem> GetComposedOf(int id, IEnumerable<TItem> all)
        {
            return all.Where(x => x.ContentTypeComposition.Any(y => y.Id == id));

        }

        public IEnumerable<TItem> GetComposedOf(int id)
        {
            // GetAll is cheap, repository has a full dataset cache policy
            // TODO: still, because it uses the cache, race conditions!
            var allContentTypes = GetAll(Array.Empty<int>());
            return GetComposedOf(id, allContentTypes);
        }

        public int Count()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds);
                return Repository.Count(Query<TItem>());
            }
        }

        public bool HasContentNodes(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds);
                return Repository.HasContentNodes(id);
            }
        }

        #endregion

        #region Save

        public void Save(TItem item, int userId = Constants.Security.SuperUserId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<TItem>(item);
                if (OnSavingCancelled(scope, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                if (string.IsNullOrWhiteSpace(item.Name))
                    throw new ArgumentException("Cannot save item with empty name.");

                if (item.Name != null && item.Name.Length > 255)
                {
                    throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
                }

                scope.WriteLock(WriteLockIds);

                // validate the DAG transform, within the lock
                ValidateLocked(item); // throws if invalid

                item.CreatorId = userId;
                if (item.Description == string.Empty) item.Description = null;
                Repository.Save(item); // also updates content/media/member items

                // figure out impacted content types
                var changes = ComposeContentTypeChanges(item).ToArray();
                var args = changes.ToEventArgs();

                OnUowRefreshedEntity(args);

                OnChanged(scope, args);
                saveEventArgs.CanCancel = false;
                OnSaved(scope, saveEventArgs);

                Audit(AuditType.Save, userId, item.Id);
                scope.Complete();
            }
        }

        public void Save(IEnumerable<TItem> items, int userId = Constants.Security.SuperUserId)
        {
            var itemsA = items.ToArray();

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<TItem>(itemsA);
                if (OnSavingCancelled(scope, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                scope.WriteLock(WriteLockIds);

                // all-or-nothing, validate them all first
                foreach (var contentType in itemsA)
                {
                    ValidateLocked(contentType); // throws if invalid
                }
                foreach (var contentType in itemsA)
                {
                    contentType.CreatorId = userId;
                    if (contentType.Description == string.Empty) contentType.Description = null;
                    Repository.Save(contentType);
                }

                // figure out impacted content types
                var changes = ComposeContentTypeChanges(itemsA).ToArray();
                var args = changes.ToEventArgs();

                OnUowRefreshedEntity(args);

                OnChanged(scope, args);
                saveEventArgs.CanCancel = false;
                OnSaved(scope, saveEventArgs);

                Audit(AuditType.Save, userId, -1);
                scope.Complete();
            }
        }

        #endregion

        #region Delete

        public void Delete(TItem item, int userId = Constants.Security.SuperUserId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var deleteEventArgs = new DeleteEventArgs<TItem>(item);
                if (OnDeletingCancelled(scope, deleteEventArgs))
                {
                    scope.Complete();
                    return;
                }

                scope.WriteLock(WriteLockIds);

                // all descendants are going to be deleted
                var descendantsAndSelf = GetDescendants(item.Id, true)
                    .ToArray();
                var deleted = descendantsAndSelf;

                // all impacted (through composition) probably lose some properties
                // don't try to be too clever here, just report them all
                // do this before anything is deleted
                var changed = descendantsAndSelf.SelectMany(xx => GetComposedOf(xx.Id))
                    .Distinct()
                    .Except(descendantsAndSelf)
                    .ToArray();

                // delete content
                DeleteItemsOfTypes(descendantsAndSelf.Select(x => x.Id));
                
                // Next find all other document types that have a reference to this content type
                var referenceToAllowedContentTypes = GetAll().Where(q => q.AllowedContentTypes.Any(p=>p.Id.Value==item.Id));
                foreach (var reference in referenceToAllowedContentTypes)
                {                                        
                    reference.AllowedContentTypes = reference.AllowedContentTypes.Where(p => p.Id.Value != item.Id);                   
                    var changedRef = new List<ContentTypeChange<TItem>>() { new ContentTypeChange<TItem>(reference, ContentTypeChangeTypes.RefreshMain) };
                    // Fire change event
                    OnChanged(scope, changedRef.ToEventArgs());                  
                }

                // finally delete the content type
                // - recursively deletes all descendants
                // - deletes all associated property data
                //  (contents of any descendant type have been deleted but
                //   contents of any composed (impacted) type remain but
                //   need to have their property data cleared)
                Repository.Delete(item);                               

                //...
                var changes = descendantsAndSelf.Select(x => new ContentTypeChange<TItem>(x, ContentTypeChangeTypes.Remove))
                    .Concat(changed.Select(x => new ContentTypeChange<TItem>(x, ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther)));
                var args = changes.ToEventArgs();

                OnUowRefreshedEntity(args);

                OnChanged(scope, args);
                deleteEventArgs.DeletedEntities = deleted.DistinctBy(x => x.Id);
                deleteEventArgs.CanCancel = false;
                OnDeleted(scope, deleteEventArgs);

                Audit(AuditType.Delete, userId, item.Id);
                scope.Complete();
            }
        }

        public void Delete(IEnumerable<TItem> items, int userId = Constants.Security.SuperUserId)
        {
            var itemsA = items.ToArray();

            using (var scope = ScopeProvider.CreateScope())
            {
                var deleteEventArgs = new DeleteEventArgs<TItem>(itemsA);
                if (OnDeletingCancelled(scope, deleteEventArgs))
                {
                    scope.Complete();
                    return;
                }

                scope.WriteLock(WriteLockIds);

                // all descendants are going to be deleted
                var allDescendantsAndSelf = itemsA.SelectMany(xx => GetDescendants(xx.Id, true))
                    .DistinctBy(x => x.Id)
                    .ToArray();
                var deleted = allDescendantsAndSelf;

                // all impacted (through composition) probably lose some properties
                // don't try to be too clever here, just report them all
                // do this before anything is deleted
                var changed = allDescendantsAndSelf.SelectMany(x => GetComposedOf(x.Id))
                    .Distinct()
                    .Except(allDescendantsAndSelf)
                    .ToArray();

                // delete content
                DeleteItemsOfTypes(allDescendantsAndSelf.Select(x => x.Id));

                // finally delete the content types
                // (see notes in overload)
                foreach (var item in itemsA)
                    Repository.Delete(item);

                var changes = allDescendantsAndSelf.Select(x => new ContentTypeChange<TItem>(x, ContentTypeChangeTypes.Remove))
                    .Concat(changed.Select(x => new ContentTypeChange<TItem>(x, ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther)));
                var args = changes.ToEventArgs();

                OnUowRefreshedEntity(args);

                OnChanged(scope, args);
                deleteEventArgs.DeletedEntities = deleted.DistinctBy(x => x.Id);
                deleteEventArgs.CanCancel = false;
                OnDeleted(scope, deleteEventArgs);

                Audit(AuditType.Delete, userId, -1);
                scope.Complete();
            }
        }

        protected abstract void DeleteItemsOfTypes(IEnumerable<int> typeIds);

        #endregion

        #region Copy

        public TItem Copy(TItem original, string alias, string name, int parentId = -1)
        {
            TItem parent = null;
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

        public TItem Copy(TItem original, string alias, string name, TItem parent)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            if (alias == null) throw new ArgumentNullException(nameof(alias));
            if (string.IsNullOrWhiteSpace(alias)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(alias));
            if (parent != null && parent.HasIdentity == false) throw new InvalidOperationException("Parent must have an identity.");

            // this is illegal
            //var originalb = (ContentTypeCompositionBase)original;
            // but we *know* it has to be a ContentTypeCompositionBase anyways
            var originalb = (ContentTypeCompositionBase) (object) original;
            var clone = (TItem) (object) originalb.DeepCloneWithResetIdentities(alias);

            clone.Name = name;

            //remove all composition that is not it's current alias
            var compositionAliases = clone.CompositionAliases().Except(new[] { alias }).ToList();
            foreach (var a in compositionAliases)
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

        public Attempt<OperationResult<MoveOperationStatusType, TItem>> Copy(TItem copying, int containerId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            TItem copy;
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(WriteLockIds);

                try
                {
                    if (containerId > 0)
                    {
                        var container = _containerRepository.Get(containerId);
                        if (container == null)
                            throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound); // causes rollback
                    }
                    var alias = Repository.GetUniqueAlias(copying.Alias);

                    // this is illegal
                    //var copyingb = (ContentTypeCompositionBase) copying;
                    // but we *know* it has to be a ContentTypeCompositionBase anyways
                    var copyingb = (ContentTypeCompositionBase) (object)copying;
                    copy = (TItem) (object) copyingb.DeepCloneWithResetIdentities(alias);

                    copy.Name = copy.Name + " (copy)"; // might not be unique

                    // if it has a parent, and the parent is a content type, unplug composition
                    // all other compositions remain in place in the copied content type
                    if (copy.ParentId > 0)
                    {
                        var parent = Repository.Get(copy.ParentId);
                        if (parent != null)
                            copy.RemoveContentType(parent.Alias);
                    }

                    copy.ParentId = containerId;
                    Repository.Save(copy);
                    scope.Complete();
                }
                catch (DataOperationException<MoveOperationStatusType> ex)
                {
                    return OperationResult.Attempt.Fail<MoveOperationStatusType, TItem>(ex.Operation, evtMsgs); // causes rollback
                }
            }

            return OperationResult.Attempt.Succeed(MoveOperationStatusType.Success, evtMsgs, copy);
        }

        #endregion

        #region Move

        public Attempt<OperationResult<MoveOperationStatusType>> Move(TItem moving, int containerId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            var moveInfo = new List<MoveEventInfo<TItem>>();
            using (var scope = ScopeProvider.CreateScope())
            {
                var moveEventInfo = new MoveEventInfo<TItem>(moving, moving.Path, containerId);
                var moveEventArgs = new MoveEventArgs<TItem>(evtMsgs, moveEventInfo);
                if (OnMovingCancelled(scope, moveEventArgs))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Fail(MoveOperationStatusType.FailedCancelledByEvent, evtMsgs);
                }

                scope.WriteLock(WriteLockIds); // also for containers

                try
                {
                    EntityContainer container = null;
                    if (containerId > 0)
                    {
                        container = _containerRepository.Get(containerId);
                        if (container == null)
                            throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound); // causes rollback
                    }
                    moveInfo.AddRange(Repository.Move(moving, container));
                    scope.Complete();
                }
                catch (DataOperationException<MoveOperationStatusType> ex)
                {
                    scope.Complete();
                    return OperationResult.Attempt.Fail(ex.Operation, evtMsgs);
                }

                // note: not raising any Changed event here because moving a content type under another container
                // has no impact on the published content types - would be entirely different if we were to support
                // moving a content type under another content type.

                moveEventArgs.MoveInfoCollection = moveInfo;
                moveEventArgs.CanCancel = false;
                OnMoved(scope, moveEventArgs);
            }

            return OperationResult.Attempt.Succeed(MoveOperationStatusType.Success, evtMsgs);
        }

        #endregion

        #region Containers

        protected abstract Guid ContainedObjectType { get; }

        protected Guid ContainerObjectType => EntityContainer.GetContainerObjectType(ContainedObjectType);

        public Attempt<OperationResult<OperationResultType, EntityContainer>> CreateContainer(int parentId, string name, int userId = Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(WriteLockIds); // also for containers

                try
                {
                    var container = new EntityContainer(ContainedObjectType)
                    {
                        Name = name,
                        ParentId = parentId,
                        CreatorId = userId
                    };

                    var saveEventArgs = new SaveEventArgs<EntityContainer>(container, evtMsgs);
                    if (OnSavingContainerCancelled(scope, saveEventArgs))
                    {
                        scope.Complete();
                        return OperationResult.Attempt.Cancel(evtMsgs, container);
                    }

                    _containerRepository.Save(container);
                    scope.Complete();

                    saveEventArgs.CanCancel = false;
                    OnSavedContainer(scope, saveEventArgs);
                    // TODO: Audit trail ?

                    return OperationResult.Attempt.Succeed(evtMsgs, container);
                }
                catch (Exception ex)
                {
                    scope.Complete();
                    return OperationResult.Attempt.Fail<OperationResultType, EntityContainer>(OperationResultType.FailedCancelledByEvent, evtMsgs, ex);
                }
            }
        }

        public Attempt<OperationResult> SaveContainer(EntityContainer container, int userId = Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            var containerObjectType = ContainerObjectType;
            if (container.ContainerObjectType != containerObjectType)
            {
                var ex = new InvalidOperationException("Not a container of the proper type.");
                return OperationResult.Attempt.Fail(evtMsgs, ex);
            }

            if (container.HasIdentity && container.IsPropertyDirty("ParentId"))
            {
                var ex = new InvalidOperationException("Cannot save a container with a modified parent, move the container instead.");
                return OperationResult.Attempt.Fail(evtMsgs, ex);
            }

            using (var scope = ScopeProvider.CreateScope())
            {
                var args = new SaveEventArgs<EntityContainer>(container, evtMsgs);
                if (OnSavingContainerCancelled(scope, args))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs);
                }

                scope.WriteLock(WriteLockIds); // also for containers

                _containerRepository.Save(container);
                scope.Complete();

                args.CanCancel = false;
                OnSavedContainer(scope, args);
            }

            // TODO: Audit trail ?

            return OperationResult.Attempt.Succeed(evtMsgs);
        }

        public EntityContainer GetContainer(int containerId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds); // also for containers

                return _containerRepository.Get(containerId);
            }
        }

        public EntityContainer GetContainer(Guid containerId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds); // also for containers

                return ((EntityContainerRepository) _containerRepository).Get(containerId);
            }
        }

        public IEnumerable<EntityContainer> GetContainers(int[] containerIds)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds); // also for containers

                return _containerRepository.GetMany(containerIds);
            }
        }

        public IEnumerable<EntityContainer> GetContainers(TItem item)
        {
            var ancestorIds = item.Path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    var asInt = x.TryConvertTo<int>();
                    return asInt ? asInt.Result : int.MinValue;
                })
                .Where(x => x != int.MinValue && x != item.Id)
                .ToArray();

            return GetContainers(ancestorIds);
        }

        public IEnumerable<EntityContainer> GetContainers(string name, int level)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds); // also for containers

                return ((EntityContainerRepository) _containerRepository).Get(name, level);
            }
        }

        public Attempt<OperationResult> DeleteContainer(int containerId, int userId = Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(WriteLockIds); // also for containers

                var container = _containerRepository.Get(containerId);
                if (container == null) return OperationResult.Attempt.NoOperation(evtMsgs);

                // 'container' here does not know about its children, so we need
                // to get it again from the entity repository, as a light entity
                var entity = _entityRepository.Get(container.Id);
                if (entity.HasChildren)
                {
                    scope.Complete();
                    return Attempt.Fail(new OperationResult(OperationResultType.FailedCannot, evtMsgs));
                }

                var deleteEventArgs = new DeleteEventArgs<EntityContainer>(container, evtMsgs);
                if (OnDeletingContainerCancelled(scope, deleteEventArgs))
                {
                    scope.Complete();
                    return Attempt.Fail(new OperationResult(OperationResultType.FailedCancelledByEvent, evtMsgs));
                }

                _containerRepository.Delete(container);
                scope.Complete();

                deleteEventArgs.CanCancel = false;
                OnDeletedContainer(scope, deleteEventArgs);

                return OperationResult.Attempt.Succeed(evtMsgs);
                // TODO: Audit trail ?
            }
        }

        public Attempt<OperationResult<OperationResultType, EntityContainer>> RenameContainer(int id, string name, int userId = Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(WriteLockIds); // also for containers

                try
                {
                    var container = _containerRepository.Get(id);

                    //throw if null, this will be caught by the catch and a failed returned
                    if (container == null)
                        throw new InvalidOperationException("No container found with id " + id);

                    container.Name = name;

                    var saveEventArgs = new SaveEventArgs<EntityContainer>(container, evtMsgs);
                    if (OnRenamingContainerCancelled(scope, saveEventArgs))
                    {
                        scope.Complete();
                        return OperationResult.Attempt.Cancel<EntityContainer>(evtMsgs);
                    }

                    _containerRepository.Save(container);
                    scope.Complete();

                    saveEventArgs.CanCancel = false;
                    OnRenamedContainer(scope, saveEventArgs);

                    return OperationResult.Attempt.Succeed(OperationResultType.Success, evtMsgs, container);
                }
                catch (Exception ex)
                {
                    return OperationResult.Attempt.Fail<EntityContainer>(evtMsgs, ex);
                }
            }
        }

        #endregion

        #region Audit

        private void Audit(AuditType type, int userId, int objectId)
        {
            _auditRepository.Save(new AuditItem(objectId, type, userId,
                ObjectTypes.GetUmbracoObjectType(ContainedObjectType).GetName()));
        }

        #endregion


    }
}
