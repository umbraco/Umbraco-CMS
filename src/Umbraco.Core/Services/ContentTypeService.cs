using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading;
using AutoMapper;
using Umbraco.Core.Auditing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
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

        public ContentTypeService(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory, IContentService contentService, IMediaService mediaService)
            : base(provider, logger, eventMessagesFactory)
        {
            if (contentService == null) throw new ArgumentNullException("contentService");
            if (mediaService == null) throw new ArgumentNullException("mediaService");
            _contentService = contentService;
            _mediaService = mediaService;
        }

        #region Containers

        public Attempt<OperationStatus<OperationStatusType, EntityContainer>> CreateContentTypeContainer(int parentId, string name, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IDocumentTypeContainerRepository>();
                try
                {
                    var container = new EntityContainer(Constants.ObjectTypes.DocumentTypeGuid)
                    {
                        Name = name,
                        ParentId = parentId,
                        CreatorId = userId
                    };

                    if (SavingContentTypeContainer.IsRaisedEventCancelled(
                        new SaveEventArgs<EntityContainer>(container, evtMsgs),
                        this))
                    {
                        return OperationStatus.Attempt.Cancel(evtMsgs, container);
                    }

                    repo.AddOrUpdate(container);
                    uow.Complete();

                    SavedContentTypeContainer.RaiseEvent(new SaveEventArgs<EntityContainer>(container, evtMsgs), this);
                    //TODO: Audit trail ?

                    return OperationStatus.Attempt.Succeed(evtMsgs, container);
                }
                catch (Exception ex)
                {
                    return OperationStatus.Attempt.Fail<OperationStatusType, EntityContainer>(OperationStatusType.FailedCancelledByEvent, evtMsgs, ex);
                }
            }
        }

        public Attempt<OperationStatus<OperationStatusType, EntityContainer>> CreateMediaTypeContainer(int parentId, string name, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IMediaTypeContainerRepository>();
                try
                {
                    var container = new EntityContainer(Constants.ObjectTypes.MediaTypeGuid)
                    {
                        Name = name,
                        ParentId = parentId,
                        CreatorId = userId
                    };

                    if (SavingMediaTypeContainer.IsRaisedEventCancelled(
                        new SaveEventArgs<EntityContainer>(container, evtMsgs),
                        this))
                    {
                        return OperationStatus.Attempt.Cancel(evtMsgs, container);
                    }

                    repo.AddOrUpdate(container);
                    uow.Complete();

                    SavedMediaTypeContainer.RaiseEvent(new SaveEventArgs<EntityContainer>(container, evtMsgs), this);
                    //TODO: Audit trail ?

                    return OperationStatus.Attempt.Succeed(evtMsgs, container);
                }
                catch (Exception ex)
                {
                    return OperationStatus.Attempt.Fail<EntityContainer>(evtMsgs, ex);
                }
            }
        }

        public Attempt<OperationStatus> SaveContentTypeContainer(EntityContainer container, int userId = 0)
        {
            return SaveContainer(
                SavingContentTypeContainer, SavedContentTypeContainer,
                container, Constants.ObjectTypes.DocumentTypeContainerGuid, "document type", userId);
        }

        public Attempt<OperationStatus> SaveMediaTypeContainer(EntityContainer container, int userId = 0)
        {
            return SaveContainer(
                SavingMediaTypeContainer, SavedMediaTypeContainer,
                container, Constants.ObjectTypes.MediaTypeContainerGuid, "media type", userId);
        }

        private Attempt<OperationStatus> SaveContainer(
            TypedEventHandler<IContentTypeService, SaveEventArgs<EntityContainer>> savingEvent,
            TypedEventHandler<IContentTypeService, SaveEventArgs<EntityContainer>> savedEvent,
            EntityContainer container,
            Guid containerObjectType,
            string objectTypeName, int userId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            if (container.ContainedObjectType != containerObjectType)
            {
                var ex = new InvalidOperationException("Not a " + objectTypeName + " container.");
                return OperationStatus.Attempt.Fail(evtMsgs, ex);
            }

            if (container.HasIdentity && container.IsPropertyDirty("ParentId"))
            {
                var ex = new InvalidOperationException("Cannot save a container with a modified parent, move the container instead.");
                return OperationStatus.Attempt.Fail(evtMsgs, ex);
            }

            if (savingEvent.IsRaisedEventCancelled(
                        new SaveEventArgs<EntityContainer>(container, evtMsgs),
                        this))
            {
                return OperationStatus.Attempt.Cancel(evtMsgs);
            }

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateContainerRepository(containerObjectType);
                repo.AddOrUpdate(container);
                uow.Complete();
            }

            savedEvent.RaiseEvent(new SaveEventArgs<EntityContainer>(container, evtMsgs), this);

            //TODO: Audit trail ?

            return OperationStatus.Attempt.Succeed(evtMsgs);
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateContainerRepository(containerObjectType);
                var container = repo.Get(containerId);
                return container;
            }
        }

        public IEnumerable<EntityContainer> GetMediaTypeContainers(int[] containerIds)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IMediaTypeContainerRepository>();
                return repo.GetAll(containerIds);
            }
        }

        public IEnumerable<EntityContainer> GetMediaTypeContainers(string name, int level)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IMediaTypeContainerRepository>();
                return ((EntityContainerRepository) repo).Get(name, level);
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IDocumentTypeContainerRepository>();
                return repo.GetAll(containerIds);
            }
        }

        public IEnumerable<EntityContainer> GetContentTypeContainers(IContentType contentType)
        {
            var ancestorIds = contentType.Path.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateContainerRepository(containerObjectType);
                var container = ((EntityContainerRepository)repo).Get(containerId);
                return container;
            }
        }

        public IEnumerable<EntityContainer> GetContentTypeContainers(string name, int level)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IDocumentTypeContainerRepository>();
                return ((EntityContainerRepository)repo).Get(name, level);
            }
        }

        public Attempt<OperationStatus> DeleteContentTypeContainer(int containerId, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IDocumentTypeContainerRepository>();
                var container = repo.Get(containerId);
                if (container == null) return OperationStatus.Attempt.NoOperation(evtMsgs);

                if (DeletingContentTypeContainer.IsRaisedEventCancelled(
                        new DeleteEventArgs<EntityContainer>(container, evtMsgs),
                        this))
                {
                    return Attempt.Fail(new OperationStatus(OperationStatusType.FailedCancelledByEvent, evtMsgs));
                }

                repo.Delete(container);
                uow.Complete();

                DeletedContentTypeContainer.RaiseEvent(new DeleteEventArgs<EntityContainer>(container, evtMsgs), this);

                return OperationStatus.Attempt.Succeed(evtMsgs);
                //TODO: Audit trail ?
            }
        }

        public Attempt<OperationStatus> DeleteMediaTypeContainer(int containerId, int userId = 0)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IMediaTypeContainerRepository>();
                var container = repo.Get(containerId);
                if (container == null) return OperationStatus.Attempt.NoOperation(evtMsgs);

                if (DeletingMediaTypeContainer.IsRaisedEventCancelled(
                        new DeleteEventArgs<EntityContainer>(container, evtMsgs),
                        this))
                {
                    return Attempt.Fail(new OperationStatus(OperationStatusType.FailedCancelledByEvent, evtMsgs));
                }

                repo.Delete(container);
                uow.Complete();

                DeletedMediaTypeContainer.RaiseEvent(new DeleteEventArgs<EntityContainer>(container, evtMsgs), this);

                return OperationStatus.Attempt.Succeed(evtMsgs);
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentTypeRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentTypeRepository>();
                return repository.GetAllContentTypeAliases(objectTypes);
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentTypeRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentTypeRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentTypeRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentTypeRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentTypeRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentTypeRepository>();
                var query = repository.Query.Where(x => x.ParentId == id);
                var contentTypes = repository.GetByQuery(query);
                return contentTypes;
            }
        }

        /// <summary>
        /// Gets a list of children for a <see cref="IContentType"/> object
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        /// <returns>An Enumerable list of <see cref="IContentType"/> objects</returns>
        public IEnumerable<IContentType> GetContentTypeChildren(Guid id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentTypeRepository>();
                var found = GetContentType(id);
                if (found == null) return Enumerable.Empty<IContentType>();
                var query = repository.Query.Where(x => x.ParentId == found.Id);
                var contentTypes = repository.GetByQuery(query);
                return contentTypes;
            }
        }

        /// <summary>
        /// Checks whether an <see cref="IContentType"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IContentType"/></param>
        /// <returns>True if the content type has any children otherwise False</returns>
        public bool HasChildren(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentTypeRepository>();
                var query = repository.Query.Where(x => x.ParentId == id);
                int count = repository.Count(query);
                return count > 0;
            }
        }

        /// <summary>
        /// Checks whether an <see cref="IContentType"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IContentType"/></param>
        /// <returns>True if the content type has any children otherwise False</returns>
        public bool HasChildren(Guid id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentTypeRepository>();
                var found = GetContentType(id);
                if (found == null) return false;
                var query = repository.Query.Where(x => x.ParentId == found.Id);
                int count = repository.Count(query);
                return count > 0;
            }
        }

        /// <summary>
        /// This is called after an IContentType is saved and is used to update the content xml structures in the database
        /// if they are required to be updated.
        /// </summary>
        /// <param name="contentTypes">A tuple of a content type and a boolean indicating if it is new (HasIdentity was false before committing)</param>
        private void UpdateContentXmlStructure(params IContentTypeBase[] contentTypes)
        {

            var toUpdate = GetContentTypesForXmlUpdates(contentTypes).ToArray();

            if (toUpdate.Any())
            {
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

        }

        public int CountContentTypes()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IContentTypeRepository>();
                return repository.Count(repository.Query);
            }
        }

        public int CountMediaTypes()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IMediaTypeRepository>();
                return repository.Count(repository.Query);
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
            indirectReferences.ForEach(stack.Push);//Push indirect references to a stack, so we can add recursively
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
	        if (SavingContentType.IsRaisedEventCancelled(new SaveEventArgs<IContentType>(contentType), this))
				return;

            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IContentTypeRepository>();
                    ValidateLocked(contentType); // throws if invalid
                    contentType.CreatorId = userId;
                    repository.AddOrUpdate(contentType);

                    uow.Complete();
                }

                UpdateContentXmlStructure(contentType);
            }
            SavedContentType.RaiseEvent(new SaveEventArgs<IContentType>(contentType, false), this);
	        Audit(AuditType.Save, string.Format("Save ContentType performed by user"), userId, contentType.Id);
        }

        /// <summary>
        /// Saves a collection of <see cref="IContentType"/> objects
        /// </summary>
        /// <param name="contentTypes">Collection of <see cref="IContentType"/> to save</param>
        /// <param name="userId">Optional id of the user saving the ContentType</param>
        public void Save(IEnumerable<IContentType> contentTypes, int userId = 0)
        {
            var asArray = contentTypes.ToArray();

            if (SavingContentType.IsRaisedEventCancelled(new SaveEventArgs<IContentType>(asArray), this))
				return;

            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IContentTypeRepository>();
                    // all-or-nothing, validate them all first
                    foreach (var contentType in asArray)
                    {
                        ValidateLocked(contentType); // throws if invalid
                    }
                    foreach (var contentType in asArray)
                    {
                        contentType.CreatorId = userId;
                        repository.AddOrUpdate(contentType);
                    }

                    //save it all in one go
                    uow.Complete();
                }

                UpdateContentXmlStructure(asArray.Cast<IContentTypeBase>().ToArray());
            }
            SavedContentType.RaiseEvent(new SaveEventArgs<IContentType>(asArray, false), this);
	        Audit(AuditType.Save, string.Format("Save ContentTypes performed by user"), userId, -1);
        }

        /// <summary>
        /// Deletes a single <see cref="IContentType"/> object
        /// </summary>
        /// <param name="contentType"><see cref="IContentType"/> to delete</param>
        /// <param name="userId">Optional id of the user issueing the delete</param>
        /// <remarks>Deleting a <see cref="IContentType"/> will delete all the <see cref="IContent"/> objects based on this <see cref="IContentType"/></remarks>
        public void Delete(IContentType contentType, int userId = 0)
        {
	        if (DeletingContentType.IsRaisedEventCancelled(new DeleteEventArgs<IContentType>(contentType), this))
				return;

            using (new WriteLock(Locker))
            {

                //TODO: This needs to change, if we are deleting a content type, we should just delete the data,
                // this method will recursively go lookup every content item, check if any of it's descendants are
                // of a different type, move them to the recycle bin, then permanently delete the content items.
                // The main problem with this is that for every content item being deleted, events are raised...
                // which we need for many things like keeping caches in sync, but we can surely do this MUCH better.

                _contentService.DeleteContentOfType(contentType.Id);

                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IContentTypeRepository>();
                    repository.Delete(contentType);
                    uow.Complete();

                    DeletedContentType.RaiseEvent(new DeleteEventArgs<IContentType>(contentType, false), this);
                }

                Audit(AuditType.Delete, string.Format("Delete ContentType performed by user"), userId, contentType.Id);
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

            if (DeletingContentType.IsRaisedEventCancelled(new DeleteEventArgs<IContentType>(asArray), this))
				return;

            using (new WriteLock(Locker))
            {
                foreach (var contentType in asArray)
                {
                    _contentService.DeleteContentOfType(contentType.Id);
                }

                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IContentTypeRepository>();
                    foreach (var contentType in asArray)
                    {
                        repository.Delete(contentType);
                    }

                    uow.Complete();

                    DeletedContentType.RaiseEvent(new DeleteEventArgs<IContentType>(asArray, false), this);
                }

                Audit(AuditType.Delete, string.Format("Delete ContentTypes performed by user"), userId, -1);
            }
        }

        /// <summary>
        /// Gets an <see cref="IMediaType"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/> to retrieve</param>
        /// <returns><see cref="IMediaType"/></returns>
        public IMediaType GetMediaType(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IMediaTypeRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IMediaTypeRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IMediaTypeRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IMediaTypeRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IMediaTypeRepository>();
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
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IMediaTypeRepository>();
                var query = repository.Query.Where(x => x.ParentId == id);
                var contentTypes = repository.GetByQuery(query);
                return contentTypes;
            }
        }

        /// <summary>
        /// Gets a list of children for a <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="id">Id of the Parent</param>
        /// <returns>An Enumerable list of <see cref="IMediaType"/> objects</returns>
        public IEnumerable<IMediaType> GetMediaTypeChildren(Guid id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IMediaTypeRepository>();
                var found = GetMediaType(id);
                if (found == null) return Enumerable.Empty<IMediaType>();
                var query = repository.Query.Where(x => x.ParentId == found.Id);
                var contentTypes = repository.GetByQuery(query);
                return contentTypes;
            }
        }

        /// <summary>
        /// Checks whether an <see cref="IMediaType"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/></param>
        /// <returns>True if the media type has any children otherwise False</returns>
        public bool MediaTypeHasChildren(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IMediaTypeRepository>();
                var query = repository.Query.Where(x => x.ParentId == id);
                int count = repository.Count(query);
                return count > 0;
            }
        }

        /// <summary>
        /// Checks whether an <see cref="IMediaType"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IMediaType"/></param>
        /// <returns>True if the media type has any children otherwise False</returns>
        public bool MediaTypeHasChildren(Guid id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IMediaTypeRepository>();
                var found = GetMediaType(id);
                if (found == null) return false;
                var query = repository.Query.Where(x => x.ParentId == found.Id);
                int count = repository.Count(query);
                return count > 0;
            }
        }

        public Attempt<OperationStatus<MoveOperationStatusType>> MoveMediaType(IMediaType toMove, int containerId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            if (MovingMediaType.IsRaisedEventCancelled(
                  new MoveEventArgs<IMediaType>(evtMsgs, new MoveEventInfo<IMediaType>(toMove, toMove.Path, containerId)),
                  this))
            {
                return OperationStatus.Attempt.Fail(MoveOperationStatusType.FailedCancelledByEvent, evtMsgs);
            }

            var moveInfo = new List<MoveEventInfo<IMediaType>>();
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var containerRepository = uow.CreateRepository<IMediaTypeContainerRepository>();
                var repository = uow.CreateRepository<IMediaTypeRepository>();

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
                    return OperationStatus.Attempt.Fail(ex.Operation, evtMsgs);
                }
                uow.Complete();
            }

            MovedMediaType.RaiseEvent(new MoveEventArgs<IMediaType>(false, evtMsgs, moveInfo.ToArray()), this);

            return OperationStatus.Attempt.Succeed(MoveOperationStatusType.Success, evtMsgs);
        }

        public Attempt<OperationStatus<MoveOperationStatusType>> MoveContentType(IContentType toMove, int containerId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            if (MovingContentType.IsRaisedEventCancelled(
                  new MoveEventArgs<IContentType>(evtMsgs, new MoveEventInfo<IContentType>(toMove, toMove.Path, containerId)),
                  this))
            {
                return OperationStatus.Attempt.Fail(MoveOperationStatusType.FailedCancelledByEvent, evtMsgs);
            }

            var moveInfo = new List<MoveEventInfo<IContentType>>();
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var containerRepository = uow.CreateRepository<IDocumentTypeContainerRepository>();
                var repository = uow.CreateRepository<IContentTypeRepository>();

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
                    return OperationStatus.Attempt.Fail(ex.Operation, evtMsgs);
                }
                uow.Complete();
            }

            MovedContentType.RaiseEvent(new MoveEventArgs<IContentType>(false, evtMsgs, moveInfo.ToArray()), this);

            return OperationStatus.Attempt.Succeed(MoveOperationStatusType.Success, evtMsgs);
        }

        public Attempt<OperationStatus<MoveOperationStatusType, IMediaType>> CopyMediaType(IMediaType toCopy, int containerId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            IMediaType copy;
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var containerRepository = uow.CreateRepository<IMediaTypeContainerRepository>();
                var repository = uow.CreateRepository<IMediaTypeRepository>();
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
                    return OperationStatus.Attempt.Fail<MoveOperationStatusType, IMediaType>(ex.Operation, evtMsgs);
                }
                uow.Complete();
            }

            return OperationStatus.Attempt.Succeed(MoveOperationStatusType.Success, evtMsgs, copy);
        }

        public Attempt<OperationStatus<MoveOperationStatusType, IContentType>> CopyContentType(IContentType toCopy, int containerId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            IContentType copy;
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var containerRepository = uow.CreateRepository<IDocumentTypeContainerRepository>();
                var repository = uow.CreateRepository<IContentTypeRepository>();
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
                    return OperationStatus.Attempt.Fail<MoveOperationStatusType, IContentType>(ex.Operation, evtMsgs);
                }
                uow.Complete();
            }

            return OperationStatus.Attempt.Succeed(MoveOperationStatusType.Success, evtMsgs, copy);
        }

        /// <summary>
        /// Saves a single <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="mediaType"><see cref="IMediaType"/> to save</param>
        /// <param name="userId">Optional Id of the user saving the MediaType</param>
        public void Save(IMediaType mediaType, int userId = 0)
        {
	        if (SavingMediaType.IsRaisedEventCancelled(new SaveEventArgs<IMediaType>(mediaType), this))
				return;

            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IMediaTypeRepository>();
                    ValidateLocked(mediaType); // throws if invalid
                    mediaType.CreatorId = userId;
                    repository.AddOrUpdate(mediaType);
                    uow.Complete();

                }

                UpdateContentXmlStructure(mediaType);
            }

            SavedMediaType.RaiseEvent(new SaveEventArgs<IMediaType>(mediaType, false), this);
	        Audit(AuditType.Save, string.Format("Save MediaType performed by user"), userId, mediaType.Id);
        }

        /// <summary>
        /// Saves a collection of <see cref="IMediaType"/> objects
        /// </summary>
        /// <param name="mediaTypes">Collection of <see cref="IMediaType"/> to save</param>
        /// <param name="userId">Optional Id of the user savging the MediaTypes</param>
        public void Save(IEnumerable<IMediaType> mediaTypes, int userId = 0)
        {
            var asArray = mediaTypes.ToArray();

            if (SavingMediaType.IsRaisedEventCancelled(new SaveEventArgs<IMediaType>(asArray), this))
				return;

            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IMediaTypeRepository>();
                    // all-or-nothing, validate them all first
                    foreach (var mediaType in asArray)
                    {
                        ValidateLocked(mediaType); // throws if invalid
                    }
                    foreach (var mediaType in asArray)
                    {
                        mediaType.CreatorId = userId;
                        repository.AddOrUpdate(mediaType);
                    }

                    //save it all in one go
                    uow.Complete();
                }

                UpdateContentXmlStructure(asArray.Cast<IContentTypeBase>().ToArray());
            }

            SavedMediaType.RaiseEvent(new SaveEventArgs<IMediaType>(asArray, false), this);
			Audit(AuditType.Save, string.Format("Save MediaTypes performed by user"), userId, -1);
        }

        /// <summary>
        /// Deletes a single <see cref="IMediaType"/> object
        /// </summary>
        /// <param name="mediaType"><see cref="IMediaType"/> to delete</param>
        /// <param name="userId">Optional Id of the user deleting the MediaType</param>
        /// <remarks>Deleting a <see cref="IMediaType"/> will delete all the <see cref="IMedia"/> objects based on this <see cref="IMediaType"/></remarks>
        public void Delete(IMediaType mediaType, int userId = 0)
        {
	        if (DeletingMediaType.IsRaisedEventCancelled(new DeleteEventArgs<IMediaType>(mediaType), this))
				return;
            using (new WriteLock(Locker))
            {
                _mediaService.DeleteMediaOfType(mediaType.Id, userId);

                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IMediaTypeRepository>();

                    repository.Delete(mediaType);
                    uow.Complete();

                    DeletedMediaType.RaiseEvent(new DeleteEventArgs<IMediaType>(mediaType, false), this);
                }

                Audit(AuditType.Delete, string.Format("Delete MediaType performed by user"), userId, mediaType.Id);
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
            var asArray = mediaTypes.ToArray();

            if (DeletingMediaType.IsRaisedEventCancelled(new DeleteEventArgs<IMediaType>(asArray), this))
				return;
            using (new WriteLock(Locker))
            {
                foreach (var mediaType in asArray)
                {
                    _mediaService.DeleteMediaOfType(mediaType.Id);
                }

                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repository = uow.CreateRepository<IMediaTypeRepository>();
                    foreach (var mediaType in asArray)
                    {
                        repository.Delete(mediaType);
                    }
                    uow.Complete();

                    DeletedMediaType.RaiseEvent(new DeleteEventArgs<IMediaType>(asArray, false), this);
                }

                Audit(AuditType.Delete, string.Format("Delete MediaTypes performed by user"), userId, -1);
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
            try
            {
                var strictSchemaBuilder = new StringBuilder();

                var contentTypes = GetAllContentTypes();
                foreach (ContentType contentType in contentTypes)
                {
                    string safeAlias = contentType.Alias.ToSafeAlias();
                    if (safeAlias != null)
                    {
                        strictSchemaBuilder.AppendLine(String.Format("<!ELEMENT {0} ANY>", safeAlias));
                        strictSchemaBuilder.AppendLine(String.Format("<!ATTLIST {0} id ID #REQUIRED>", safeAlias));
                    }
                }

                // Only commit the strong schema to the container if we didn't generate an error building it
                dtd.Append(strictSchemaBuilder);
            }
            catch (Exception exception)
            {
                LogHelper.Error<ContentTypeService>("Error while trying to build DTD for Xml schema; is Umbraco installed correctly and the connection string configured?", exception);
            }
            return dtd.ToString();
        }

        private void Audit(AuditType type, string message, int userId, int objectId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IAuditRepository>();
                repo.AddOrUpdate(new AuditItem(objectId, message, type, userId));
                uow.Complete();
            }
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