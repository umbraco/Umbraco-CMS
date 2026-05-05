using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
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
        private readonly IDataTypeRepository _dataTypeRepository;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly IMediaTypeRepository _mediaTypeRepository;
        private readonly IMemberTypeRepository _memberTypeRepository;
        private readonly IAuditService _auditService;
        private readonly IDataTypeContainerService _dataTypeContainerService;
        private readonly IUserIdKeyResolver _userIdKeyResolver;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataTypeService" /> class.
        /// </summary>
        /// <param name="provider">The core scope provider.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="eventMessagesFactory">The event messages factory.</param>
        /// <param name="dataTypeRepository">The data type repository.</param>
        /// <param name="auditService">The audit service.</param>
        /// <param name="contentTypeRepository">The content type repository.</param>
        /// <param name="mediaTypeRepository">The media type repository.</param>
        /// <param name="memberTypeRepository">The member type repository.</param>
        /// <param name="dataTypeContainerService">The data type container service.</param>
        /// <param name="userIdKeyResolver">The user id-to-key resolver.</param>
        public DataTypeService(
            ICoreScopeProvider provider,
            ILoggerFactory loggerFactory,
            IEventMessagesFactory eventMessagesFactory,
            IDataTypeRepository dataTypeRepository,
            IAuditService auditService,
            IContentTypeRepository contentTypeRepository,
            IMediaTypeRepository mediaTypeRepository,
            IMemberTypeRepository memberTypeRepository,
            IDataTypeContainerService dataTypeContainerService,
            IUserIdKeyResolver userIdKeyResolver)
            : base(provider, loggerFactory, eventMessagesFactory)
        {
            _dataTypeRepository = dataTypeRepository;
            _auditService = auditService;
            _contentTypeRepository = contentTypeRepository;
            _mediaTypeRepository = mediaTypeRepository;
            _memberTypeRepository = memberTypeRepository;
            _dataTypeContainerService = dataTypeContainerService;
            _userIdKeyResolver = userIdKeyResolver;
        }

        /// <inheritdoc />
        public Task<IDataType?> GetAsync(string name)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IDataType? dataType = _dataTypeRepository.Get(Query<IDataType>().Where(x => x.Name == name))?.FirstOrDefault();

            return Task.FromResult(dataType);
        }

        /// <inheritdoc />
        public Task<IEnumerable<IDataType>> GetAllAsync(params Guid[] keys)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IEnumerable<IDataType> dataTypes = _dataTypeRepository.GetMany(keys);
            return Task.FromResult(dataTypes);
        }

        /// <inheritdoc />
        public async Task<PagedModel<IDataType>> FilterAsync(string? name = null, string? editorUiAlias = null, string? editorAlias = null, int skip = 0, int take = 100)
        {
            IEnumerable<IDataType> query = await GetAllAsync();

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

            return new PagedModel<IDataType>
            {
                Total = result.Length,
                Items = result.Skip(skip).Take(take),
            };
        }

        /// <inheritdoc />
        public Task<IDataType?> GetAsync(Guid id)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IDataType? dataType = _dataTypeRepository.Get(id);

            return Task.FromResult(dataType);
        }

        /// <inheritdoc />
        public Task<IEnumerable<IDataType>> GetByEditorAliasAsync(string propertyEditorAlias)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IQuery<IDataType> query = Query<IDataType>().Where(x => x.EditorAlias == propertyEditorAlias);
            IEnumerable<IDataType> dataTypes = _dataTypeRepository.Get(query).ToArray();

            return Task.FromResult(dataTypes);
        }

        /// <inheritdoc />
        public Task<IEnumerable<IDataType>> GetByEditorAliasAsync(string[] propertyEditorAlias)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

            // Need to use a List here because the expression tree cannot convert the array when used in Contains.
            // See ExpressionTests.Sql_In().
            List<string> propertyEditorAliasesAsList = [.. propertyEditorAlias];
            IQuery<IDataType> query = Query<IDataType>().Where(x => propertyEditorAliasesAsList.Contains(x.EditorAlias));

            IEnumerable<IDataType> dataTypes = _dataTypeRepository.Get(query).ToArray();
            return Task.FromResult(dataTypes);
        }

        /// <inheritdoc />
        public Task<IEnumerable<IDataType>> GetByEditorUiAlias(string editorUiAlias)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
            IQuery<IDataType> query = Query<IDataType>().Where(x => x.EditorUiAlias == editorUiAlias);
            IEnumerable<IDataType> dataTypes = _dataTypeRepository.Get(query).ToArray();

            return Task.FromResult(dataTypes);
        }

        /// <inheritdoc />
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

                await AuditAsync(AuditType.Move, userKey, toMove.Id);
                scope.Complete();
            }

            return Attempt.SucceedWithStatus(DataTypeOperationStatus.Success, toMove);
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
                AuditType.Save);

        /// <inheritdoc />
        public async Task<Attempt<IDataType?, DataTypeOperationStatus>> DeleteAsync(Guid id, Guid userKey)
        {
            EventMessages eventMessages = EventMessagesFactory.Get();
            using ICoreScope scope = ScopeProvider.CreateCoreScope();

            IDataType? dataType = _dataTypeRepository.Get(id);
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

            await AuditAsync(AuditType.Delete, userKey, dataType.Id);

            scope.Complete();

            return Attempt.SucceedWithStatus<IDataType?, DataTypeOperationStatus>(DataTypeOperationStatus.Success, dataType);
        }

        /// <inheritdoc />
        public Task<PagedModel<RelationItemModel>> GetPagedRelationsAsync(Guid key, int skip, int take)
        {
            using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

            IDataType? dataType = _dataTypeRepository.Get(key);
            if (dataType == null)
            {
                // Is an unexpected response, but returning an empty collection aligns with how we handle retrieval of concrete Umbraco
                // relations based on documents, media and members.
                return Task.FromResult(new PagedModel<RelationItemModel>());
            }

            // We don't really need true paging here, as the number of data type relations will be small compared to what there could
            // potentially by for concrete Umbraco relations based on documents, media and members.
            // So we'll retrieve all usages for the data type and construct a paged response.
            // This allows us to re-use the existing repository methods used for FindUsages and FindListViewUsages.
            IReadOnlyDictionary<Udi, IEnumerable<string>> usages = _dataTypeRepository.FindUsages(dataType.Id);
            IReadOnlyDictionary<Udi, IEnumerable<string>> listViewUsages = _dataTypeRepository.FindListViewUsages(dataType.Id);

            // Combine the property and list view usages into a single collection of property aliases and content type UDIs.
            IList<(string PropertyAlias, Udi Udi)> combinedUsages = usages
                .SelectMany(kvp => kvp.Value.Select(value => (value, kvp.Key)))
                .Concat(listViewUsages.SelectMany(kvp => kvp.Value.Select(value => (value, kvp.Key))))
                .ToList();

            var totalItems = combinedUsages.Count;

            // Create the page of items.
            List<(string PropertyAlias, Udi Udi)> pagedUsages = combinedUsages
                .OrderBy(x => x.Udi.EntityType) // Document types first, then media types, then member types.
                .ThenBy(x => x.PropertyAlias)
                .Skip(skip)
                .Take(take)
                .ToList();

            // Get the content types for the UDIs referenced in the page of items to construct the response from.
            // They could be document, media or member types.
            List<IContentTypeComposition> contentTypes = GetReferencedContentTypes(pagedUsages);

            IEnumerable<RelationItemModel> relations = pagedUsages
                .Select(x =>
                {
                    // Get the matching content type so we can populate the content type and property details.
                    IContentTypeComposition contentType = contentTypes.Single(y => y.Key == ((GuidUdi)x.Udi).Guid);

                    string nodeType = x.Udi.EntityType switch
                    {
                        Constants.UdiEntityType.DocumentType => Constants.ReferenceType.DocumentTypePropertyType,
                        Constants.UdiEntityType.MediaType => Constants.ReferenceType.MediaTypePropertyType,
                        Constants.UdiEntityType.MemberType => Constants.ReferenceType.MemberTypePropertyType,
                        _ => throw new ArgumentOutOfRangeException(nameof(x.Udi.EntityType)),
                    };

                    // Look-up the property details from the property alias. This will be null for a list view reference.
                    IPropertyType? propertyType = contentType.PropertyTypes.SingleOrDefault(y => y.Alias == x.PropertyAlias);
                    return new RelationItemModel
                    {
                        ContentTypeKey = contentType.Key,
                        ContentTypeAlias = contentType.Alias,
                        ContentTypeIcon = contentType.Icon,
                        ContentTypeName = contentType.Name,
                        NodeType = nodeType,
                        NodeName = propertyType?.Name ?? x.PropertyAlias,
                        NodeAlias = x.PropertyAlias,
                        NodeKey = propertyType?.Key ?? Guid.Empty,
                    };
                });

            var pagedModel = new PagedModel<RelationItemModel>(totalItems, relations);
            return Task.FromResult(pagedModel);
        }

        /// <summary>
        ///     Gets the content types referenced by the paged usages.
        /// </summary>
        /// <param name="pagedUsages">The paged usages containing property aliases and UDIs.</param>
        /// <returns>A list of content type compositions.</returns>
        private List<IContentTypeComposition> GetReferencedContentTypes(List<(string PropertyAlias, Udi Udi)> pagedUsages)
        {
            IEnumerable<IContentTypeComposition> documentTypes = GetContentTypes(
                pagedUsages,
                Constants.UdiEntityType.DocumentType,
                _contentTypeRepository);
            IEnumerable<IContentTypeComposition> mediaTypes = GetContentTypes(
                pagedUsages,
                Constants.UdiEntityType.MediaType,
                _mediaTypeRepository);
            IEnumerable<IContentTypeComposition> memberTypes = GetContentTypes(
                pagedUsages,
                Constants.UdiEntityType.MemberType,
                _memberTypeRepository);
            return documentTypes.Concat(mediaTypes).Concat(memberTypes).ToList();
        }

        /// <summary>
        ///     Gets content types for the specified entity type from the usages collection.
        /// </summary>
        /// <typeparam name="T">The content type composition type.</typeparam>
        /// <param name="dataTypeUsages">The data type usages.</param>
        /// <param name="entityType">The entity type to filter by.</param>
        /// <param name="repository">The repository to query.</param>
        /// <returns>A collection of content types.</returns>
        private static IEnumerable<T> GetContentTypes<T>(
            IEnumerable<(string PropertyAlias, Udi Udi)> dataTypeUsages,
            string entityType,
            IContentTypeRepositoryBase<T> repository)
            where T : IContentTypeComposition
        {
            Guid[] contentTypeKeys = dataTypeUsages
                .Where(x => x.Udi is GuidUdi && x.Udi.EntityType == entityType)
                .Select(x => ((GuidUdi)x.Udi).Guid)
                .Distinct()
                .ToArray();
            return contentTypeKeys.Length > 0
                ? repository.GetMany(contentTypeKeys)
                : [];
        }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> ValidateConfigurationData(IDataType dataType)
        {
            IConfigurationEditor? configurationEditor = dataType.Editor?.GetConfigurationEditor();
            return configurationEditor == null
                ?
                [
                    new ValidationResult($"Data type with editor alias {dataType.EditorAlias} does not have a configuration editor")
                ]
                : configurationEditor.Validate(dataType.ConfigurationData);
        }

        /// <summary>
        ///     Saves a data type with validation and notifications.
        /// </summary>
        /// <param name="dataType">The data type to save.</param>
        /// <param name="operationValidation">A function that validates the operation and returns a status.</param>
        /// <param name="userKey">The key of the user performing the action.</param>
        /// <param name="auditType">The type of audit entry to create.</param>
        /// <returns>An attempt result with the saved data type and operation status.</returns>
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

            await AuditAsync(auditType, userKey, dataType.Id);
            scope.Complete();

            return Attempt.SucceedWithStatus(DataTypeOperationStatus.Success, dataType);
        }

        /// <summary>
        ///     Asynchronously creates an audit entry for a data type operation.
        /// </summary>
        /// <param name="type">The audit type.</param>
        /// <param name="userId">The ID of the user performing the action.</param>
        /// <param name="objectId">The ID of the object being audited.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task AuditAsync(AuditType type, int userId, int objectId)
        {
            Guid userKey = await _userIdKeyResolver.GetAsync(userId);
            await AuditAsync(type, userKey, objectId);
        }

        /// <summary>
        ///     Asynchronously creates an audit entry for a data type operation.
        /// </summary>
        /// <param name="type">The audit type.</param>
        /// <param name="userKey">The key of the user performing the action.</param>
        /// <param name="objectId">The ID of the object being audited.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task AuditAsync(AuditType type, Guid userKey, int objectId) =>
            await _auditService.AddAsync(
                type,
                userKey,
                objectId,
                UmbracoObjectTypes.DataType.GetName());
    }
}
