using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.Implement
{
    /// <summary>
    /// Represents the DataType Service, which is an easy access to operations involving <see cref="IDataType"/>
    /// </summary>
    public class DataTypeService : RepositoryService, IDataTypeService
    {
        private readonly IDataValueEditorFactory _dataValueEditorFactory;
        private readonly IDataTypeRepository _dataTypeRepository;
        private readonly IDataTypeContainerRepository _dataTypeContainerRepository;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly IAuditRepository _auditRepository;
        private readonly IIOHelper _ioHelper;
        private readonly IDataTypeContainerService _dataTypeContainerService;
        private readonly IUserIdKeyResolver _userIdKeyResolver;
        private readonly Lazy<IIdKeyMap> _idKeyMap;

        public DataTypeService(
            ICoreScopeProvider provider,
            ILoggerFactory loggerFactory,
            IEventMessagesFactory eventMessagesFactory,
            IDataTypeRepository dataTypeRepository,
            IDataValueEditorFactory dataValueEditorFactory,
            IAuditRepository auditRepository,
            IContentTypeRepository contentTypeRepository,
            IIOHelper ioHelper,
            Lazy<IIdKeyMap> idKeyMap)
            : base(provider, loggerFactory, eventMessagesFactory)
        {
            _dataValueEditorFactory = dataValueEditorFactory;
            _dataTypeRepository = dataTypeRepository;
            _auditRepository = auditRepository;
            _contentTypeRepository = contentTypeRepository;
            _ioHelper = ioHelper;
            _idKeyMap = idKeyMap;

            // resolve dependencies for obsolete methods through the static service provider, so they don't pollute the constructor signature
            _dataTypeContainerService = StaticServiceProvider.Instance.GetRequiredService<IDataTypeContainerService>();
            _dataTypeContainerRepository = StaticServiceProvider.Instance.GetRequiredService<IDataTypeContainerRepository>();
            _userIdKeyResolver = StaticServiceProvider.Instance.GetRequiredService<IUserIdKeyResolver>();
        }

        #region Containers

        [Obsolete($"Please use {nameof(IDataTypeContainerService)} for all data type container operations. Will be removed in V15.")]
        public Attempt<OperationResult<OperationResultType, EntityContainer>?> CreateContainer(int parentId, Guid key, string name, int userId = Constants.Security.SuperUserId)
        {
            EventMessages evtMsgs = EventMessagesFactory.Get();
            using (ScopeProvider.CreateCoreScope(autoComplete:true))
            {
                try
                {
                    Guid? parentKey = parentId > 0 ? _dataTypeContainerRepository.Get(parentId)?.Key : null;
                    Guid currentUserKey = _userIdKeyResolver.GetAsync(userId).GetAwaiter().GetResult();
                    Attempt<EntityContainer?, EntityContainerOperationStatus> result = _dataTypeContainerService.CreateAsync(key, name, parentKey, currentUserKey).GetAwaiter().GetResult();

                    // mimic old service behavior
                    return result.Status switch
                    {
                        EntityContainerOperationStatus.CancelledByNotification => OperationResult.Attempt.Cancel(evtMsgs, new EntityContainer(Constants.ObjectTypes.DataType)),
                        EntityContainerOperationStatus.Success => OperationResult.Attempt.Succeed(evtMsgs, result.Result ?? throw new NullReferenceException("Container creation operation succeeded but the result was null")),
                        _ => throw new InvalidOperationException($"Invalid operation status: {result.Status}")
                    };
                }
                catch (Exception ex)
                {
                    return OperationResult.Attempt.Fail<EntityContainer>(evtMsgs, ex);
                }
            }
        }

        [Obsolete($"Please use {nameof(IDataTypeContainerService)} for all data type container operations. Will be removed in V16.")]
        public EntityContainer? GetContainer(int containerId)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            return _dataTypeContainerRepository.Get(containerId);
        }

        [Obsolete($"Please use {nameof(IDataTypeContainerService)} for all data type container operations. Will be removed in V16.")]
        public EntityContainer? GetContainer(Guid containerId)
            => _dataTypeContainerService.GetAsync(containerId).GetAwaiter().GetResult();

        [Obsolete($"Please use {nameof(IDataTypeContainerService)} for all data type container operations. Will be removed in V16.")]
        public IEnumerable<EntityContainer> GetContainers(string name, int level)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            return _dataTypeContainerRepository.Get(name, level);
        }

        [Obsolete($"Please use {nameof(IDataTypeContainerService)} for all data type container operations. Will be removed in V16.")]
        public IEnumerable<EntityContainer> GetContainers(IDataType dataType)
        {
            var ancestorIds = dataType.Path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    Attempt<int> asInt = x.TryConvertTo<int>();
                    return asInt.Success ? asInt.Result : int.MinValue;
                })
                .Where(x => x != int.MinValue && x != dataType.Id)
                .ToArray();

            return GetContainers(ancestorIds);
        }

        [Obsolete($"Please use {nameof(IDataTypeContainerService)} for all data type container operations. Will be removed in V16.")]
        public IEnumerable<EntityContainer> GetContainers(int[] containerIds)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            return _dataTypeContainerRepository.GetMany(containerIds);
        }

        [Obsolete($"Please use {nameof(IDataTypeContainerService)} for all data type container operations. Will be removed in V16.")]
        public Attempt<OperationResult?> SaveContainer(EntityContainer container, int userId = Constants.Security.SuperUserId)
        {
            EventMessages evtMsgs = EventMessagesFactory.Get();
            using (ScopeProvider.CreateCoreScope(autoComplete:true))
            {
                var isNew = container.Id == 0;
                Guid? parentKey = isNew && container.ParentId > 0 ? _dataTypeContainerRepository.Get(container.ParentId)?.Key : null;
                Guid currentUserKey = _userIdKeyResolver.GetAsync(userId).GetAwaiter().GetResult();

                Attempt<EntityContainer?, EntityContainerOperationStatus> result = isNew
                    ? _dataTypeContainerService.CreateAsync(container.Key, container.Name.IfNullOrWhiteSpace(string.Empty), parentKey, currentUserKey).GetAwaiter().GetResult()
                    : _dataTypeContainerService.UpdateAsync(container.Key, container.Name.IfNullOrWhiteSpace(string.Empty), currentUserKey).GetAwaiter().GetResult();

                // mimic old service behavior
                return result.Status switch
                {
                    EntityContainerOperationStatus.Success => OperationResult.Attempt.Succeed(evtMsgs),
                    EntityContainerOperationStatus.CancelledByNotification => OperationResult.Attempt.Cancel(evtMsgs),
                    EntityContainerOperationStatus.ParentNotFound => OperationResult.Attempt.Fail(evtMsgs, new InvalidOperationException("Cannot save a container with a modified parent, move the container instead.")),
                    EntityContainerOperationStatus.InvalidObjectType => OperationResult.Attempt.Fail(evtMsgs, new InvalidOperationException("Not a " + Constants.ObjectTypes.DataType + " container.")),
                    _ => OperationResult.Attempt.Fail(evtMsgs, new InvalidOperationException($"Invalid operation status: {result.Status}"))
                };
            }
        }

        [Obsolete($"Please use {nameof(IDataTypeContainerService)} for all data type container operations. Will be removed in V16.")]
        public Attempt<OperationResult?> DeleteContainer(int containerId, int userId = Constants.Security.SuperUserId)
        {
            EventMessages evtMsgs = EventMessagesFactory.Get();
            using (ScopeProvider.CreateCoreScope(autoComplete:true))
            {
                EntityContainer? container = _dataTypeContainerRepository.Get(containerId);
                if (container == null)
                {
                    return OperationResult.Attempt.NoOperation(evtMsgs);
                }

                Guid currentUserKey = _userIdKeyResolver.GetAsync(userId).GetAwaiter().GetResult();
                Attempt<EntityContainer?, EntityContainerOperationStatus> result = _dataTypeContainerService.DeleteAsync(container.Key, currentUserKey).GetAwaiter().GetResult();
                // mimic old service behavior
                return result.Status switch
                {
                    EntityContainerOperationStatus.Success => OperationResult.Attempt.Succeed(evtMsgs),
                    EntityContainerOperationStatus.NotEmpty => Attempt.Fail(new OperationResult(OperationResultType.FailedCannot, evtMsgs)),
                    EntityContainerOperationStatus.CancelledByNotification => Attempt.Fail(new OperationResult(OperationResultType.FailedCancelledByEvent, evtMsgs)),
                    _ => OperationResult.Attempt.Fail(evtMsgs, new InvalidOperationException($"Invalid operation status: {result.Status}"))
                };
            }
        }

        [Obsolete($"Please use {nameof(IDataTypeContainerService)} for all data type container operations. Will be removed in V16.")]
        public Attempt<OperationResult<OperationResultType, EntityContainer>?> RenameContainer(int id, string name, int userId = Constants.Security.SuperUserId)
        {
            EventMessages evtMsgs = EventMessagesFactory.Get();
            using (ScopeProvider.CreateCoreScope(autoComplete:true))
            {
                try
                {
                    EntityContainer? container = _dataTypeContainerRepository.Get(id);

                    //throw if null, this will be caught by the catch and a failed returned
                    if (container == null)
                    {
                        throw new InvalidOperationException("No container found with id " + id);
                    }

                    container.Name = name;
                    Guid currentUserKey = _userIdKeyResolver.GetAsync(userId).GetAwaiter().GetResult();
                    Attempt<EntityContainer?, EntityContainerOperationStatus> result = _dataTypeContainerService.UpdateAsync(container.Key, container.Name, currentUserKey).GetAwaiter().GetResult();
                    // mimic old service behavior
                    return result.Status switch
                    {
                        EntityContainerOperationStatus.Success => OperationResult.Attempt.Succeed(OperationResultType.Success, evtMsgs, result.Result ?? throw new NullReferenceException("Container update operation succeeded but the result was null")),
                        EntityContainerOperationStatus.CancelledByNotification => OperationResult.Attempt.Cancel(evtMsgs, container),
                        _ => OperationResult.Attempt.Fail<EntityContainer>(evtMsgs, new InvalidOperationException($"Invalid operation status: {result.Status}"))
                    };
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
        [Obsolete("Please use GetAsync. Will be removed in V15.")]
        public IDataType? GetDataType(string name)
            => GetAsync(name).GetAwaiter().GetResult();

        /// <inheritdoc />
        public Task<IDataType?> GetAsync(string name)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IDataType? dataType = _dataTypeRepository.Get(Query<IDataType>().Where(x => x.Name == name))?.FirstOrDefault();
            ConvertMissingEditorOfDataTypeToLabel(dataType);

            return Task.FromResult(dataType);
        }

        /// <inheritdoc />
        public Task<IEnumerable<IDataType>> GetAllAsync(params Guid[] keys)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

            IQuery<IDataType> query = Query<IDataType>();
            if (keys.Length > 0)
            {
                query = query.Where(x => keys.Contains(x.Key));
            }

            IDataType[] dataTypes = _dataTypeRepository.Get(query).ToArray();
            ConvertMissingEditorsOfDataTypesToLabels(dataTypes);

            return Task.FromResult<IEnumerable<IDataType>>(dataTypes);
        }

        /// <inheritdoc />
        public Task<PagedModel<IDataType>> FilterAsync(string? name = null, string? editorUiAlias = null, string? editorAlias = null, int skip = 0, int take = 100)
        {
            IEnumerable<IDataType> query = GetAll();

            if (name is not null)
            {
                query = query.Where(datatype => datatype.Name?.InvariantContains(name) ?? false);
            }

            if (editorUiAlias != null)
            {
                query = query.Where(datatype => datatype.EditorUiAlias?.InvariantContains(editorUiAlias) ?? false);
            }

            if (editorAlias != null)
            {
                query = query.Where(datatype => datatype.EditorAlias.InvariantContains(editorAlias));
            }

            IDataType[] result = query.ToArray();

            return Task.FromResult(new PagedModel<IDataType>
            {
                Total = result.Length,
                Items = result.Skip(skip).Take(take),
            });
        }

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataType"/></param>
        /// <returns><see cref="IDataType"/></returns>
        [Obsolete("Please use GetAsync. Will be removed in V15.")]
        public IDataType? GetDataType(int id)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IDataType? dataType = _dataTypeRepository.Get(id);
            ConvertMissingEditorOfDataTypeToLabel(dataType);

            return dataType;
        }

        /// <inheritdoc />
        public Task<IDataType?> GetAsync(Guid id)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IDataType? dataType = GetDataTypeFromRepository(id);
            ConvertMissingEditorOfDataTypeToLabel(dataType);

            return Task.FromResult(dataType);
        }

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its control Id
        /// </summary>
        /// <param name="propertyEditorAlias">Alias of the property editor</param>
        /// <returns>Collection of <see cref="IDataType"/> objects with a matching control id</returns>
        [Obsolete("Please use GetByEditorAliasAsync. Will be removed in V15.")]
        public IEnumerable<IDataType> GetByEditorAlias(string propertyEditorAlias)
            => GetByEditorAliasAsync(propertyEditorAlias).GetAwaiter().GetResult();

        /// <inheritdoc />
        public Task<IEnumerable<IDataType>> GetByEditorAliasAsync(string propertyEditorAlias)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IQuery<IDataType> query = Query<IDataType>().Where(x => x.EditorAlias == propertyEditorAlias);
            IEnumerable<IDataType> dataTypes = _dataTypeRepository.Get(query).ToArray();
            ConvertMissingEditorsOfDataTypesToLabels(dataTypes);

            return Task.FromResult(dataTypes);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDataType>> GetByEditorAliasAsync(string[] propertyEditorAlias)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IQuery<IDataType> query = Query<IDataType>().Where(x => propertyEditorAlias.Contains(x.EditorAlias));
            IEnumerable<IDataType> dataTypes = _dataTypeRepository.Get(query).ToArray();
            ConvertMissingEditorsOfDataTypesToLabels(dataTypes);
            return await Task.FromResult(dataTypes);
        }

        /// <inheritdoc />
        public Task<IEnumerable<IDataType>> GetByEditorUiAlias(string editorUiAlias)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IQuery<IDataType> query = Query<IDataType>().Where(x => x.EditorUiAlias == editorUiAlias);
            IEnumerable<IDataType> dataTypes = _dataTypeRepository.Get(query).ToArray();
            ConvertMissingEditorsOfDataTypesToLabels(dataTypes);

            return Task.FromResult(dataTypes);
        }

        /// <summary>
        /// Gets all <see cref="IDataType"/> objects or those with the ids passed in
        /// </summary>
        /// <param name="ids">Optional array of Ids</param>
        /// <returns>An enumerable list of <see cref="IDataType"/> objects</returns>
        public IEnumerable<IDataType> GetAll(params int[] ids)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IEnumerable<IDataType> dataTypes = _dataTypeRepository.GetMany(ids).ToArray();
            ConvertMissingEditorsOfDataTypesToLabels(dataTypes);

            return dataTypes;
        }

        private void ConvertMissingEditorOfDataTypeToLabel(IDataType? dataType)
        {
            if (dataType == null)
            {
                return;
            }

            ConvertMissingEditorsOfDataTypesToLabels(new[] { dataType });
        }

        private void ConvertMissingEditorsOfDataTypesToLabels(IEnumerable<IDataType> dataTypes)
        {
            // Any data types that don't have an associated editor are created of a specific type.
            // We convert them to labels to make clear to the user why the data type cannot be used.
            IEnumerable<IDataType> dataTypesWithMissingEditors = dataTypes.Where(x => x.Editor is MissingPropertyEditor);
            foreach (IDataType dataType in dataTypesWithMissingEditors)
            {
                dataType.Editor = new LabelPropertyEditor(_dataValueEditorFactory, _ioHelper);
            }
        }

        public Attempt<OperationResult<MoveOperationStatusType>?> Move(IDataType toMove, int parentId)
        {
            Guid? containerKey = null;
            if (parentId > 0)
            {
                // mimic obsolete Copy method behavior
                EntityContainer? container = GetContainer(parentId);
                if (container is null)
                {
                    throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound);
                }

                containerKey = container.Key;
            }

            Attempt<IDataType, DataTypeOperationStatus> result = MoveAsync(toMove, containerKey, Constants.Security.SuperUserKey).GetAwaiter().GetResult();

            // mimic old service behavior
            EventMessages evtMsgs = EventMessagesFactory.Get();
            return result.Status switch
            {
                DataTypeOperationStatus.Success => OperationResult.Attempt.Succeed(MoveOperationStatusType.Success, evtMsgs),
                DataTypeOperationStatus.CancelledByNotification => OperationResult.Attempt.Fail(MoveOperationStatusType.FailedCancelledByEvent, evtMsgs),
                DataTypeOperationStatus.ParentNotFound => OperationResult.Attempt.Fail(MoveOperationStatusType.FailedParentNotFound, evtMsgs),
                _ => OperationResult.Attempt.Fail<MoveOperationStatusType>(MoveOperationStatusType.FailedNotAllowedByPath, evtMsgs, new InvalidOperationException($"Invalid operation status: {result.Status}")),
            };
        }

        public async Task<Attempt<IDataType, DataTypeOperationStatus>> MoveAsync(IDataType toMove, Guid? containerKey, Guid userKey)
        {
            EventMessages eventMessages = EventMessagesFactory.Get();

            using (ICoreScope scope = ScopeProvider.CreateCoreScope())
            {
                EntityContainer? container = null;
                var parentId = Constants.System.Root;
                if (containerKey.HasValue && containerKey.Value != Guid.Empty)
                {
                    container = await _dataTypeContainerService.GetAsync(containerKey.Value);
                    if (container is null)
                    {
                        return Attempt.FailWithStatus(DataTypeOperationStatus.ParentNotFound, toMove);
                    }

                    parentId = container.Id;
                }

                if (toMove.ParentId == parentId)
                {
                    return Attempt.SucceedWithStatus(DataTypeOperationStatus.Success, toMove);
                }

                var moveEventInfo = new MoveEventInfo<IDataType>(toMove, toMove.Path, parentId, containerKey);
                var movingDataTypeNotification = new DataTypeMovingNotification(moveEventInfo, eventMessages);
                if (scope.Notifications.PublishCancelable(movingDataTypeNotification))
                {
                    scope.Complete();
                    return Attempt.FailWithStatus(DataTypeOperationStatus.CancelledByNotification, toMove);
                }

                _dataTypeRepository.Move(toMove, container);

                scope.Notifications.Publish(new DataTypeMovedNotification(moveEventInfo, eventMessages).WithStateFrom(movingDataTypeNotification));

                var currentUserId = await _userIdKeyResolver.GetAsync(userKey);
                Audit(AuditType.Move, currentUserId, toMove.Id);
                scope.Complete();
            }

            return Attempt.SucceedWithStatus(DataTypeOperationStatus.Success, toMove);
        }

        [Obsolete("Use the method which specifies the userId parameter")]
        public Attempt<OperationResult<MoveOperationStatusType, IDataType>?> Copy(IDataType copying, int containerId)
            => Copy(copying, containerId, Constants.Security.SuperUserId);

        public Attempt<OperationResult<MoveOperationStatusType, IDataType>?> Copy(IDataType copying, int containerId, int userId = Constants.Security.SuperUserId)
        {
            Guid? containerKey = null;
            if (containerId > 0)
            {
                // mimic obsolete Copy method behavior
                EntityContainer? container = GetContainer(containerId);
                if (container is null)
                {
                    throw new DataOperationException<MoveOperationStatusType>(MoveOperationStatusType.FailedParentNotFound);
                }

                containerKey = container.Key;
            }

            Guid currentUserKey = _userIdKeyResolver.GetAsync(userId).GetAwaiter().GetResult();
            Attempt<IDataType, DataTypeOperationStatus> result = CopyAsync(copying, containerKey, currentUserKey).GetAwaiter().GetResult();

            // mimic old service behavior
            EventMessages evtMsgs = EventMessagesFactory.Get();
            return result.Status switch
            {
                DataTypeOperationStatus.Success => OperationResult.Attempt.Succeed(MoveOperationStatusType.Success, evtMsgs, result.Result),
                DataTypeOperationStatus.CancelledByNotification => OperationResult.Attempt.Fail(MoveOperationStatusType.FailedCancelledByEvent, evtMsgs, result.Result),
                DataTypeOperationStatus.ParentNotFound => OperationResult.Attempt.Fail(MoveOperationStatusType.FailedParentNotFound, evtMsgs, result.Result),
                _ =>  OperationResult.Attempt.Fail(MoveOperationStatusType.FailedNotAllowedByPath, evtMsgs, result.Result, new InvalidOperationException($"Invalid operation status: {result.Status}")),
            };
        }

        /// <inheritdoc />
        public async Task<Attempt<IDataType, DataTypeOperationStatus>> CopyAsync(IDataType toCopy, Guid? containerKey, Guid userKey)
        {
            EntityContainer? container = null;
            if (containerKey.HasValue && containerKey.Value != Guid.Empty)
            {
                container = await _dataTypeContainerService.GetAsync(containerKey.Value);
                if (container is null)
                {
                    return Attempt.FailWithStatus(DataTypeOperationStatus.ParentNotFound, toCopy);
                }
            }

            IDataType copy = toCopy.DeepCloneWithResetIdentities();
            copy.Name += " (copy)"; // might not be unique
            copy.ParentId = container?.Id ?? Constants.System.Root;

            return await SaveAsync(copy, () => DataTypeOperationStatus.Success, userKey, AuditType.Copy);
        }

        /// <summary>
        /// Saves an <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataType"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issuing the save</param>
        [Obsolete("Please use CreateAsync or UpdateAsync. Will be removed in V15.")]
        public void Save(IDataType dataType, int userId = Constants.Security.SuperUserId)
        {
            // mimic old service behavior
            if (string.IsNullOrWhiteSpace(dataType.Name))
            {
                throw new ArgumentException("Cannot save datatype with empty name.");
            }

            if (dataType.Name != null && dataType.Name.Length > 255)
            {
                throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
            }

            Guid currentUserKey = _userIdKeyResolver.GetAsync(userId).GetAwaiter().GetResult();

            SaveAsync(
                dataType,
                () => DataTypeOperationStatus.Success,
                currentUserKey,
                AuditType.Sort).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public async Task<Attempt<IDataType, DataTypeOperationStatus>> CreateAsync(IDataType dataType, Guid userKey)
        {
            if (dataType.Id != 0)
            {
                return Attempt.FailWithStatus(DataTypeOperationStatus.InvalidId, dataType);
            }

            using ICoreScope scope = ScopeProvider.CreateCoreScope();

            if (_dataTypeRepository.Get(dataType.Key) is not null)
            {
                return Attempt.FailWithStatus(DataTypeOperationStatus.DuplicateKey, dataType);
            }

            Attempt<IDataType, DataTypeOperationStatus> result = await SaveAsync(dataType, () => DataTypeOperationStatus.Success, userKey, AuditType.New);

            scope.Complete();

            return result;
        }

        /// <inheritdoc />
        public async Task<Attempt<IDataType, DataTypeOperationStatus>> UpdateAsync(IDataType dataType, Guid userKey)
            => await SaveAsync(
                dataType,
                () =>
                {
                    IDataType? current = _dataTypeRepository.Get(dataType.Id);
                    return current == null
                        ? DataTypeOperationStatus.NotFound
                        : DataTypeOperationStatus.Success;
                },
                userKey,
                AuditType.New);

        /// <summary>
        /// Saves a collection of <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issuing the save</param>
        [Obsolete("Please use CreateAsync or UpdateAsync. Will be removed in V15.")]
        public void Save(IEnumerable<IDataType> dataTypeDefinitions, int userId)
        {
            EventMessages evtMsgs = EventMessagesFactory.Get();
            IDataType[] dataTypeDefinitionsA = dataTypeDefinitions.ToArray();

            using ICoreScope scope = ScopeProvider.CreateCoreScope();
            var savingDataTypeNotification = new DataTypeSavingNotification(dataTypeDefinitions, evtMsgs);
            if (scope.Notifications.PublishCancelable(savingDataTypeNotification))
            {
                scope.Complete();
                return;
            }

            foreach (IDataType dataTypeDefinition in dataTypeDefinitionsA)
            {
                dataTypeDefinition.CreatorId = userId;
                _dataTypeRepository.Save(dataTypeDefinition);
            }

            scope.Notifications.Publish(new DataTypeSavedNotification(dataTypeDefinitions, evtMsgs).WithStateFrom(savingDataTypeNotification));

            Audit(AuditType.Save, userId, -1);

            scope.Complete();
        }

        /// <summary>
        /// Deletes an <see cref="IDataType"/>
        /// </summary>
        /// <remarks>
        /// Please note that deleting a <see cref="IDataType"/> will remove
        /// all the <see cref="IPropertyType"/> data that references this <see cref="IDataType"/>.
        /// </remarks>
        /// <param name="dataType"><see cref="IDataType"/> to delete</param>
        /// <param name="userId">Optional Id of the user issuing the deletion</param>
        public void Delete(IDataType dataType, int userId = Constants.Security.SuperUserId)
        {
            Guid currentUserKey = _userIdKeyResolver.GetAsync(userId).GetAwaiter().GetResult();
            DeleteAsync(dataType.Key, currentUserKey).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public async Task<Attempt<IDataType?, DataTypeOperationStatus>> DeleteAsync(Guid id, Guid userKey)
        {
            EventMessages eventMessages = EventMessagesFactory.Get();
            using ICoreScope scope = ScopeProvider.CreateCoreScope();

            IDataType? dataType = GetDataTypeFromRepository(id);
            if (dataType == null)
            {
                return Attempt.FailWithStatus(DataTypeOperationStatus.NotFound, dataType);
            }

            if (dataType.IsDeletableDataType() is false)
            {
                scope.Complete();
                return Attempt.FailWithStatus<IDataType?, DataTypeOperationStatus>(DataTypeOperationStatus.NonDeletable, dataType);
            }

            var deletingDataTypeNotification = new DataTypeDeletingNotification(dataType, eventMessages);
            if (await scope.Notifications.PublishCancelableAsync(deletingDataTypeNotification))
            {
                scope.Complete();
                return Attempt.FailWithStatus<IDataType?, DataTypeOperationStatus>(DataTypeOperationStatus.CancelledByNotification, dataType);
            }

            // find ContentTypes using this IDataTypeDefinition on a PropertyType, and delete
            // TODO: media and members?!
            // TODO: non-group properties?!
            IQuery<PropertyType> query = Query<PropertyType>().Where(x => x.DataTypeId == dataType.Id);
            IEnumerable<IContentType> contentTypes = _contentTypeRepository.GetByQuery(query);
            foreach (IContentType contentType in contentTypes)
            {
                foreach (PropertyGroup propertyGroup in contentType.PropertyGroups)
                {
                    var types = propertyGroup.PropertyTypes?.Where(x => x.DataTypeId == dataType.Id).ToList();
                    if (types is not null)
                    {
                        foreach (IPropertyType propertyType in types)
                        {
                            propertyGroup.PropertyTypes?.Remove(propertyType);
                        }
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

            scope.Notifications.Publish(new DataTypeDeletedNotification(dataType, eventMessages).WithStateFrom(deletingDataTypeNotification));

            var currentUserId = await _userIdKeyResolver.GetAsync(userKey);
            Audit(AuditType.Delete, currentUserId, dataType.Id);

            scope.Complete();

            return Attempt.SucceedWithStatus<IDataType?, DataTypeOperationStatus>(DataTypeOperationStatus.Success, dataType);
        }

        /// <inheritdoc />
        public Task<Attempt<IReadOnlyDictionary<Udi, IEnumerable<string>>, DataTypeOperationStatus>> GetReferencesAsync(Guid id)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete:true);
            IDataType? dataType = GetDataTypeFromRepository(id);
            if (dataType == null)
            {
                return Task.FromResult(Attempt.FailWithStatus<IReadOnlyDictionary<Udi, IEnumerable<string>>, DataTypeOperationStatus>(DataTypeOperationStatus.NotFound, new Dictionary<Udi, IEnumerable<string>>()));
            }

            IReadOnlyDictionary<Udi, IEnumerable<string>> usages = _dataTypeRepository.FindUsages(dataType.Id);
            return Task.FromResult(Attempt.SucceedWithStatus(DataTypeOperationStatus.Success, usages));
        }

        public IReadOnlyDictionary<Udi, IEnumerable<string>> GetListViewReferences(int id)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            return _dataTypeRepository.FindListViewUsages(id);
        }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> ValidateConfigurationData(IDataType dataType)
        {
            IConfigurationEditor? configurationEditor = dataType.Editor?.GetConfigurationEditor();
            return configurationEditor == null
                ? new[]
                {
                    new ValidationResult($"Data type with editor alias {dataType.EditorAlias} does not have a configuration editor")
                }
                : configurationEditor.Validate(dataType.ConfigurationData);
        }

        private async Task<Attempt<IDataType, DataTypeOperationStatus>> SaveAsync(
            IDataType dataType,
            Func<DataTypeOperationStatus> operationValidation,
            Guid userKey,
            AuditType auditType)
        {
            IEnumerable<ValidationResult> validationResults = ValidateConfigurationData(dataType);
            if (validationResults.Any())
            {
                LoggerFactory
                    .CreateLogger<DataTypeService>()
                    .LogError($"Invalid data type configuration for data type: {dataType.Name} - validation error messages: {string.Join(Environment.NewLine, validationResults.Select(r => r.ErrorMessage))}");
                return Attempt.FailWithStatus(DataTypeOperationStatus.InvalidConfiguration, dataType);
            }

            EventMessages eventMessages = EventMessagesFactory.Get();

            var currentUserId = await _userIdKeyResolver.GetAsync(userKey);
            dataType.CreatorId = currentUserId;

            using ICoreScope scope = ScopeProvider.CreateCoreScope();

            DataTypeOperationStatus status = operationValidation();
            if (status != DataTypeOperationStatus.Success)
            {
                return Attempt.FailWithStatus(status, dataType);
            }

            var savingDataTypeNotification = new DataTypeSavingNotification(dataType, eventMessages);
            if (await scope.Notifications.PublishCancelableAsync(savingDataTypeNotification))
            {
                scope.Complete();
                return Attempt.FailWithStatus(DataTypeOperationStatus.CancelledByNotification, dataType);
            }

            if (string.IsNullOrWhiteSpace(dataType.Name))
            {
                return Attempt.FailWithStatus(DataTypeOperationStatus.InvalidName, dataType);
            }

            if (dataType.Name is { Length: > 255 })
            {
                return Attempt.FailWithStatus(DataTypeOperationStatus.InvalidName, dataType);
            }


            _dataTypeRepository.Save(dataType);

            scope.Notifications.Publish(new DataTypeSavedNotification(dataType, eventMessages).WithStateFrom(savingDataTypeNotification));

            Audit(auditType, currentUserId, dataType.Id);
            scope.Complete();

            return Attempt.SucceedWithStatus(DataTypeOperationStatus.Success, dataType);
        }

        private IDataType? GetDataTypeFromRepository(Guid id)
        => _idKeyMap.Value.GetIdForKey(id, UmbracoObjectTypes.DataType) switch
        {
            { Success: false } => null,
            { Result: var intId } => _dataTypeRepository.Get(intId),
        };

        private void Audit(AuditType type, int userId, int objectId)
            => _auditRepository.Save(new AuditItem(objectId, type, userId, ObjectTypes.GetName(UmbracoObjectTypes.DataType)));
    }
}
