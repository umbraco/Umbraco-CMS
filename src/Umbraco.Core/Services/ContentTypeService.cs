using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the ContentType Service, which is an easy access to operations involving <see cref="IContentType"/>
    /// </summary>
    public class ContentTypeService : ContentTypeServiceBase, IContentTypeService
    {
        private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;

        //Support recursive locks because some of the methods that require locking call other methods that require locking.
        //for example, the Move method needs to be locked but this calls the Save method which also needs to be locked.
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public ContentTypeService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory, IContentService contentService,
            IMediaService mediaService)
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        {
            if (contentService == null) throw new ArgumentNullException("contentService");
            if (mediaService == null) throw new ArgumentNullException("mediaService");
            _contentService = contentService;
            _mediaService = mediaService;
        }

        #region Containers

        public Attempt<OperationStatus<EntityContainer, OperationStatusType>> CreateContentTypeContainer(int parentId, string name, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.DocumentTypeContainerGuid);

                try
                {
                    var container = new EntityContainer(Constants.ObjectTypes.DocumentTypeGuid)
                    {
                        Name = name,
                        ParentId = parentId,
                        CreatorId = userId
                    };

                    var saveEventArgs = new SaveEventArgs<EntityContainer>(container, evtMsgs);
                    if (uow.Events.DispatchCancelable(SavingContentTypeContainer, this, saveEventArgs))
                    {
                        uow.Commit();
                        return Attempt.Fail(new OperationStatus<EntityContainer, OperationStatusType>(container, OperationStatusType.FailedCancelledByEvent, evtMsgs));
                    }

                    repo.AddOrUpdate(container);
                    uow.Commit();
                    saveEventArgs.CanCancel = false;
                    uow.Events.Dispatch(SavedContentTypeContainer, this, saveEventArgs, "SavedContentTypeContainer");
                    //TODO: Audit trail ?

                    return Attempt.Succeed(new OperationStatus<EntityContainer, OperationStatusType>(container, OperationStatusType.Success, evtMsgs));
                }
                catch (Exception ex)
                {
                    uow.Commit();
                    return Attempt.Fail(new OperationStatus<EntityContainer, OperationStatusType>(null, OperationStatusType.FailedExceptionThrown, evtMsgs), ex);
                }
            }
        }

        public Attempt<OperationStatus<EntityContainer, OperationStatusType>> RenameContentTypeContainer(int id, string name, int userId = 0)
        {
            return RenameTypeContainer(id, name, Constants.ObjectTypes.DocumentTypeContainerGuid);
        }

        private Attempt<OperationStatus<EntityContainer, OperationStatusType>> RenameTypeContainer(int id, string name, Guid typeCode)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateEntityContainerRepository(uow, typeCode))
            {
                try
                {
                    var container = repo.Get(id);

                    //throw if null, this will be caught by the catch and a failed returned
                    if (container == null)
                        throw new InvalidOperationException("No container found with id " + id);

                    container.Name = name;

                    repo.AddOrUpdate(container);
                    uow.Commit();

                    uow.Events.Dispatch(SavedContentTypeContainer, this, new SaveEventArgs<EntityContainer>(container, evtMsgs), "RenamedContainer");

                    return Attempt.Succeed(new OperationStatus<EntityContainer, OperationStatusType>(container, OperationStatusType.Success, evtMsgs));
                }
                catch (Exception ex)
                {
                    return Attempt.Fail(new OperationStatus<EntityContainer, OperationStatusType>(null, OperationStatusType.FailedExceptionThrown, evtMsgs), ex);
                }
            }
        }

        public Attempt<OperationStatus<EntityContainer, OperationStatusType>> CreateMediaTypeContainer(int parentId, string name, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.MediaTypeContainerGuid);

                try
                {
                    var container = new EntityContainer(Constants.ObjectTypes.MediaTypeGuid)
                    {
                        Name = name,
                        ParentId = parentId,
                        CreatorId = userId
                    };

                    var saveEventArgs = new SaveEventArgs<EntityContainer>(container, evtMsgs);
                    if (uow.Events.DispatchCancelable(SavingMediaTypeContainer, this, saveEventArgs))
                    {
                        uow.Commit();
                        return Attempt.Fail(new OperationStatus<EntityContainer, OperationStatusType>(container, OperationStatusType.FailedCancelledByEvent, evtMsgs));
                    }

                    repo.AddOrUpdate(container);
                    uow.Commit();
                    saveEventArgs.CanCancel = false;
                    uow.Events.Dispatch(SavedMediaTypeContainer, this, saveEventArgs, "SavedMediaTypeContainer");
                    //TODO: Audit trail ?

                    return Attempt.Succeed(new OperationStatus<EntityContainer, OperationStatusType>(container, OperationStatusType.Success, evtMsgs));
                }
                catch (Exception ex)
                {
                    uow.Commit();
                    return Attempt.Fail(new OperationStatus<EntityContainer, OperationStatusType>(null, OperationStatusType.FailedExceptionThrown, evtMsgs), ex);
                }
            }
        }

        public Attempt<OperationStatus<EntityContainer, OperationStatusType>> RenameMediaTypeContainer(int id, string name, int userId = 0)
        {
            return RenameTypeContainer(id, name, Constants.ObjectTypes.MediaTypeContainerGuid);
        }

        public Attempt<OperationStatus<EntityContainer, OperationStatusType>> RenameDataTypeContainer(int id, string name, int userId = 0)
        {
            return RenameTypeContainer(id, name, Constants.ObjectTypes.DataTypeContainerGuid);
        }

        public Attempt<OperationStatus> SaveContentTypeContainer(EntityContainer container, int userId = 0)
        {
            return SaveContainer(
                SavingContentTypeContainer, SavedContentTypeContainer,
                container, Constants.ObjectTypes.DocumentTypeContainerGuid, "document type",
                "SavedContentTypeContainer", userId);
        }

        public Attempt<OperationStatus> SaveMediaTypeContainer(EntityContainer container, int userId = 0)
        {
            return SaveContainer(
                SavingMediaTypeContainer, SavedMediaTypeContainer,
                container, Constants.ObjectTypes.MediaTypeContainerGuid, "media type",
                "SavedMediaTypeContainer", userId);
        }

        private Attempt<OperationStatus> SaveContainer(
            TypedEventHandler<IContentTypeService, SaveEventArgs<EntityContainer>> savingEvent,
            TypedEventHandler<IContentTypeService, SaveEventArgs<EntityContainer>> savedEvent,
            EntityContainer container,
            Guid containerObjectType,
            string objectTypeName,
            string savedEventName,
            int userId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            if (container.ContainerObjectType != containerObjectType)
            {
                var ex = new InvalidOperationException("Not a " + objectTypeName + " container.");
                return OperationStatus.Exception(evtMsgs, ex);
            }

            if (container.HasIdentity && container.IsPropertyDirty("ParentId"))
            {
                var ex = new InvalidOperationException("Cannot save a container with a modified parent, move the container instead.");
                return OperationStatus.Exception(evtMsgs, ex);
            }

            using (var uow = UowProvider.GetUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(savingEvent, this, new SaveEventArgs<EntityContainer>(container, evtMsgs)))
                {
                    uow.Commit();
                    return OperationStatus.Cancelled(evtMsgs);
                }

                var repo = RepositoryFactory.CreateEntityContainerRepository(uow, containerObjectType);
                repo.AddOrUpdate(container);
                uow.Commit();
                uow.Events.Dispatch(savedEvent, this, new SaveEventArgs<EntityContainer>(container, evtMsgs), savedEventName);
            }

            //TODO: Audit trail ?

            return OperationStatus.Success(evtMsgs);
        }

        public EntityContainer GetContentTypeContainer(int containerId)
        {
            return GetContainer(containerId, Constants.ObjectTypes.DocumentTypeContainerGuid);
        }

        public EntityContainer GetMediaTypeContainer(int containerId)
        {
            return GetContainer(containerId, Constants.ObjectTypes.MediaTypeContainerGuid);
        }

        private EntityContainer GetContainer(int containerId, Guid containerObjectType)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repo = RepositoryFactory.CreateEntityContainerRepository(uow, containerObjectType);
                return repo.Get(containerId);
            }
        }

        public IEnumerable<EntityContainer> GetMediaTypeContainers(int[] containerIds)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repo = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.MediaTypeContainerGuid);
                return repo.GetAll(containerIds);
            }
        }

        public IEnumerable<EntityContainer> GetMediaTypeContainers(string name, int level)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repo = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.MediaTypeContainerGuid);
                return repo.Get(name, level);
            }
        }

        public IEnumerable<EntityContainer> GetMediaTypeContainers(IMediaType mediaType)
        {
            var ancestorIds = mediaType.Path.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    var asInt = x.TryConvertTo<int>();
                    if (asInt) return asInt.Result;
                    return int.MinValue;
                })
                .Where(x => x != int.MinValue && x != mediaType.Id)
                .ToArray();

            return GetMediaTypeContainers(ancestorIds);
        }

        public EntityContainer GetContentTypeContainer(Guid containerId)
        {
            return GetContainer(containerId, Constants.ObjectTypes.DocumentTypeContainerGuid);
        }

        public IEnumerable<EntityContainer> GetContentTypeContainers(int[] containerIds)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repo = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.DocumentTypeContainerGuid);
                return repo.GetAll(containerIds);
            }
        }

        public IEnumerable<EntityContainer> GetContentTypeContainers(IContentType contentType)
        {
            var ancestorIds = contentType.Path.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    var asInt = x.TryConvertTo<int>();
                    if (asInt) return asInt.Result;
                    return int.MinValue;
                })
                .Where(x => x != int.MinValue && x != contentType.Id)
                .ToArray();

            return GetContentTypeContainers(ancestorIds);
        }

        public EntityContainer GetMediaTypeContainer(Guid containerId)
        {
            return GetContainer(containerId, Constants.ObjectTypes.MediaTypeContainerGuid);
        }

        private EntityContainer GetContainer(Guid containerId, Guid containerObjectType)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repo = RepositoryFactory.CreateEntityContainerRepository(uow, containerObjectType);
                return repo.Get(containerId);
            }
        }

        public IEnumerable<EntityContainer> GetContentTypeContainers(string name, int level)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repo = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.DocumentTypeContainerGuid);
                return repo.Get(name, level);
            }
        }

        public Attempt<OperationStatus> DeleteContentTypeContainer(int containerId, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.DocumentTypeContainerGuid);
                var container = repo.Get(containerId);
                if (container == null)
                {
                    uow.Commit();
                    return OperationStatus.NoOperation(evtMsgs);
                }

                var deleteEventArgs = new DeleteEventArgs<EntityContainer>(container, evtMsgs);
                if (uow.Events.DispatchCancelable(DeletingContentTypeContainer, this, deleteEventArgs))
                {
                    uow.Commit();
                    return Attempt.Fail(new OperationStatus(OperationStatusType.FailedCancelledByEvent, evtMsgs));
                }

                repo.Delete(container);
                uow.Commit();
                deleteEventArgs.CanCancel = false;
                uow.Events.Dispatch(DeletedContentTypeContainer, this, deleteEventArgs, "DeletedContentTypeContainer");

                return OperationStatus.Success(evtMsgs);
                //TODO: Audit trail ?
            }
        }

        public Attempt<OperationStatus> DeleteMediaTypeContainer(int containerId, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.MediaTypeContainerGuid);

                var container = repo.Get(containerId);
                if (container == null)
                {
                    uow.Commit();
                    return OperationStatus.NoOperation(evtMsgs);
                }

                var deleteEventArgs = new DeleteEventArgs<EntityContainer>(container, evtMsgs);
                if (uow.Events.DispatchCancelable(DeletingMediaTypeContainer, this, deleteEventArgs))
                {
                    uow.Commit();
                    return Attempt.Fail(new OperationStatus(OperationStatusType.FailedCancelledByEvent, evtMsgs));
                }

                repo.Delete(container);
                uow.Commit();
                deleteEventArgs.CanCancel = false;
                uow.Events.Dispatch(DeletedMediaTypeContainer, this, deleteEventArgs, "DeletedMediaTypeContainer");

                return OperationStatus.Success(evtMsgs);
                //TODO: Audit trail ?
            }
        }

        #endregion

        /// <summary>
        /// Gets all property type aliases.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllPropertyTypeAliases()
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateContentTypeRepository(uow);
                return repository.GetAllPropertyTypeAliases();
            }
        }

        /// <summary>
        /// Gets all content type aliases
        /// </summary>
        /// <param name="objectTypes">
        /// If this list is empty, it will return all content type aliases for media, members and content, otherwise
        /// it will only return content type aliases for the object types specified
        /// </param>
        /// <returns></returns>
        public IEnumerable<string> GetAllContentTypeAliases(params Guid[] objectTypes)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateContentTypeRepository(uow);
                return repository.GetAllContentTypeAliases(objectTypes);
            }
        }

        public IEnumerable<int> GetAllContentTypeIds(string[] aliases)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateContentTypeRepository(uow);
                return repository.GetAllContentTypeIds(aliases);
            }
        }

        /// <summary>
        /// Copies a content type as a child under the specified parent if specified (otherwise to the root)
        /// </summary>
        /// <param name="original">
        /// The content type to copy
        /// </param>
        /// <param name="alias">
        /// The new alias of the content type
        /// </param>
        /// <param name="name">
        /// The new name of the content type
        /// </param>
        /// <param name="parentId">
        /// The parent to copy the content type to, default is -1 (root)
        /// </param>
        /// <returns></returns>
        public IContentType Copy(IContentType original, string alias, string name, int parentId = -1)
        {
            IContentType parent = null;
            if (parentId > 0)
            {
                parent = GetContentType(parentId);
                if (parent == null)
                {
                    throw new InvalidOperationException("Could not find content type with id " + parentId);
                }
            }
            return Copy(original, alias, name, parent);
        }

        /// <summary>
        /// Copies a content type as a child under the specified parent if specified (otherwise to the root)
        /// </summary>
        /// <param name="original">
        /// The content type to copy
        /// </param>
        /// <param name="alias">
        /// The new alias of the content type
        /// </param>
        /// <param name="name">
        /// The new name of the content type
        /// </param>
        /// <param name="parent">
        /// The parent to copy the content type to, default is null (root)
        /// </param>
        /// <returns></returns>
        public IContentType Copy(IContentType original, string alias, string name, IContentType parent)
        {
            Mandate.ParameterNotNull(original, "original");
            Mandate.ParameterNotNullOrEmpty(alias, "alias");
            if (parent != null)
            {
                Mandate.That(parent.HasIdentity, () => new InvalidOperationException("The parent content type must have an identity"));
            }

            var clone = original.DeepCloneWithResetIdentities(alias);

            clone.Name = name;

            var compositionAliases = clone.CompositionAliases().Except(new[] { alias }).ToList();
            //remove all composition that is not it's current alias
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

        /// <summary>
        /// Gets an <see cref="IContentType"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IContentType"/> to retrieve</param>
        /// <returns><see cref="IContentType"/></returns>
        public IContentType GetContentType(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateContentTypeRepository(uow);
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets an <see cref="IContentType"/> object by its Alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="IContentType"/> to retrieve</param>
        /// <returns><see cref="IContentType"/></returns>
        public IContentType GetContentType(string alias)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateContentTypeRepository(uow);
                return repository.Get(alias);
            }
        }

        /// <summary>
        /// Gets an <see cref="IContentType"/> object by its Key
        /// </summary>
        /// <param name="id">Alias of the <see cref="IContentType"/> to retrieve</param>
        /// <returns><see cref="IContentType"/></returns>
        public IContentType GetContentType(Guid id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateContentTypeRepository(uow);
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets a list of all available <see cref="IContentType"/> objects
        /// </summary>
        /// <param name="ids">Optional list of ids</param>
        /// <returns>An Enumerable list of <see cref="IContentType"/> objects</returns>
        public IEnumerable<IContentType> GetAllContentTypes(params int[] ids)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateContentTypeRepository(uow);
                return repository.GetAll(ids);
            }
        }

        /// <summary>
        /// Gets a list of all available <see cref="IContentType"/> objects
        /// </summary>
        /// <param name="ids">Optional list of ids</param>
        /// <returns>An Enumerable list of <see cref="IContentType"/> objects</returns>
        public IEnumerable<IContentType> GetAllContentTypes(IEnumerable<Guid> ids)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateContentTypeRepository(uow);
                return repository.GetAll(ids.ToArray());
            }
        }

        /// <summary>
        /// Gets a list of children for a <see cref="IContentType"/> object
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        /// <returns>An Enumerable list of <see cref="IContentType"/> objects</returns>
        public IEnumerable<IContentType> GetContentTypeChildren(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateContentTypeRepository(uow);
                var query = Query<IContentType>.Builder.Where(x => x.ParentId == id);
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Gets a list of children for a <see cref="IContentType"/> object
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        /// <returns>An Enumerable list of <see cref="IContentType"/> objects</returns>
        public IEnumerable<IContentType> GetContentTypeChildren(Guid id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateContentTypeRepository(uow);

                var found = GetContentType(id);
                if (found == null) return Enumerable.Empty<IContentType>();
                var query = Query<IContentType>.Builder.Where(x => x.ParentId == found.Id);
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Checks whether an <see cref="IContentType"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IContentType"/></param>
        /// <returns>True if the content type has any children otherwise False</returns>
        public bool HasChildren(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateContentTypeRepository(uow);

                var query = Query<IContentType>.Builder.Where(x => x.ParentId == id);
                return repository.Count(query) > 0;
            }
        }

        /// <summary>
        /// Checks whether an <see cref="IContentType"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IContentType"/></param>
        /// <returns>True if the content type has any children otherwise False</returns>
        public bool HasChildren(Guid id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateContentTypeRepository(uow);

                var found = GetContentType(id);
                if (found == null) return false;
                var query = Query<IContentType>.Builder.Where(x => x.ParentId == found.Id);
                return repository.Count(query) > 0;
            }
        }

        public override IEnumerable<IContentTypeBase> GetDescendants(IContentTypeBase contentType)
        {
            var ctype = contentType as IContentType;
            if (ctype != null) return GetDescendants(ctype);
            var mtype = contentType as IMediaType;
            if (mtype != null) return GetDescendants(mtype);
            return Enumerable.Empty<IContentTypeBase>();
        }

        public IEnumerable<IContentType> GetDescendants(IContentType contentType)
        {
            return GetContentTypeChildren(contentType.Id)
                .SelectRecursive(type => GetContentTypeChildren(type.Id));
        }

        public IEnumerable<IMediaType> GetDescendants(IMediaType contentType)
        {
            return GetMediaTypeChildren(contentType.Id)
                .SelectRecursive(type => GetMediaTypeChildren(type.Id));
        }

        /// <summary>
        /// This is called after an IContentType is saved and is used to update the content xml structures in the database
        /// if they are required to be updated.
        /// </summary>
        /// <param name="contentTypes">A tuple of a content type and a boolean indicating if it is new (HasIdentity was false before committing)</param>
        private void UpdateContentXmlStructure(params IContentTypeBase[] contentTypes)
        {
            var toUpdate = GetContentTypesForXmlUpdates(contentTypes).ToArray();

            if (toUpdate.Any() == false) return;

            var firstType = toUpdate.First();
            //if it is a content type then call the rebuilding methods or content
            if (firstType is IContentType)
            {
                var typedContentService = _contentService as ContentService;
                if (typedContentService != null)
                {
                    typedContentService.RePublishAll(toUpdate.Select(x => x.Id).ToArray());
                }
                else
                {
                    //this should never occur, the content service should always be typed but we'll check anyways.
                    _contentService.RePublishAll();
                }
            }
            else if (firstType is IMediaType)
            {
                //if it is a media type then call the rebuilding methods for media
                var typedContentService = _mediaService as MediaService;
                if (typedContentService != null)
                {
                    typedContentService.RebuildXmlStructures(toUpdate.Select(x => x.Id).ToArray());
                }
            }
        }

        public int CountContentTypes()
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateContentTypeRepository(uow);
                return repository.Count(Query<IContentType>.Builder);
            }
        }

        public int CountMediaTypes()
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaTypeRepository(uow);
                return repository.Count(Query<IMediaType>.Builder);
            }
        }

        /// <summary>
        /// Validates the composition, if its invalid a list of property type aliases that were duplicated is returned
        /// </summary>
        /// <param name="compo"></param>
        /// <returns></returns>
        public Attempt<string[]> ValidateComposition(IContentTypeComposition compo)
        {
            using (new WriteLock(Locker))
            {
                try
                {
                    ValidateLocked(compo);
                    return Attempt<string[]>.Succeed();
                }
                catch (InvalidCompositionException ex)
                {
                    return Attempt.Fail(ex.PropertyTypeAliases, ex);
                }
            }
        }

        protected void ValidateLocked(IContentTypeComposition compositionContentType)
        {
            // performs business-level validation of the composition
            // should ensure that it is absolutely safe to save the composition

            // eg maybe a property has been added, with an alias that's OK (no conflict with ancestors)
            // but that cannot be used (conflict with descendants)

            var contentType = compositionContentType as IContentType;
            var mediaType = compositionContentType as IMediaType;
            var memberType = compositionContentType as IMemberType; // should NOT do it here but... v8!

            IContentTypeComposition[] allContentTypes;
            if (contentType != null)
                allContentTypes = GetAllContentTypes().Cast<IContentTypeComposition>().ToArray();
            else if (mediaType != null)
                allContentTypes = GetAllMediaTypes().Cast<IContentTypeComposition>().ToArray();
            else if (memberType != null)
                return; // no compositions on members, always validate
            else
                throw new Exception("Composition is neither IContentType nor IMediaType nor IMemberType?");

            var compositionAliases = compositionContentType.CompositionAliases();
            var compositions = allContentTypes.Where(x => compositionAliases.Any(y => x.Alias.Equals(y)));
            var propertyTypeAliases = compositionContentType.PropertyTypes.Select(x => x.Alias.ToLowerInvariant()).ToArray();
            var indirectReferences = allContentTypes.Where(x => x.ContentTypeComposition.Any(y => y.Id == compositionContentType.Id));
            var comparer = new DelegateEqualityComparer<IContentTypeComposition>((x, y) => x.Id == y.Id, x => x.Id);
            var dependencies = new HashSet<IContentTypeComposition>(compositions, comparer);
            var stack = new Stack<IContentTypeComposition>();
            indirectReferences.ForEach(stack.Push); //Push indirect references to a stack, so we can add recursively
            while (stack.Count > 0)
            {
                var indirectReference = stack.Pop();
                dependencies.Add(indirectReference);
                //Get all compositions for the current indirect reference
                var directReferences = indirectReference.ContentTypeComposition;

                foreach (var directReference in directReferences)
                {
                    if (directReference.Id == compositionContentType.Id || directReference.Alias.Equals(compositionContentType.Alias)) continue;
                    dependencies.Add(directReference);
                    //A direct reference has compositions of its own - these also need to be taken into account
                    var directReferenceGraph = directReference.CompositionAliases();
                    allContentTypes.Where(x => directReferenceGraph.Any(y => x.Alias.Equals(y, StringComparison.InvariantCultureIgnoreCase))).ForEach(c => dependencies.Add(c));
                }
                //Recursive lookup of indirect references
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

        /// <summary>
        /// Saves a single <see cref="IContentType"/> object
        /// </summary>
        /// <param name="contentType"><see cref="IContentType"/> to save</param>
        /// <param name="userId">Optional id of the user saving the ContentType</param>
        public void Save(IContentType contentType, int userId = 0)
        {
            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.GetUnitOfWork())
                {
                    var saveEventArgs = new SaveEventArgs<IContentType>(contentType);
                    if (uow.Events.DispatchCancelable(SavingContentType, this, saveEventArgs))
                    {
                        uow.Commit();
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(contentType.Name))
                    {
                        throw new ArgumentException("Cannot save content type with empty name.");
                    }

                    var repository = RepositoryFactory.CreateContentTypeRepository(uow);

                    ValidateLocked(contentType); // throws if invalid
                    contentType.CreatorId = userId;
                    if (contentType.Description == string.Empty)
                        contentType.Description = null;
                    repository.AddOrUpdate(contentType);

                    uow.Commit();

                    UpdateContentXmlStructure(contentType);

                    saveEventArgs.CanCancel = false;
                    uow.Events.Dispatch(SavedContentType, this, saveEventArgs);

                    Audit(uow, AuditType.Save, "Save ContentType performed by user", userId, contentType.Id);
                    uow.Commit();
                }
            }
        }

        /// <summary>
        /// Saves a collection of <see cref="IContentType"/> objects
        /// </summary>
        /// <param name="contentTypes">Collection of <see cref="IContentType"/> to save</param>
        /// <param name="userId">Optional id of the user saving the ContentType</param>
        public void Save(IEnumerable<IContentType> contentTypes, int userId = 0)
        {
            var asArray = contentTypes.ToArray();

            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.GetUnitOfWork())
                {
                    var saveEventArgs = new SaveEventArgs<IContentType>(asArray);
                    if (uow.Events.DispatchCancelable(SavingContentType, this, saveEventArgs))
                    {
                        uow.Commit();
                        return;
                    }

                    var repository = RepositoryFactory.CreateContentTypeRepository(uow);

                    foreach (var contentType in asArray)
                    {
                        ValidateLocked(contentType); // throws if invalid
                    }
                    foreach (var contentType in asArray)
                    {
                        contentType.CreatorId = userId;
                        if (contentType.Description == string.Empty)
                            contentType.Description = null;
                        repository.AddOrUpdate(contentType);
                    }

                    //save it all in one go
                    uow.Commit();

                    UpdateContentXmlStructure(asArray.Cast<IContentTypeBase>().ToArray());

                    saveEventArgs.CanCancel = false;
                    uow.Events.Dispatch(SavedContentType, this, saveEventArgs);

                    Audit(uow, AuditType.Save, "Save ContentTypes performed by user", userId, -1);
                    uow.Commit();
                }
            }
        }

        /// <summary>
        /// Deletes a single <see cref="IContentType"/> object
        /// </summary>
        /// <param name="contentType"><see cref="IContentType"/> to delete</param>
        /// <param name="userId">Optional id of the user issueing the delete</param>
        /// <remarks>Deleting a <see cref="IContentType"/> will delete all the <see cref="IContent"/> objects based on this <see cref="IContentType"/></remarks>
        public void Delete(IContentType contentType, int userId = 0)
        {
            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.GetUnitOfWork())
                {
                    var deleteEventArgs = new DeleteEventArgs<IContentType>(contentType);
                    if (uow.Events.DispatchCancelable(DeletingContentType, this, deleteEventArgs))
                    {
                        uow.Commit();
                        return;
                    }

                    var repository = RepositoryFactory.CreateContentTypeRepository(uow);

                    //If we are deleting this content type, we are also deleting it's descendents!
                    var deletedContentTypes = new List<IContentType> { contentType };
                    deletedContentTypes.AddRange(GetDescendants(contentType));

                    var ids = deletedContentTypes.Select(x => x.Id).ToArray();
                    _contentService.DeleteContentOfTypes(ids, userId);
                    _contentService.DeleteBlueprintsOfTypes(ids);

                    repository.Delete(contentType);
                    deleteEventArgs.DeletedEntities = deletedContentTypes.DistinctBy(x => x.Id);
                    deleteEventArgs.CanCancel = false;
                    uow.Events.Dispatch(DeletedContentType, this, deleteEventArgs);

                    Audit(uow, AuditType.Delete, string.Format("Delete ContentType performed by user"), userId, contentType.Id);
                    uow.Commit();
                }
            }
        }

        /// <summary>
        /// Deletes a collection of <see cref="IContentType"/> objects.
        /// </summary>
        /// <param name="contentTypes">Collection of <see cref="IContentType"/> to delete</param>
        /// <param name="userId">Optional id of the user issueing the delete</param>
        /// <remarks>
        /// Deleting a <see cref="IContentType"/> will delete all the <see cref="IContent"/> objects based on this <see cref="IContentType"/>
        /// </remarks>
        public void Delete(IEnumerable<IContentType> contentTypes, int userId = 0)
        {
            var asArray = contentTypes.ToArray();

            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.GetUnitOfWork())
                {
                    var deleteEventArgs = new DeleteEventArgs<IContentType>(asArray);
                    if (uow.Events.DispatchCancelable(DeletingContentType, this, deleteEventArgs))
                    {
                        uow.Commit();
                        return;
                    }

                    var repository = RepositoryFactory.CreateContentTypeRepository(uow);

                    //If we are deleting this content type, we are also deleting it's descendents!
                    var deletedContentTypes = new List<IContentType>(asArray);
                    foreach (var contentType in asArray)
                    {
                        deletedContentTypes.AddRange(GetDescendants(contentType));
                    }

                    _contentService.DeleteContentOfTypes(deletedContentTypes.Select(x => x.Id), userId);

                    foreach (var contentType in asArray)
                    {
                        repository.Delete(contentType);
                    }
                    deleteEventArgs.DeletedEntities = deletedContentTypes.DistinctBy(x => x.Id);
                    deleteEventArgs.CanCancel = false;
                    uow.Events.Dispatch(DeletedContentType, this, deleteEventArgs);

                    Audit(uow, AuditType.Delete, string.Format("Delete ContentTypes performed by user"), userId, -1);
                    uow.Commit();
                }
            }
        }

        /// <summary>
        /// Gets an <see cref="IMediaType"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/> to retrieve</param>
        /// <returns><see cref="IMediaType"/></returns>
        public IMediaType GetMediaType(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaTypeRepository(uow);
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets an <see cref="IMediaType"/> object by its Alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="IMediaType"/> to retrieve</param>
        /// <returns><see cref="IMediaType"/></returns>
        public IMediaType GetMediaType(string alias)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaTypeRepository(uow);
                return repository.Get(alias);
            }
        }

        /// <summary>
        /// Gets an <see cref="IMediaType"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/> to retrieve</param>
        /// <returns><see cref="IMediaType"/></returns>
        public IMediaType GetMediaType(Guid id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaTypeRepository(uow);
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets a list of all available <see cref="IMediaType"/> objects
        /// </summary>
        /// <param name="ids">Optional list of ids</param>
        /// <returns>An Enumerable list of <see cref="IMediaType"/> objects</returns>
        public IEnumerable<IMediaType> GetAllMediaTypes(params int[] ids)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaTypeRepository(uow);
                return repository.GetAll(ids);
            }
        }

        /// <summary>
        /// Gets a list of all available <see cref="IMediaType"/> objects
        /// </summary>
        /// <param name="ids">Optional list of ids</param>
        /// <returns>An Enumerable list of <see cref="IMediaType"/> objects</returns>
        public IEnumerable<IMediaType> GetAllMediaTypes(IEnumerable<Guid> ids)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaTypeRepository(uow);
                return repository.GetAll(ids.ToArray());
            }
        }

        /// <summary>
        /// Gets a list of children for a <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        /// <returns>An Enumerable list of <see cref="IMediaType"/> objects</returns>
        public IEnumerable<IMediaType> GetMediaTypeChildren(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaTypeRepository(uow);
                var query = Query<IMediaType>.Builder.Where(x => x.ParentId == id);
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Gets a list of children for a <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        /// <returns>An Enumerable list of <see cref="IMediaType"/> objects</returns>
        public IEnumerable<IMediaType> GetMediaTypeChildren(Guid id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaTypeRepository(uow);
                var found = GetMediaType(id);
                if (found == null) return Enumerable.Empty<IMediaType>();
                var query = Query<IMediaType>.Builder.Where(x => x.ParentId == found.Id);
                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Checks whether an <see cref="IMediaType"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/></param>
        /// <returns>True if the media type has any children otherwise False</returns>
        public bool MediaTypeHasChildren(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaTypeRepository(uow);
                var query = Query<IMediaType>.Builder.Where(x => x.ParentId == id);
                return repository.Count(query) > 0;
            }
        }

        /// <summary>
        /// Checks whether an <see cref="IMediaType"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/></param>
        /// <returns>True if the media type has any children otherwise False</returns>
        public bool MediaTypeHasChildren(Guid id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMediaTypeRepository(uow);
                var found = GetMediaType(id);
                if (found == null) return false;
                var query = Query<IMediaType>.Builder.Where(x => x.ParentId == found.Id);
                return repository.Count(query) > 0;
            }
        }

        public Attempt<OperationStatus<MoveOperationStatusType>> MoveMediaType(IMediaType toMove, int containerId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            var moveInfo = new List<MoveEventInfo<IMediaType>>();
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var moveEventInfo = new MoveEventInfo<IMediaType>(toMove, toMove.Path, containerId);
                var moveEventArgs = new MoveEventArgs<IMediaType>(evtMsgs, moveEventInfo);
                if (uow.Events.DispatchCancelable(MovingMediaType, this, moveEventArgs))
                {
                    uow.Commit();
                    return Attempt.Fail(new OperationStatus<MoveOperationStatusType>(MoveOperationStatusType.FailedCancelledByEvent, evtMsgs));
                }

                var containerRepository = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.MediaTypeContainerGuid);
                var repository = RepositoryFactory.CreateMediaTypeRepository(uow);
                try
                {
                    EntityContainer container = null;
                    if (containerId > 0)
                    {
                        container = containerRepository.Get(containerId);
                        if (container == null)
                            throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound);
                    }
                    moveInfo.AddRange(repository.Move(toMove, container));
                }
                catch (DataOperationException<MoveOperationStatusType> ex)
                {
                    uow.Commit();
                    return Attempt.Fail(new OperationStatus<MoveOperationStatusType>(ex.Operation, evtMsgs));
                }
                uow.Commit();
                moveEventArgs.MoveInfoCollection = moveInfo;
                moveEventArgs.CanCancel = false;
                uow.Events.Dispatch(MovedMediaType, this, moveEventArgs);
            }

            return Attempt.Succeed(
                new OperationStatus<MoveOperationStatusType>(MoveOperationStatusType.Success, evtMsgs));
        }

        public Attempt<OperationStatus<MoveOperationStatusType>> MoveContentType(IContentType toMove, int containerId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            var moveInfo = new List<MoveEventInfo<IContentType>>();
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var moveEventInfo = new MoveEventInfo<IContentType>(toMove, toMove.Path, containerId);
                var moveEventArgs = new MoveEventArgs<IContentType>(evtMsgs, moveEventInfo);
                if (uow.Events.DispatchCancelable(MovingContentType, this, moveEventArgs))
                {
                    uow.Commit();
                    return Attempt.Fail(new OperationStatus<MoveOperationStatusType>(MoveOperationStatusType.FailedCancelledByEvent, evtMsgs));
                }

                var containerRepository = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.DocumentTypeContainerGuid);
                var repository = RepositoryFactory.CreateContentTypeRepository(uow);
                try
                {
                    EntityContainer container = null;
                    if (containerId > 0)
                    {
                        container = containerRepository.Get(containerId);
                        if (container == null)
                            throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound);
                    }
                    moveInfo.AddRange(repository.Move(toMove, container));
                }
                catch (DataOperationException<MoveOperationStatusType> ex)
                {
                    uow.Commit();
                    return Attempt.Fail(new OperationStatus<MoveOperationStatusType>(ex.Operation, evtMsgs));
                }
                moveEventArgs.MoveInfoCollection = moveInfo;
                moveEventArgs.CanCancel = false;
                uow.Commit();
                uow.Events.Dispatch(MovedContentType, this, moveEventArgs);
            }

            return Attempt.Succeed(
                new OperationStatus<MoveOperationStatusType>(MoveOperationStatusType.Success, evtMsgs));
        }

        public Attempt<OperationStatus<IMediaType, MoveOperationStatusType>> CopyMediaType(IMediaType toCopy, int containerId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            IMediaType copy;
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var containerRepository = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.MediaTypeContainerGuid);
                var repository = RepositoryFactory.CreateMediaTypeRepository(uow);

                try
                {
                    if (containerId > 0)
                    {
                        var container = containerRepository.Get(containerId);
                        if (container == null)
                            throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound);
                    }
                    var alias = repository.GetUniqueAlias(toCopy.Alias);
                    copy = toCopy.DeepCloneWithResetIdentities(alias);
                    copy.Name = copy.Name + " (copy)"; // might not be unique

                    // if it has a parent, and the parent is a content type, unplug composition
                    // all other compositions remain in place in the copied content type
                    if (copy.ParentId > 0)
                    {
                        var parent = repository.Get(copy.ParentId);
                        if (parent != null)
                            copy.RemoveContentType(parent.Alias);
                    }

                    copy.ParentId = containerId;
                    repository.AddOrUpdate(copy);
                }
                catch (DataOperationException<MoveOperationStatusType> ex)
                {
                    uow.Commit();
                    return Attempt.Fail(new OperationStatus<IMediaType, MoveOperationStatusType>(null, ex.Operation, evtMsgs));
                }
                uow.Commit();
            }

            return Attempt.Succeed(new OperationStatus<IMediaType, MoveOperationStatusType>(copy, MoveOperationStatusType.Success, evtMsgs));
        }

        public Attempt<OperationStatus<IContentType, MoveOperationStatusType>> CopyContentType(IContentType toCopy, int containerId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            IContentType copy;
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var containerRepository = RepositoryFactory.CreateEntityContainerRepository(uow, Constants.ObjectTypes.DocumentTypeContainerGuid);
                var repository = RepositoryFactory.CreateContentTypeRepository(uow);

                try
                {
                    if (containerId > 0)
                    {
                        var container = containerRepository.Get(containerId);
                        if (container == null)
                            throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound);
                    }
                    var alias = repository.GetUniqueAlias(toCopy.Alias);
                    copy = toCopy.DeepCloneWithResetIdentities(alias);
                    copy.Name = copy.Name + " (copy)"; // might not be unique

                    // if it has a parent, and the parent is a content type, unplug composition
                    // all other compositions remain in place in the copied content type
                    if (copy.ParentId > 0)
                    {
                        var parent = repository.Get(copy.ParentId);
                        if (parent != null)
                            copy.RemoveContentType(parent.Alias);
                    }

                    copy.ParentId = containerId;
                    repository.AddOrUpdate(copy);
                }
                catch (DataOperationException<MoveOperationStatusType> ex)
                {
                    uow.Commit();
                    return Attempt.Fail(new OperationStatus<IContentType, MoveOperationStatusType>(null, ex.Operation, evtMsgs));
                }
                uow.Commit();
            }

            return Attempt.Succeed(new OperationStatus<IContentType, MoveOperationStatusType>(copy, MoveOperationStatusType.Success, evtMsgs));
        }

        /// <summary>
        /// Saves a single <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="mediaType"><see cref="IMediaType"/> to save</param>
        /// <param name="userId">Optional Id of the user saving the MediaType</param>
        public void Save(IMediaType mediaType, int userId = 0)
        {
            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.GetUnitOfWork())
                {
                    var saveEventArgs = new SaveEventArgs<IMediaType>(mediaType);
                    if (uow.Events.DispatchCancelable(SavingMediaType, this, saveEventArgs))
                    {
                        uow.Commit();
                        return;
                    }

                    var repository = RepositoryFactory.CreateMediaTypeRepository(uow);

                    ValidateLocked(mediaType); // throws if invalid
                    mediaType.CreatorId = userId;
                    if (mediaType.Description == string.Empty)
                        mediaType.Description = null;
                    repository.AddOrUpdate(mediaType);
                    uow.Commit();

                    UpdateContentXmlStructure(mediaType);
                    saveEventArgs.CanCancel = false;
                    uow.Events.Dispatch(SavedMediaType, this, saveEventArgs);

                    Audit(uow, AuditType.Save, "Save MediaType performed by user", userId, mediaType.Id);
                    uow.Commit();
                }
            }
        }

        /// <summary>
        /// Saves a collection of <see cref="IMediaType"/> objects
        /// </summary>
        /// <param name="mediaTypes">Collection of <see cref="IMediaType"/> to save</param>
        /// <param name="userId">Optional Id of the user savging the MediaTypes</param>
        public void Save(IEnumerable<IMediaType> mediaTypes, int userId = 0)
        {
            var asArray = mediaTypes.ToArray();

            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.GetUnitOfWork())
                {
                    var saveEventArgs = new SaveEventArgs<IMediaType>(asArray);
                    if (uow.Events.DispatchCancelable(SavingMediaType, this, saveEventArgs))
                    {
                        uow.Commit();
                        return;
                    }

                    var repository = RepositoryFactory.CreateMediaTypeRepository(uow);

                    foreach (var mediaType in asArray)
                    {
                        ValidateLocked(mediaType); // throws if invalid
                    }
                    foreach (var mediaType in asArray)
                    {
                        mediaType.CreatorId = userId;
                        if (mediaType.Description == string.Empty)
                            mediaType.Description = null;
                        repository.AddOrUpdate(mediaType);
                    }

                    //save it all in one go
                    uow.Commit();

                    UpdateContentXmlStructure(asArray.Cast<IContentTypeBase>().ToArray());
                    saveEventArgs.CanCancel = false;
                    uow.Events.Dispatch(SavedMediaType, this, saveEventArgs);

                    Audit(uow, AuditType.Save, "Save MediaTypes performed by user", userId, -1);
                    uow.Commit();
                }
            }
        }

        /// <summary>
        /// Deletes a single <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="mediaType"><see cref="IMediaType"/> to delete</param>
        /// <param name="userId">Optional Id of the user deleting the MediaType</param>
        /// <remarks>Deleting a <see cref="IMediaType"/> will delete all the <see cref="IMedia"/> objects based on this <see cref="IMediaType"/></remarks>
        public void Delete(IMediaType mediaType, int userId = 0)
        {
            //TODO: Share all of this logic with the Delete IContentType methods, no need for code duplication

            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.GetUnitOfWork())
                {
                    var deleteEventArgs = new DeleteEventArgs<IMediaType>(mediaType);
                    if (uow.Events.DispatchCancelable(DeletingMediaType, this, deleteEventArgs))
                    {
                        uow.Commit();
                        return;
                    }

                    var repository = RepositoryFactory.CreateMediaTypeRepository(uow);

                    //If we are deleting this content type, we are also deleting it's descendents!
                    var deletedMediaTypes = new List<IMediaType> { mediaType };
                    deletedMediaTypes.AddRange(GetDescendants(mediaType));

                    _mediaService.DeleteMediaOfTypes(deletedMediaTypes.Select(x => x.Id), userId);

                    repository.Delete(mediaType);

                    deleteEventArgs.CanCancel = false;
                    deleteEventArgs.DeletedEntities = deletedMediaTypes.DistinctBy(x => x.Id);
                    uow.Events.Dispatch(DeletedMediaType, this, deleteEventArgs);

                    Audit(uow, AuditType.Delete, "Delete MediaType performed by user", userId, mediaType.Id);
                    uow.Commit();
                }
            }
        }

        /// <summary>
        /// Deletes a collection of <see cref="IMediaType"/> objects
        /// </summary>
        /// <param name="mediaTypes">Collection of <see cref="IMediaType"/> to delete</param>
        /// <param name="userId"></param>
        /// <remarks>Deleting a <see cref="IMediaType"/> will delete all the <see cref="IMedia"/> objects based on this <see cref="IMediaType"/></remarks>
        public void Delete(IEnumerable<IMediaType> mediaTypes, int userId = 0)
        {
            //TODO: Share all of this logic with the Delete IContentType methods, no need for code duplication

            var asArray = mediaTypes.ToArray();

            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.GetUnitOfWork())
                {
                    var deleteEventArgs = new DeleteEventArgs<IMediaType>(asArray);
                    if (uow.Events.DispatchCancelable(DeletingMediaType, this, deleteEventArgs))
                    {
                        uow.Commit();
                        return;
                    }

                    var repository = RepositoryFactory.CreateMediaTypeRepository(uow);

                    //If we are deleting this content type, we are also deleting it's descendents!
                    var deletedMediaTypes = new List<IMediaType>(asArray);
                    foreach (var mediaType in asArray)
                    {
                        deletedMediaTypes.AddRange(GetDescendants(mediaType));
                    }

                    _mediaService.DeleteMediaOfTypes(deletedMediaTypes.Select(x => x.Id), userId);

                    foreach (var mediaType in asArray)
                    {
                        repository.Delete(mediaType);
                    }

                    deleteEventArgs.DeletedEntities = deletedMediaTypes.DistinctBy(x => x.Id);
                    deleteEventArgs.CanCancel = false;
                    uow.Events.Dispatch(DeletedMediaType, this, deleteEventArgs);

                    Audit(uow, AuditType.Delete, "Delete MediaTypes performed by user", userId, -1);
                    uow.Commit();
                }
            }
        }

        /// <summary>
        /// Generates the complete (simplified) XML DTD.
        /// </summary>
        /// <returns>The DTD as a string</returns>
        public string GetDtd()
        {
            var dtd = new StringBuilder();
            dtd.AppendLine("<!DOCTYPE root [ ");

            dtd.AppendLine(GetContentTypesDtd());
            dtd.AppendLine("]>");

            return dtd.ToString();
        }

        /// <summary>
        /// Generates the complete XML DTD without the root.
        /// </summary>
        /// <returns>The DTD as a string</returns>
        public string GetContentTypesDtd()
        {
            var dtd = new StringBuilder();
            if (UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema)
            {
                dtd.AppendLine("<!ELEMENT node ANY> <!ATTLIST node id ID #REQUIRED>  <!ELEMENT data ANY>");
            }
            else
            {
                try
                {
                    var strictSchemaBuilder = new StringBuilder();

                    var contentTypes = GetAllContentTypes();
                    foreach (ContentType contentType in contentTypes)
                    {
                        string safeAlias = contentType.Alias.ToUmbracoAlias();
                        if (safeAlias != null)
                        {
                            strictSchemaBuilder.AppendLine(string.Format("<!ELEMENT {0} ANY>", safeAlias));
                            strictSchemaBuilder.AppendLine(string.Format("<!ATTLIST {0} id ID #REQUIRED>", safeAlias));
                        }
                    }

                    // Only commit the strong schema to the container if we didn't generate an error building it
                    dtd.Append(strictSchemaBuilder);
                }
                catch (Exception exception)
                {
                    LogHelper.Error<ContentTypeService>("Error while trying to build DTD for Xml schema; is Umbraco installed correctly and the connection string configured?", exception);
                }

            }
            return dtd.ToString();
        }

        private void Audit(IScopeUnitOfWork uow, AuditType type, string message, int userId, int objectId)
        {
            var auditRepo = RepositoryFactory.CreateAuditRepository(uow);
            auditRepo.AddOrUpdate(new AuditItem(objectId, message, type, userId));
        }

        #region Event Handlers

        public static event TypedEventHandler<IContentTypeService, SaveEventArgs<EntityContainer>> SavingContentTypeContainer;
        public static event TypedEventHandler<IContentTypeService, SaveEventArgs<EntityContainer>> SavedContentTypeContainer;
        public static event TypedEventHandler<IContentTypeService, DeleteEventArgs<EntityContainer>> DeletingContentTypeContainer;
        public static event TypedEventHandler<IContentTypeService, DeleteEventArgs<EntityContainer>> DeletedContentTypeContainer;
        public static event TypedEventHandler<IContentTypeService, SaveEventArgs<EntityContainer>> SavingMediaTypeContainer;
        public static event TypedEventHandler<IContentTypeService, SaveEventArgs<EntityContainer>> SavedMediaTypeContainer;
        public static event TypedEventHandler<IContentTypeService, DeleteEventArgs<EntityContainer>> DeletingMediaTypeContainer;
        public static event TypedEventHandler<IContentTypeService, DeleteEventArgs<EntityContainer>> DeletedMediaTypeContainer;


        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IContentTypeService, DeleteEventArgs<IContentType>> DeletingContentType;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IContentTypeService, DeleteEventArgs<IContentType>> DeletedContentType;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IContentTypeService, DeleteEventArgs<IMediaType>> DeletingMediaType;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IContentTypeService, DeleteEventArgs<IMediaType>> DeletedMediaType;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IContentTypeService, SaveEventArgs<IContentType>> SavingContentType;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IContentTypeService, SaveEventArgs<IContentType>> SavedContentType;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IContentTypeService, SaveEventArgs<IMediaType>> SavingMediaType;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IContentTypeService, SaveEventArgs<IMediaType>> SavedMediaType;

        /// <summary>
        /// Occurs before Move
        /// </summary>
        public static event TypedEventHandler<IContentTypeService, MoveEventArgs<IMediaType>> MovingMediaType;

        /// <summary>
        /// Occurs after Move
        /// </summary>
        public static event TypedEventHandler<IContentTypeService, MoveEventArgs<IMediaType>> MovedMediaType;

        /// <summary>
        /// Occurs before Move
        /// </summary>
        public static event TypedEventHandler<IContentTypeService, MoveEventArgs<IContentType>> MovingContentType;

        /// <summary>
        /// Occurs after Move
        /// </summary>
        public static event TypedEventHandler<IContentTypeService, MoveEventArgs<IContentType>> MovedContentType;

        #endregion
    }
}