using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    internal abstract class ContentTypeServiceBase : RepositoryService
    {
        protected ContentTypeServiceBase(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        { }

        /// <summary>
        /// This is called after an content type is saved and is used to update the content xml structures in the database
        /// if they are required to be updated.
        /// </summary>
        /// <param name="contentTypes"></param>
        internal IEnumerable<IContentTypeBase> GetContentTypesForXmlUpdates(params IContentTypeBase[] contentTypes)
        {

            var toUpdate = new List<IContentTypeBase>();

            foreach (var contentType in contentTypes)
            {
                //we need to determine if we need to refresh the xml content in the database. This is to be done when:
                // - the item is not new (already existed in the db) AND
                //      - a content type changes it's alias OR
                //      - if a content type has it's property removed OR
                //      - if a content type has a property whose alias has changed
                //here we need to check if the alias of the content type changed or if one of the properties was removed.
                var dirty = contentType as IRememberBeingDirty;
                if (dirty == null) continue;

                //check if any property types have changed their aliases (and not new property types)
                var hasAnyPropertiesChangedAlias = contentType.PropertyTypes.Any(propType =>
                    {
                        var dirtyProperty = propType as IRememberBeingDirty;
                        if (dirtyProperty == null) return false;
                        return dirtyProperty.WasPropertyDirty("HasIdentity") == false   //ensure it's not 'new'
                               && dirtyProperty.WasPropertyDirty("Alias");              //alias has changed
                    });

                if (dirty.WasPropertyDirty("HasIdentity") == false //ensure it's not 'new'
                    && (dirty.WasPropertyDirty("Alias") || dirty.WasPropertyDirty("HasPropertyTypeBeenRemoved") || hasAnyPropertiesChangedAlias))
                {
                    //If the alias was changed then we only need to update the xml structures for content of the current content type.
                    //If a property was deleted or a property alias was changed then we need to update the xml structures for any
                    // content of the current content type and any of the content type's child content types.
                    if (dirty.WasPropertyDirty("Alias")
                        && dirty.WasPropertyDirty("HasPropertyTypeBeenRemoved") == false && hasAnyPropertiesChangedAlias == false)
                    {
                        //if only the alias changed then only update the current content type
                        toUpdate.Add(contentType);
                    }
                    else
                    {
                        //TODO: This is pretty nasty, fix this
                        var contentTypeService = this as IContentTypeService;
                        if (contentTypeService != null)
                        {
                            //if a property was deleted or alias changed, then update all content of the current content type
                            // and all of it's desscendant doc types.
                            toUpdate.AddRange(((IContentType) contentType).DescendantsAndSelf(contentTypeService));
                        }
                        else
                        {
                            var mediaTypeService = this as IMediaTypeService;
                            if (mediaTypeService != null)
                            {
                                //if a property was deleted or alias changed, then update all content of the current content type
                                // and all of it's desscendant doc types.
                                toUpdate.AddRange(((IMediaType) contentType).DescendantsAndSelf(mediaTypeService));
                            }
                            else
                            {
                                var memberTypeService = this as IMemberTypeService;
                                if (memberTypeService != null)
                                {
                                    //if a property was deleted or alias changed, then update all content of the current content type
                                    // and all of it's desscendant doc types.
                                    toUpdate.AddRange(((IMemberType)contentType).DescendantsAndSelf(memberTypeService));
                                }
                            }
                        }
                        
                    }
                }
            }

            return toUpdate;

        }
    }

    internal abstract class ContentTypeServiceBase<TItem, TService> : ContentTypeServiceBase
        where TItem : class, IContentTypeComposition
        where TService : class, IContentTypeServiceBase<TItem>
    {
        protected ContentTypeServiceBase(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        {
            _this = this as TService;
            if (_this == null) throw new Exception("Oops.");
        }

        private readonly TService _this;

        public static event TypedEventHandler<TService, SaveEventArgs<TItem>> Saving;
        public static event TypedEventHandler<TService, SaveEventArgs<TItem>> Saved;

        protected void OnSaving(SaveEventArgs<TItem> args)
        {
            Saving.RaiseEvent(args, _this);
        }

        protected bool OnSavingCancelled(SaveEventArgs<TItem> args)
        {
            return Saving.IsRaisedEventCancelled(args, _this);
        }

        protected void OnSaved(SaveEventArgs<TItem> args)
        {
            Saved.RaiseEvent(args, _this);
        }

        public static event TypedEventHandler<TService, DeleteEventArgs<TItem>> Deleting;
        public static event TypedEventHandler<TService, DeleteEventArgs<TItem>> Deleted;

        protected void OnDeleting(DeleteEventArgs<TItem> args)
        {
            Deleting.RaiseEvent(args, _this);
        }

        protected bool OnDeletingCancelled(DeleteEventArgs<TItem> args)
        {
            return Deleting.IsRaisedEventCancelled(args, _this);
        }

        protected void OnDeleted(DeleteEventArgs<TItem> args)
        {
            Deleted.RaiseEvent(args, (TService)(object)this);
        }

        public static event TypedEventHandler<TService, MoveEventArgs<TItem>> Moving;
        public static event TypedEventHandler<TService, MoveEventArgs<TItem>> Moved;

        protected void OnMoving(MoveEventArgs<TItem> args)
        {
            Moving.RaiseEvent(args, _this);
        }

        protected bool OnMovingCancelled(MoveEventArgs<TItem> args)
        {
            return Moving.IsRaisedEventCancelled(args, _this);
        }

        protected void OnMoved(MoveEventArgs<TItem> args)
        {
            Moved.RaiseEvent(args, _this);
        }

        public static event TypedEventHandler<TService, SaveEventArgs<EntityContainer>> SavingContainer;
        public static event TypedEventHandler<TService, SaveEventArgs<EntityContainer>> SavedContainer;

        protected void OnSavingContainer(SaveEventArgs<EntityContainer> args)
        {
            SavingContainer.RaiseEvent(args, _this);
        }

        protected bool OnSavingContainerCancelled(SaveEventArgs<EntityContainer> args)
        {
            return SavingContainer.IsRaisedEventCancelled(args, _this);
        }

        protected void OnSavedContainer(SaveEventArgs<EntityContainer> args)
        {
            SavedContainer.RaiseEvent(args, _this);
        }

        public static event TypedEventHandler<TService, DeleteEventArgs<EntityContainer>> DeletingContainer;
        public static event TypedEventHandler<TService, DeleteEventArgs<EntityContainer>> DeletedContainer;

        protected void OnDeletingContainer(DeleteEventArgs<EntityContainer> args)
        {
            DeletingContainer.RaiseEvent(args, _this);
        }

        protected bool OnDeletingContainerCancelled(DeleteEventArgs<EntityContainer> args)
        {
            return DeletingContainer.IsRaisedEventCancelled(args, _this);
        }

        protected void OnDeletedContainer(DeleteEventArgs<EntityContainer> args)
        {
            DeletedContainer.RaiseEvent(args, _this);
        }

        // for later usage
        //public static event TypedEventHandler<TService, Change.EventArgs> TxRefreshed;

        //protected void OnTxRefreshed(Change.EventArgs args)
        //{
        //    TxRefreshed.RaiseEvent(args, this);
        //}
    }

    internal abstract class ContentTypeServiceBase<TRepository, TItem, TService> : ContentTypeServiceBase<TItem, TService>, IContentTypeServiceBase<TItem>
        where TRepository : IContentTypeRepositoryBase<TItem>
        where TItem : class, IContentTypeComposition
        where TService : class, IContentTypeServiceBase<TItem>
    {
        protected ContentTypeServiceBase(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        { }

        protected abstract int[] WriteLockIds { get; }
        protected abstract int[] ReadLockIds { get; }

        #region Validation

        public Attempt<string[]> ValidateComposition(TItem compo)
        {
            try
            {
                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repo = uow.CreateRepository<TRepository>();
                    uow.ReadLock(ReadLockIds);
                    ValidateLocked(repo, compo);
                    uow.Complete();
                }
                return Attempt<string[]>.Succeed();
            }
            catch (InvalidCompositionException ex)
            {
                return Attempt.Fail(ex.PropertyTypeAliases, ex);
            }
        }

        protected void ValidateLocked(TRepository repository, TItem compositionContentType)
        {
            // performs business-level validation of the composition
            // should ensure that it is absolutely safe to save the composition

            // eg maybe a property has been added, with an alias that's OK (no conflict with ancestors)
            // but that cannot be used (conflict with descendants)

            var allContentTypes = repository.GetAll(new int[0]).Cast<IContentTypeComposition>().ToArray();

            var compositionAliases = compositionContentType.CompositionAliases();
            var compositions = allContentTypes.Where(x => compositionAliases.Any(y => x.Alias.Equals(y)));
            var propertyTypeAliases = compositionContentType.PropertyTypes.Select(x => x.Alias.ToLowerInvariant()).ToArray();
            var indirectReferences = allContentTypes.Where(x => x.ContentTypeComposition.Any(y => y.Id == compositionContentType.Id));
            var comparer = new DelegateEqualityComparer<IContentTypeComposition>((x, y) => x.Id == y.Id, x => x.Id);
            var dependencies = new HashSet<IContentTypeComposition>(compositions, comparer);
            var stack = new Stack<IContentTypeComposition>();
            indirectReferences.ForEach(stack.Push); // push indirect references to a stack, so we can add recursively
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
                    allContentTypes.Where(x => directReferenceGraph.Any(y => x.Alias.Equals(y, StringComparison.InvariantCultureIgnoreCase))).ForEach(c => dependencies.Add(c));
                }
                // recursive lookup of indirect references
                allContentTypes.Where(x => x.ContentTypeComposition.Any(y => y.Id == indirectReference.Id)).ForEach(stack.Push);
            }

            foreach (var dependency in dependencies)
            {
                if (dependency.Id == compositionContentType.Id) continue;
                var contentTypeDependency = allContentTypes.FirstOrDefault(x => x.Alias.Equals(dependency.Alias, StringComparison.InvariantCultureIgnoreCase));
                if (contentTypeDependency == null) continue;
                var intersect = contentTypeDependency.PropertyTypes.Select(x => x.Alias.ToLowerInvariant()).Intersect(propertyTypeAliases).ToArray();
                if (intersect.Length == 0) continue;

                throw new InvalidCompositionException(compositionContentType.Alias, intersect.ToArray());
            }
        }

        #endregion

        #region Composition
        #endregion

        #region Get, Has, Is, Count

        public TItem Get(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<TRepository>();
                uow.ReadLock(ReadLockIds);
                var item = repo.Get(id);
                uow.Complete();
                return item;
            }
        }

        public TItem Get(string alias)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<TRepository>();
                uow.ReadLock(ReadLockIds);
                var item = repo.Get(alias);
                uow.Complete();
                return item;
            }
        }

        public TItem Get(Guid id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<TRepository>();
                uow.ReadLock(ReadLockIds);
                var item = repo.Get(id);
                uow.Complete();
                return item;
            }
        }

        public IEnumerable<TItem> GetAll(params int[] ids)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<TRepository>();
                uow.ReadLock(ReadLockIds);
                var items = repo.GetAll(ids);
                uow.Complete();
                return items;
            }
        }

        public IEnumerable<TItem> GetAll(params Guid[] ids)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<TRepository>();
                uow.ReadLock(ReadLockIds);
                // IReadRepository<Guid, TEntity> is explicitely implemented, need to cast the repo
                var items = ((IReadRepository<Guid, TItem>) repo).GetAll(ids);
                uow.Complete();
                return items;
            }
        }

        public IEnumerable<TItem> GetChildren(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<TRepository>();
                uow.ReadLock(ReadLockIds);
                var query = repo.Query.Where(x => x.ParentId == id);
                var items = repo.GetByQuery(query);
                uow.Complete();
                return items;
            }
        }

        public IEnumerable<TItem> GetChildren(Guid id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<TRepository>();
                uow.ReadLock(ReadLockIds);
                var found = Get(id);
                if (found == null) return Enumerable.Empty<TItem>();
                var query = repo.Query.Where(x => x.ParentId == found.Id);
                var items = repo.GetByQuery(query);
                uow.Complete();
                return items;
            }
        }

        public bool HasChildren(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<TRepository>();
                uow.ReadLock(ReadLockIds);
                var query = repo.Query.Where(x => x.ParentId == id);
                var count = repo.Count(query);
                uow.Complete();
                return count > 0;
            }
        }

        public bool HasChildren(Guid id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<TRepository>();
                uow.ReadLock(ReadLockIds);
                var found = Get(id);
                if (found == null) return false;
                var query = repo.Query.Where(x => x.ParentId == found.Id);
                var count = repo.Count(query);
                uow.Complete();
                return count > 0;
            }
        }

        public IEnumerable<TItem> GetDescendants(int id, bool andSelf)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<TRepository>();
                uow.ReadLock(ReadLockIds);

                var descendants = new List<TItem>();
                if (andSelf) descendants.Add(repo.Get(id));
                var ids = new Stack<int>();
                ids.Push(id);

                while (ids.Count > 0)
                {
                    var i = ids.Pop();
                    var query = repo.Query.Where(x => x.ParentId == i);
                    var result = repo.GetByQuery(query).ToArray();

                    foreach (var c in result)
                    {
                        descendants.Add(c);
                        ids.Push(c.Id);
                    }
                }

                var descendantsA = descendants.ToArray();
                uow.Complete();
                return descendantsA;
            }
        }

        public IEnumerable<TItem> GetComposedOf(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<TRepository>();
                uow.ReadLock(ReadLockIds);

                // hash set handles duplicates
                var composed = new HashSet<TItem>(new DelegateEqualityComparer<TItem>(
                    (x, y) => x.Id == y.Id,
                    x => x.Id.GetHashCode()));

                var ids = new Stack<int>();
                ids.Push(id);

                while (ids.Count > 0)
                {
                    var i = ids.Pop();
                    var result = repo.GetTypesDirectlyComposedOf(i).ToArray();

                    foreach (var c in result)
                    {
                        composed.Add(c);
                        ids.Push(c.Id);
                    }
                }

                var composedA = composed.ToArray();
                uow.Complete();
                return composedA;
            }
        }

        public int Count()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<TRepository>();
                uow.ReadLock(ReadLockIds);
                var count = repo.Count(repo.Query);
                uow.Complete();
                return count;
            }
        }

        #endregion

        #region Save

        public void Save(TItem item, int userId = 0)
        {
            if (OnSavingCancelled(new SaveEventArgs<TItem>(item)))
                return;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<TRepository>();
                uow.WriteLock(WriteLockIds);

                // validate the DAG transform, within the lock
                ValidateLocked(repo, item); // throws if invalid

                item.CreatorId = userId;
                repo.AddOrUpdate(item); // also updates content/media/member items
                uow.Flush(); // to db but no commit yet

                // ...

                uow.Complete();
            }

            // todo: should use TxRefreshed event within the transaction instead, see CC branch
            UpdateContentXmlStructure(item);

            OnSaved(new SaveEventArgs<TItem>(item, false));
            Audit(AuditType.Save, $"Save {typeof(TItem).Name} performed by user", userId, item.Id);
        }

        public void Save(IEnumerable<TItem> items, int userId = 0)
        {
            var itemsA = items.ToArray();

            if (OnSavingCancelled(new SaveEventArgs<TItem>(itemsA)))
                return;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<TRepository>();
                uow.WriteLock(WriteLockIds);

                // all-or-nothing, validate them all first
                foreach (var contentType in itemsA)
                {
                    ValidateLocked(repo, contentType); // throws if invalid
                }
                foreach (var contentType in itemsA)
                {
                    contentType.CreatorId = userId;
                    repo.AddOrUpdate(contentType);
                }

                //save it all in one go
                uow.Complete();
            }

            // todo: should use TxRefreshed event within the transaction instead, see CC branch
            UpdateContentXmlStructure(itemsA.Cast<IContentTypeBase>().ToArray());

            OnSaved(new SaveEventArgs<TItem>(itemsA, false));
            Audit(AuditType.Save, $"Save {typeof(TItem).Name} performed by user", userId, -1);
        }

        #endregion

        #region Delete

        public void Delete(TItem item, int userId = 0)
        {
            if (OnDeletingCancelled(new DeleteEventArgs<TItem>(item)))
                return;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<TRepository>();
                uow.WriteLock(WriteLockIds);

                // all descendants are going to be deleted
                var descendantsAndSelf = item.DescendantsAndSelf(this)
                    .ToArray();

                // delete content
                DeleteItemsOfTypes(descendantsAndSelf.Select(x => x.Id));

                // finally delete the content type
                // - recursively deletes all descendants
                // - deletes all associated property data
                //  (contents of any descendant type have been deleted but
                //   contents of any composed (impacted) type remain but
                //   need to have their property data cleared)
                repo.Delete(item);
                uow.Flush(); // to db but no commit yet

                //...

                uow.Complete();
            }

            OnDeleted(new DeleteEventArgs<TItem>(item, false));
            Audit(AuditType.Delete, $"Delete {typeof(TItem).Name} performed by user", userId, item.Id);
        }

        public void Delete(IEnumerable<TItem> items, int userId = 0)
        {
            var itemsA = items.ToArray();

            if (OnDeletingCancelled(new DeleteEventArgs<TItem>(itemsA)))
                return;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<TRepository>();
                uow.WriteLock(WriteLockIds);

                // all descendants are going to be deleted
                var allDescendantsAndSelf = itemsA.SelectMany(xx => xx.DescendantsAndSelf(this))
                    .Distinct()
                    .ToArray();

                // delete content
                DeleteItemsOfTypes(allDescendantsAndSelf.Select(x => x.Id));

                // finally delete the content types
                // (see notes in overload)
                foreach (var item in itemsA)
                    repo.Delete(item);

                uow.Flush(); // to db but no commit yet

                // ...

                uow.Complete();

            }

            OnDeleted(new DeleteEventArgs<TItem>(itemsA, false));
            Audit(AuditType.Delete, $"Delete {typeof(TItem).Name} performed by user", userId, -1);
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
            Mandate.ParameterNotNull(original, "original");
            Mandate.ParameterNotNullOrEmpty(alias, "alias");

            if (parent != null)
                Mandate.That(parent.HasIdentity, () => new InvalidOperationException("The parent must have an identity"));

            // this is illegal
            //var originalb = (ContentTypeCompositionBase)original;
            // but we *know* it has to be a ContentTypeCompositionBase anyways
            var originalb = (ContentTypeCompositionBase) (object) original;
            var clone = (TItem) originalb.DeepCloneWithResetIdentities(alias);

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

        public Attempt<OperationStatus<MoveOperationStatusType, TItem>> Copy(TItem copying, int containerId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            TItem copy;
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<TRepository>();
                uow.WriteLock(WriteLockIds);

                var containerRepository = uow.CreateContainerRepository(ContainerObjectType);
                try
                {
                    if (containerId > 0)
                    {
                        var container = containerRepository.Get(containerId);
                        if (container == null)
                            throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound); // causes rollback
                    }
                    var alias = repo.GetUniqueAlias(copying.Alias);

                    // this is illegal
                    //var copyingb = (ContentTypeCompositionBase) copying;
                    // but we *know* it has to be a ContentTypeCompositionBase anyways
                    var copyingb = (ContentTypeCompositionBase) (object)copying;
                    copy = (TItem) copyingb.DeepCloneWithResetIdentities(alias);

                    copy.Name = copy.Name + " (copy)"; // might not be unique

                    // if it has a parent, and the parent is a content type, unplug composition
                    // all other compositions remain in place in the copied content type
                    if (copy.ParentId > 0)
                    {
                        var parent = repo.Get(copy.ParentId);
                        if (parent != null)
                            copy.RemoveContentType(parent.Alias);
                    }

                    copy.ParentId = containerId;
                    repo.AddOrUpdate(copy);
                    uow.Complete();
                }
                catch (DataOperationException<MoveOperationStatusType> ex)
                {
                    return OperationStatus.Attempt.Fail<MoveOperationStatusType, TItem>(ex.Operation, evtMsgs); // causes rollback
                }
            }

            return OperationStatus.Attempt.Succeed(MoveOperationStatusType.Success, evtMsgs, copy);
        }

        #endregion

        #region Move

        public Attempt<OperationStatus<MoveOperationStatusType>> Move(TItem moving, int containerId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            if (OnMovingCancelled(new MoveEventArgs<TItem>(evtMsgs, new MoveEventInfo<TItem>(moving, moving.Path, containerId))))
                return OperationStatus.Attempt.Fail(MoveOperationStatusType.FailedCancelledByEvent, evtMsgs);

            var moveInfo = new List<MoveEventInfo<TItem>>();
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(WriteLockIds); // also for containers

                var repo = uow.CreateRepository<TRepository>();
                var containerRepo = uow.CreateRepository<IDocumentTypeContainerRepository>();

                try
                {
                    EntityContainer container = null;
                    if (containerId > 0)
                    {
                        container = containerRepo.Get(containerId);
                        if (container == null)
                            throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound); // causes rollback
                    }
                    moveInfo.AddRange(repo.Move(moving, container));
                    uow.Complete();
                }
                catch (DataOperationException<MoveOperationStatusType> ex)
                {
                    return OperationStatus.Attempt.Fail(ex.Operation, evtMsgs); // causes rollback
                }
            }

            OnMoved(new MoveEventArgs<TItem>(false, evtMsgs, moveInfo.ToArray()));

            return OperationStatus.Attempt.Succeed(MoveOperationStatusType.Success, evtMsgs);
        }

        #endregion

        #region Containers

        protected abstract Guid ContainedObjectType { get; }

        protected Guid ContainerObjectType => EntityContainer.GetContainerObjectType(ContainedObjectType);

        public Attempt<OperationStatus<OperationStatusType, EntityContainer>> CreateContainer(int parentId, string name, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(WriteLockIds); // also for containers

                var repo = uow.CreateContainerRepository(ContainerObjectType);
                try
                {
                    var container = new EntityContainer(Constants.ObjectTypes.DocumentTypeGuid)
                    {
                        Name = name,
                        ParentId = parentId,
                        CreatorId = userId
                    };

                    if (OnSavingContainerCancelled(new SaveEventArgs<EntityContainer>(container, evtMsgs)))
                        return OperationStatus.Attempt.Cancel(evtMsgs, container); // causes rollback

                    repo.AddOrUpdate(container);
                    uow.Complete();

                    OnSavedContainer(new SaveEventArgs<EntityContainer>(container, evtMsgs));
                    //TODO: Audit trail ?

                    return OperationStatus.Attempt.Succeed(evtMsgs, container);
                }
                catch (Exception ex)
                {
                    return OperationStatus.Attempt.Fail<OperationStatusType, EntityContainer>(OperationStatusType.FailedCancelledByEvent, evtMsgs, ex);
                }
            }
        }

        public Attempt<OperationStatus> SaveContainer(EntityContainer container, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();

            var containerObjectType = ContainerObjectType;
            if (container.ContainedObjectType != containerObjectType)
            {
                var ex = new InvalidOperationException("Not a container of the proper type.");
                return OperationStatus.Attempt.Fail(evtMsgs, ex);
            }

            if (container.HasIdentity && container.IsPropertyDirty("ParentId"))
            {
                var ex = new InvalidOperationException("Cannot save a container with a modified parent, move the container instead.");
                return OperationStatus.Attempt.Fail(evtMsgs, ex);
            }

            if (OnSavingContainerCancelled(new SaveEventArgs<EntityContainer>(container, evtMsgs)))
                return OperationStatus.Attempt.Cancel(evtMsgs);

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(WriteLockIds); // also for containers

                var repo = uow.CreateContainerRepository(containerObjectType);
                repo.AddOrUpdate(container);
                uow.Complete();
            }

            OnSavedContainer(new SaveEventArgs<EntityContainer>(container, evtMsgs));

            //TODO: Audit trail ?

            return OperationStatus.Attempt.Succeed(evtMsgs);
        }

        public EntityContainer GetContainer(int containerId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(ReadLockIds); // also for containers

                var repo = uow.CreateContainerRepository(ContainerObjectType);
                var container = repo.Get(containerId);
                uow.Complete();
                return container;
            }
        }

        public EntityContainer GetContainer(Guid containerId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(ReadLockIds); // also for containers

                var repo = uow.CreateContainerRepository(ContainerObjectType);
                var container = ((EntityContainerRepository) repo).Get(containerId);
                uow.Complete();
                return container;
            }
        }

        public IEnumerable<EntityContainer> GetContainers(int[] containerIds)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(ReadLockIds); // also for containers

                var repo = uow.CreateContainerRepository(ContainerObjectType);
                var containers = repo.GetAll(containerIds);
                uow.Complete();
                return containers;
            }
        }

        public IEnumerable<EntityContainer> GetContainers(TItem item)
        {
            var ancestorIds = item.Path.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(ReadLockIds); // also for containers

                var repo = uow.CreateContainerRepository(ContainerObjectType);
                var containers = ((EntityContainerRepository) repo).Get(name, level);
                uow.Complete();
                return containers;
            }
        }

        // fixme - what happens if deleting a non-empty container?
        public Attempt<OperationStatus> DeleteContainer(int containerId, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(WriteLockIds); // also for containers

                var repo = uow.CreateContainerRepository(ContainerObjectType);
                var container = repo.Get(containerId);
                if (container == null) return OperationStatus.Attempt.NoOperation(evtMsgs);

                if (OnDeletingContainerCancelled(new DeleteEventArgs<EntityContainer>(container, evtMsgs)))
                    return Attempt.Fail(new OperationStatus(OperationStatusType.FailedCancelledByEvent, evtMsgs)); // causes rollback

                repo.Delete(container);
                uow.Complete();

                OnDeletedContainer(new DeleteEventArgs<EntityContainer>(container, evtMsgs));

                return OperationStatus.Attempt.Succeed(evtMsgs);
                //TODO: Audit trail ?
            }
        }

        #endregion

        #region Audit

        private void Audit(AuditType type, string message, int userId, int objectId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IAuditRepository>();
                repo.AddOrUpdate(new AuditItem(objectId, message, type, userId));
                uow.Complete();
            }
        }

        #endregion

        #region Xml - Should Move!

        protected abstract void UpdateContentXmlStructure(params IContentTypeBase[] contentTypes);

        #endregion
    }
}