using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    /// <summary>
    /// Represents the DataType Service, which is an easy access to operations involving <see cref="IDataType"/>
    /// </summary>
    public class DataTypeService : ScopeRepositoryService, IDataTypeService
    {
        private readonly IDataTypeRepository _dataTypeRepository;
        private readonly IDataTypeContainerRepository _dataTypeContainerRepository;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly IAuditRepository _auditRepository;
        private readonly IEntityRepository _entityRepository;

        public DataTypeService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory,
            IDataTypeRepository dataTypeRepository, IDataTypeContainerRepository dataTypeContainerRepository,
            IAuditRepository auditRepository, IEntityRepository entityRepository, IContentTypeRepository contentTypeRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _dataTypeRepository = dataTypeRepository;
            _dataTypeContainerRepository = dataTypeContainerRepository;
            _auditRepository = auditRepository;
            _entityRepository = entityRepository;
            _contentTypeRepository = contentTypeRepository;
        }

        #region Containers

        public Attempt<OperationResult<OperationResultType, EntityContainer>> CreateContainer(int parentId, string name, int userId = Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var scope = ScopeProvider.CreateScope())
            {
                try
                {
                    var container = new EntityContainer(Constants.ObjectTypes.DataType)
                    {
                        Name = name,
                        ParentId = parentId,
                        CreatorId = userId
                    };

                    if (scope.Events.DispatchCancelable(SavingContainer, this, new SaveEventArgs<EntityContainer>(container, evtMsgs)))
                    {
                        scope.Complete();
                        return OperationResult.Attempt.Cancel(evtMsgs, container);
                    }

                    _dataTypeContainerRepository.Save(container);
                    scope.Complete();

                    scope.Events.Dispatch(SavedContainer, this, new SaveEventArgs<EntityContainer>(container, evtMsgs));
                    // TODO: Audit trail ?

                    return OperationResult.Attempt.Succeed(evtMsgs, container);
                }
                catch (Exception ex)
                {
                    return OperationResult.Attempt.Fail<EntityContainer>(evtMsgs, ex);
                }
            }
        }

        public EntityContainer GetContainer(int containerId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _dataTypeContainerRepository.Get(containerId);
            }
        }

        public EntityContainer GetContainer(Guid containerId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return ((EntityContainerRepository) _dataTypeContainerRepository).Get(containerId);
            }
        }

        public IEnumerable<EntityContainer> GetContainers(string name, int level)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return ((EntityContainerRepository) _dataTypeContainerRepository).Get(name, level);
            }
        }

        public IEnumerable<EntityContainer> GetContainers(IDataType dataType)
        {
            var ancestorIds = dataType.Path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    var asInt = x.TryConvertTo<int>();
                    return asInt ? asInt.Result : int.MinValue;
                })
                .Where(x => x != int.MinValue && x != dataType.Id)
                .ToArray();

            return GetContainers(ancestorIds);
        }

        public IEnumerable<EntityContainer> GetContainers(int[] containerIds)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _dataTypeContainerRepository.GetMany(containerIds);
            }
        }

        public Attempt<OperationResult> SaveContainer(EntityContainer container, int userId = Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();

            if (container.ContainedObjectType != Constants.ObjectTypes.DataType)
            {
                var ex = new InvalidOperationException("Not a " + Constants.ObjectTypes.DataType + " container.");
                return OperationResult.Attempt.Fail(evtMsgs, ex);
            }

            if (container.HasIdentity && container.IsPropertyDirty("ParentId"))
            {
                var ex = new InvalidOperationException("Cannot save a container with a modified parent, move the container instead.");
                return OperationResult.Attempt.Fail(evtMsgs, ex);
            }

            using (var scope = ScopeProvider.CreateScope())
            {
                if (scope.Events.DispatchCancelable(SavingContainer, this, new SaveEventArgs<EntityContainer>(container, evtMsgs)))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Cancel(evtMsgs);
                }

                _dataTypeContainerRepository.Save(container);

                scope.Events.Dispatch(SavedContainer, this, new SaveEventArgs<EntityContainer>(container, evtMsgs));
                scope.Complete();
            }

            // TODO: Audit trail ?
            return OperationResult.Attempt.Succeed(evtMsgs);
        }

        public Attempt<OperationResult> DeleteContainer(int containerId, int userId = Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var scope = ScopeProvider.CreateScope())
            {
                var container = _dataTypeContainerRepository.Get(containerId);
                if (container == null) return OperationResult.Attempt.NoOperation(evtMsgs);

                // 'container' here does not know about its children, so we need
                // to get it again from the entity repository, as a light entity
                var entity = _entityRepository.Get(container.Id);
                if (entity.HasChildren)
                {
                    scope.Complete();
                    return Attempt.Fail(new OperationResult(OperationResultType.FailedCannot, evtMsgs));
                }

                if (scope.Events.DispatchCancelable(DeletingContainer, this, new DeleteEventArgs<EntityContainer>(container, evtMsgs)))
                {
                    scope.Complete();
                    return Attempt.Fail(new OperationResult(OperationResultType.FailedCancelledByEvent, evtMsgs));
                }

                _dataTypeContainerRepository.Delete(container);

                scope.Events.Dispatch(DeletedContainer, this, new DeleteEventArgs<EntityContainer>(container, evtMsgs));
                scope.Complete();
            }

            // TODO: Audit trail ?
            return OperationResult.Attempt.Succeed(evtMsgs);
        }

        public Attempt<OperationResult<OperationResultType, EntityContainer>> RenameContainer(int id, string name, int userId = Constants.Security.SuperUserId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            using (var scope = ScopeProvider.CreateScope())
            {
                try
                {
                    var container = _dataTypeContainerRepository.Get(id);

                    //throw if null, this will be caught by the catch and a failed returned
                    if (container == null)
                        throw new InvalidOperationException("No container found with id " + id);

                    container.Name = name;

                    _dataTypeContainerRepository.Save(container);
                    scope.Complete();

                    // TODO: triggering SavedContainer with a different name?!
                    scope.Events.Dispatch(SavedContainer, this, new SaveEventArgs<EntityContainer>(container, evtMsgs), "RenamedContainer");

                    return OperationResult.Attempt.Succeed(OperationResultType.Success, evtMsgs, container);
                }
                catch (Exception ex)
                {
                    return OperationResult.Attempt.Fail<EntityContainer>(evtMsgs, ex);
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its Name
        /// </summary>
        /// <param name="name">Name of the <see cref="IDataType"/></param>
        /// <returns><see cref="IDataType"/></returns>
        public IDataType GetDataType(string name)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _dataTypeRepository.Get(Query<IDataType>().Where(x => x.Name == name)).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataType"/></param>
        /// <returns><see cref="IDataType"/></returns>
        public IDataType GetDataType(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _dataTypeRepository.Get(id);
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its unique guid Id
        /// </summary>
        /// <param name="id">Unique guid Id of the DataType</param>
        /// <returns><see cref="IDataType"/></returns>
        public IDataType GetDataType(Guid id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IDataType>().Where(x => x.Key == id);
                return _dataTypeRepository.Get(query).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its control Id
        /// </summary>
        /// <param name="propertyEditorAlias">Alias of the property editor</param>
        /// <returns>Collection of <see cref="IDataType"/> objects with a matching control id</returns>
        public IEnumerable<IDataType> GetByEditorAlias(string propertyEditorAlias)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IDataType>().Where(x => x.EditorAlias == propertyEditorAlias);
                return _dataTypeRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets all <see cref="IDataType"/> objects or those with the ids passed in
        /// </summary>
        /// <param name="ids">Optional array of Ids</param>
        /// <returns>An enumerable list of <see cref="IDataType"/> objects</returns>
        public IEnumerable<IDataType> GetAll(params int[] ids)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _dataTypeRepository.GetMany(ids);
            }
        }

        public Attempt<OperationResult<MoveOperationStatusType>> Move(IDataType toMove, int parentId)
        {
            var evtMsgs = EventMessagesFactory.Get();
            var moveInfo = new List<MoveEventInfo<IDataType>>();

            using (var scope = ScopeProvider.CreateScope())
            {
                var moveEventInfo = new MoveEventInfo<IDataType>(toMove, toMove.Path, parentId);
                var moveEventArgs = new MoveEventArgs<IDataType>(evtMsgs, moveEventInfo);
                if (scope.Events.DispatchCancelable(Moving, this, moveEventArgs))
                {
                    scope.Complete();
                    return OperationResult.Attempt.Fail(MoveOperationStatusType.FailedCancelledByEvent, evtMsgs);
                }

                try
                {
                    EntityContainer container = null;
                    if (parentId > 0)
                    {
                        container = _dataTypeContainerRepository.Get(parentId);
                        if (container == null)
                            throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound); // causes rollback
                    }
                    moveInfo.AddRange(_dataTypeRepository.Move(toMove, container));

                    moveEventArgs.MoveInfoCollection = moveInfo;
                    moveEventArgs.CanCancel = false;
                    scope.Events.Dispatch(Moved, this, moveEventArgs);
                    scope.Complete();
                }
                catch (DataOperationException<MoveOperationStatusType> ex)
                {
                    scope.Complete(); // TODO: what are we doing here exactly?
                    return OperationResult.Attempt.Fail(ex.Operation, evtMsgs);
                }
            }

            return OperationResult.Attempt.Succeed(MoveOperationStatusType.Success, evtMsgs);
        }

        /// <summary>
        /// Saves an <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataType"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issuing the save</param>
        public void Save(IDataType dataType, int userId = Constants.Security.SuperUserId)
        {
            dataType.CreatorId = userId;

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IDataType>(dataType);
                if (scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                if (string.IsNullOrWhiteSpace(dataType.Name))
                {
                    throw new ArgumentException("Cannot save datatype with empty name.");
                }

                if (dataType.Name != null && dataType.Name.Length > 255)
                {
                    throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
                }

                _dataTypeRepository.Save(dataType);

                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Saved, this, saveEventArgs);
                Audit(AuditType.Save, userId, dataType.Id);
                scope.Complete();
            }
        }

        /// <summary>
        /// Saves a collection of <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issuing the save</param>
        public void Save(IEnumerable<IDataType> dataTypeDefinitions, int userId = Constants.Security.SuperUserId)
        {
            Save(dataTypeDefinitions, userId, true);
        }

        /// <summary>
        /// Saves a collection of <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issuing the save</param>
        /// <param name="raiseEvents">Boolean indicating whether or not to raise events</param>
        public void Save(IEnumerable<IDataType> dataTypeDefinitions, int userId, bool raiseEvents)
        {
            var dataTypeDefinitionsA = dataTypeDefinitions.ToArray();
            var saveEventArgs = new SaveEventArgs<IDataType>(dataTypeDefinitionsA);

            using (var scope = ScopeProvider.CreateScope())
            {
                if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                foreach (var dataTypeDefinition in dataTypeDefinitionsA)
                {
                    dataTypeDefinition.CreatorId = userId;
                    _dataTypeRepository.Save(dataTypeDefinition);
                }

                if (raiseEvents)
                {
                    saveEventArgs.CanCancel = false;
                    scope.Events.Dispatch(Saved, this, saveEventArgs);
                }
                Audit(AuditType.Save, userId, -1);

                scope.Complete();
            }
        }

        /// <summary>
        /// Deletes an <see cref="IDataType"/>
        /// </summary>
        /// <remarks>
        /// Please note that deleting a <see cref="IDataType"/> will remove
        /// all the <see cref="PropertyType"/> data that references this <see cref="IDataType"/>.
        /// </remarks>
        /// <param name="dataType"><see cref="IDataType"/> to delete</param>
        /// <param name="userId">Optional Id of the user issuing the deletion</param>
        public void Delete(IDataType dataType, int userId = Constants.Security.SuperUserId)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var deleteEventArgs = new DeleteEventArgs<IDataType>(dataType);
                if (scope.Events.DispatchCancelable(Deleting, this, deleteEventArgs))
                {
                    scope.Complete();
                    return;
                }

                // find ContentTypes using this IDataTypeDefinition on a PropertyType, and delete
                // TODO: media and members?!
                // TODO: non-group properties?!
                var query = Query<PropertyType>().Where(x => x.DataTypeId == dataType.Id);
                var contentTypes = _contentTypeRepository.GetByQuery(query);
                foreach (var contentType in contentTypes)
                {
                    foreach (var propertyGroup in contentType.PropertyGroups)
                    {
                        var types = propertyGroup.PropertyTypes.Where(x => x.DataTypeId == dataType.Id).ToList();
                        foreach (var propertyType in types)
                        {
                            propertyGroup.PropertyTypes.Remove(propertyType);
                        }
                    }

                    // so... we are modifying content types here. the service will trigger Deleted event,
                    // which will propagate to DataTypeCacheRefresher which will clear almost every cache
                    // there is to clear... and in addition published snapshot caches will clear themselves too, so
                    // this is probably safe although it looks... weird.
                    //
                    // what IS weird is that a content type is losing a property and we do NOT raise any
                    // content type event... so ppl better listen on the data type events too.

                    _contentTypeRepository.Save(contentType);
                }

                _dataTypeRepository.Delete(dataType);

                deleteEventArgs.CanCancel = false;
                scope.Events.Dispatch(Deleted, this, deleteEventArgs);
                Audit(AuditType.Delete, userId, dataType.Id);

                scope.Complete();
            }
        }

        public IReadOnlyDictionary<Udi, IEnumerable<string>> GetReferences(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete:true))
            {
                return _dataTypeRepository.FindUsages(id);
            }
        }

        private void Audit(AuditType type, int userId, int objectId)
        {
            _auditRepository.Save(new AuditItem(objectId, type, userId, ObjectTypes.GetName(UmbracoObjectTypes.DataType)));
        }

        #region Event Handlers

        public static event TypedEventHandler<IDataTypeService, SaveEventArgs<EntityContainer>> SavingContainer;
        public static event TypedEventHandler<IDataTypeService, SaveEventArgs<EntityContainer>> SavedContainer;
        public static event TypedEventHandler<IDataTypeService, DeleteEventArgs<EntityContainer>> DeletingContainer;
        public static event TypedEventHandler<IDataTypeService, DeleteEventArgs<EntityContainer>> DeletedContainer;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IDataTypeService, DeleteEventArgs<IDataType>> Deleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IDataTypeService, DeleteEventArgs<IDataType>> Deleted;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IDataTypeService, SaveEventArgs<IDataType>> Saving;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IDataTypeService, SaveEventArgs<IDataType>> Saved;

        /// <summary>
        /// Occurs before Move
        /// </summary>
        public static event TypedEventHandler<IDataTypeService, MoveEventArgs<IDataType>> Moving;

        /// <summary>
        /// Occurs after Move
        /// </summary>
        public static event TypedEventHandler<IDataTypeService, MoveEventArgs<IDataType>> Moved;
        #endregion
    }
}
