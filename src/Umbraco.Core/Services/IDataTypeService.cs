using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the DataType Service, which is an easy access to operations involving <see cref="IDataType" />
/// </summary>
public interface IDataTypeService : IService
{
    /// <summary>
    ///     Gets a paged result of items which are in relation with the current data type.
    /// </summary>
    /// <param name="key">The identifier of the data type to retrieve relations for.</param>
    /// <param name="skip">The amount of items to skip</param>
    /// <param name="take">The amount of items to take.</param>
    /// <returns>A paged result of <see cref="RelationItemModel" /> objects.</returns>
    /// <remarks>
    /// Note that the model and method signature here aligns with with how we handle retrieval of concrete Umbraco
    /// relations based on documents, media and members in <see cref="ITrackedReferencesService"/>.
    /// The intention is that we align data type relations with these so they can be handled polymorphically at the management API
    /// and backoffice UI level.
    /// </remarks>
    Task<PagedModel<RelationItemModel>> GetPagedRelationsAsync(Guid key, int skip, int take)
        => Task.FromResult(new PagedModel<RelationItemModel>());

    /// <summary>
    ///     Creates a container for organizing data types.
    /// </summary>
    /// <param name="parentId">The parent container ID, or -1 for root.</param>
    /// <param name="key">The unique key for the new container.</param>
    /// <param name="name">The name of the container.</param>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <returns>An operation result containing the created container.</returns>
    [Obsolete("Please use IDataTypeContainerService for all data type container operations. Will be removed in V15.")]
    Attempt<OperationResult<OperationResultType, EntityContainer>?> CreateContainer(
        int parentId,
        Guid key,
        string name,
        int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Saves a container.
    /// </summary>
    /// <param name="container">The container to save.</param>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <returns>An operation result indicating success or failure.</returns>
    [Obsolete("Please use IDataTypeContainerService for all data type container operations. Will be removed in V15.")]
    Attempt<OperationResult?> SaveContainer(EntityContainer container, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets a container by its ID.
    /// </summary>
    /// <param name="containerId">The container ID.</param>
    /// <returns>The container, or null if not found.</returns>
    [Obsolete("Please use IDataTypeContainerService for all data type container operations. Will be removed in V15.")]
    EntityContainer? GetContainer(int containerId);

    /// <summary>
    ///     Gets a container by its unique key.
    /// </summary>
    /// <param name="containerId">The container unique key.</param>
    /// <returns>The container, or null if not found.</returns>
    [Obsolete("Please use IDataTypeContainerService for all data type container operations. Will be removed in V15.")]
    EntityContainer? GetContainer(Guid containerId);

    /// <summary>
    ///     Gets containers by name and level.
    /// </summary>
    /// <param name="folderName">The container name.</param>
    /// <param name="level">The container level.</param>
    /// <returns>A collection of matching containers.</returns>
    [Obsolete("Please use IDataTypeContainerService for all data type container operations. Will be removed in V15.")]
    IEnumerable<EntityContainer> GetContainers(string folderName, int level);

    /// <summary>
    ///     Gets all ancestor containers for a data type.
    /// </summary>
    /// <param name="dataType">The data type.</param>
    /// <returns>A collection of ancestor containers.</returns>
    [Obsolete("Please use IDataTypeContainerService for all data type container operations. Will be removed in V15.")]
    IEnumerable<EntityContainer> GetContainers(IDataType dataType);

    /// <summary>
    ///     Gets containers by their IDs.
    /// </summary>
    /// <param name="containerIds">The container IDs.</param>
    /// <returns>A collection of containers.</returns>
    [Obsolete("Please use IDataTypeContainerService for all data type container operations. Will be removed in V15.")]
    IEnumerable<EntityContainer> GetContainers(int[] containerIds);

    /// <summary>
    ///     Deletes a container.
    /// </summary>
    /// <param name="containerId">The ID of the container to delete.</param>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <returns>An operation result indicating success or failure.</returns>
    [Obsolete("Please use IDataTypeContainerService for all data type container operations. Will be removed in V15.")]
    Attempt<OperationResult?> DeleteContainer(int containerId, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Renames a container.
    /// </summary>
    /// <param name="id">The ID of the container to rename.</param>
    /// <param name="name">The new name for the container.</param>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <returns>An operation result containing the renamed container.</returns>
    [Obsolete("Please use IDataTypeContainerService for all data type container operations. Will be removed in V15.")]
    Attempt<OperationResult<OperationResultType, EntityContainer>?> RenameContainer(
        int id,
        string name,
        int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets a <see cref="IDataType" /> by its Name
    /// </summary>
    /// <param name="name">Name of the <see cref="IDataType" /></param>
    /// <returns>
    ///     <see cref="IDataType" />
    /// </returns>
    [Obsolete("Please use GetAsync. Will be removed in V15.")]
    IDataType? GetDataType(string name);

    /// <summary>
    ///     Gets a <see cref="IDataType" /> by its Id
    /// </summary>
    /// <param name="id">Id of the <see cref="IDataType" /></param>
    /// <returns>
    ///     <see cref="IDataType" />
    /// </returns>
    [Obsolete("Please use GetAsync. Will be removed in V15.")]
    IDataType? GetDataType(int id);

    /// <summary>
    ///     Gets an <see cref="IDataType" /> by its Name
    /// </summary>
    /// <param name="name">Name of the <see cref="IDataType" /></param>
    /// <returns>
    ///     <see cref="IDataType" />
    /// </returns>
    Task<IDataType?> GetAsync(string name);

    /// <summary>
    ///     Gets an <see cref="IDataType" /> by its unique guid Id
    /// </summary>
    /// <param name="id">Unique guid Id of the DataType</param>
    /// <returns>
    ///     <see cref="IDataType" />
    /// </returns>
    Task<IDataType?> GetAsync(Guid id);

    /// <summary>
    /// Gets multiple <see cref="IDataType"/> objects by their unique keys.
    /// </summary>
    /// <param name="keys">The keys to get datatypes by.</param>
    /// <returns>An attempt with the requested data types.</returns>
    Task<IEnumerable<IDataType>> GetAllAsync(params Guid[] keys);

    /// <summary>
    /// Gets multiple <see cref="IDataType"/> objects by their unique keys.
    /// </summary>
    /// <param name="name">Name to filter by.</param>
    /// <param name="editorUiAlias">Editor ui alias to filter by.</param>
    /// <param name="editorAlias">Editor alias to filter by.</param>
    /// <param name="skip">Number of items to skip.</param>
    /// <param name="take">Number of items to take.</param>
    /// <returns>An attempt with the requested data types.</returns>
    Task<PagedModel<IDataType>> FilterAsync(string? name = null, string? editorUiAlias = null, string? editorAlias = null, int skip = 0, int take = 100);

    /// <summary>
    ///     Gets all <see cref="IDataType" /> objects or those with the ids passed in
    /// </summary>
    /// <param name="ids">Optional array of Ids</param>
    /// <returns>An enumerable list of <see cref="IDataType" /> objects</returns>
    [Obsolete("Please use GetAllAsync. Will be removed in V15.")]
    IEnumerable<IDataType> GetAll(params int[] ids);

    /// <summary>
    ///     Saves an <see cref="IDataType" />
    /// </summary>
    /// <param name="dataType"><see cref="IDataType" /> to save</param>
    /// <param name="userId">Id of the user issuing the save</param>
    [Obsolete("Please use CreateAsync or UpdateAsync. Will be removed in V15.")]
    void Save(IDataType dataType, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Saves a collection of <see cref="IDataType" />
    /// </summary>
    /// <param name="dataTypeDefinitions"><see cref="IDataType" /> to save</param>
    /// <param name="userId">Id of the user issuing the save</param>
    [Obsolete("Please use CreateAsync or UpdateAsync. Will be removed in V15.")]
    void Save(IEnumerable<IDataType> dataTypeDefinitions, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Creates a new <see cref="IDataType" />
    /// </summary>
    /// <param name="dataType"><see cref="IDataType" /> to create</param>
    /// <param name="userKey">Key of the user issuing the creation</param>
    Task<Attempt<IDataType, DataTypeOperationStatus>> CreateAsync(IDataType dataType, Guid userKey);

    /// <summary>
    ///     Updates an existing <see cref="IDataType" />
    /// </summary>
    /// <param name="dataType"><see cref="IDataType" /> to update</param>
    /// <param name="userKey">Key of the user issuing the update</param>
    Task<Attempt<IDataType, DataTypeOperationStatus>> UpdateAsync(IDataType dataType, Guid userKey);

    /// <summary>
    ///     Deletes an <see cref="IDataType" />
    /// </summary>
    /// <remarks>
    ///     Please note that deleting a <see cref="IDataType" /> will remove
    ///     all the <see cref="IPropertyType" /> data that references this <see cref="IDataType" />.
    /// </remarks>
    /// <param name="dataType"><see cref="IDataType" /> to delete</param>
    /// <param name="userId">Id of the user issuing the deletion</param>
    [Obsolete("Please use DeleteAsync. Will be removed in V15.")]
    void Delete(IDataType dataType, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes an <see cref="IDataType" />
    /// </summary>
    /// <remarks>
    ///     Please note that deleting a <see cref="IDataType" /> will remove
    ///     all the <see cref="IPropertyType" /> data that references this <see cref="IDataType" />.
    /// </remarks>
    /// <param name="id">The guid Id of the <see cref="IDataType" /> to delete</param>
    /// <param name="userKey">Key of the user issuing the deletion</param>
    Task<Attempt<IDataType?, DataTypeOperationStatus>> DeleteAsync(Guid id, Guid userKey);

    /// <summary>
    ///     Gets a <see cref="IDataType" /> by its control Id
    /// </summary>
    /// <param name="propertyEditorAlias">Alias of the property editor</param>
    /// <returns>Collection of <see cref="IDataType" /> objects with a matching control id</returns>
    [Obsolete("Please use GetByEditorAliasAsync. Will be removed in V15.")]
    IEnumerable<IDataType> GetByEditorAlias(string propertyEditorAlias);

    /// <summary>
    ///     Gets all <see cref="IDataType" /> for a given property editor.
    /// </summary>
    /// <param name="propertyEditorAlias">Alias of the property editor.</param>
    /// <returns>Collection of <see cref="IDataType" /> configured for the property editor.</returns>
    Task<IEnumerable<IDataType>> GetByEditorAliasAsync(string propertyEditorAlias) => Task.FromResult(GetByEditorAlias(propertyEditorAlias));

    /// <summary>
    ///     Gets all <see cref="IDataType" /> for a given editor UI alias.
    /// </summary>
    /// <param name="editorUiAlias">The UI Alias to query by.</param>
    /// <returns>Collection of <see cref="IDataType" /> which has the UI alias.</returns>
    Task<IEnumerable<IDataType>> GetByEditorUiAlias(string editorUiAlias);

    /// <summary>
    ///     Moves a <see cref="IDataType" /> to a given container.
    /// </summary>
    /// <param name="toMove">The data type that will be moved.</param>
    /// <param name="parentId">The ID of the parent container to move to.</param>
    /// <returns>An operation result indicating the move status.</returns>
    [Obsolete("Please use MoveAsync instead. Will be removed in V15")]
    Attempt<OperationResult<MoveOperationStatusType>?> Move(IDataType toMove, int parentId);

    /// <summary>
    /// Moves a <see cref="IDataType"/> to a given container
    /// </summary>
    /// <param name="toMove">The data type that will be moved</param>
    /// <param name="containerKey">The container key where the data type will be moved to.</param>
    /// <param name="userKey">The user that did the Move action</param>
    /// <returns>An attempt result with the moved data type and operation status.</returns>
    Task<Attempt<IDataType, DataTypeOperationStatus>> MoveAsync(IDataType toMove, Guid? containerKey, Guid userKey);

    /// <summary>
    ///     Copies a <see cref="IDataType" /> to a given container.
    /// </summary>
    /// <param name="copying">The data type to copy.</param>
    /// <param name="containerId">The ID of the target container.</param>
    /// <returns>An operation result containing the copied data type.</returns>
    [Obsolete("Please use CopyASync instead. Will be removed in V15")]
    Attempt<OperationResult<MoveOperationStatusType, IDataType>?> Copy(IDataType copying, int containerId) =>
        Copy(copying, containerId, Constants.Security.SuperUserId);

    /// <summary>
    ///     Copies a <see cref="IDataType" /> to a given container.
    /// </summary>
    /// <param name="copying">The data type to copy.</param>
    /// <param name="containerId">The ID of the target container.</param>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <returns>An operation result containing the copied data type.</returns>
    [Obsolete("Please use CopyASync instead. Will be removed in V15")]
    Attempt<OperationResult<MoveOperationStatusType, IDataType>?> Copy(
        IDataType copying,
        int containerId,
        int userId = Constants.Security.SuperUserId) => throw new NotImplementedException();

    /// <summary>
    /// Copies a <see cref="IDataType"/> to a given container
    /// </summary>
    /// <param name="toCopy">The data type that will be copied</param>
    /// <param name="containerKey">The container key where the data type will be copied to.</param>
    /// <param name="userKey">The user that did the Copy action</param>
    /// <returns>An attempt result with the copied data type and operation status.</returns>
    Task<Attempt<IDataType, DataTypeOperationStatus>> CopyAsync(IDataType toCopy, Guid? containerKey, Guid userKey);

    /// <summary>
    /// Performs validation for the configuration data of a given data type.
    /// </summary>
    /// <param name="dataType">The data type whose configuration to validate.</param>
    /// <returns>One or more <see cref="ValidationResult"/> if the configuration data is invalid, an empty collection otherwise.</returns>
    IEnumerable<ValidationResult> ValidateConfigurationData(IDataType dataType);

    /// <summary>
    ///     Gets all <see cref="IDataType" /> for a set of property editors.
    /// </summary>
    /// <param name="propertyEditorAlias">Aliases of the property editors.</param>
    /// <returns>Collection of <see cref="IDataType" /> configured for the property editors.</returns>
    Task<IEnumerable<IDataType>> GetByEditorAliasAsync(string[] propertyEditorAlias) => Task.FromResult(propertyEditorAlias.SelectMany(x=>GetByEditorAlias(x)));
}
