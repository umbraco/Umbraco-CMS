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
    ///     Returns a dictionary of content type <see cref="Udi" />s and the property type aliases that use a
    ///     <see cref="IDataType" />
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Obsolete("Please use GetReferencesAsync. Will be deleted in V15.")]
    IReadOnlyDictionary<Udi, IEnumerable<string>> GetReferences(int id);

    /// <summary>
    ///     Returns a dictionary of content type <see cref="Udi" />s and the property type aliases that use a <see cref="IDataType" />
    /// </summary>
    /// <param name="id">The guid Id of the <see cref="IDataType" /></param>
    /// <returns></returns>
    Task<Attempt<IReadOnlyDictionary<Udi, IEnumerable<string>>, DataTypeOperationStatus>> GetReferencesAsync(Guid id);

    Attempt<OperationResult<OperationResultType, EntityContainer>?> CreateContainer(int parentId, Guid key, string name, int userId = Constants.Security.SuperUserId);

    Attempt<OperationResult?> SaveContainer(EntityContainer container, int userId = Constants.Security.SuperUserId);

    EntityContainer? GetContainer(int containerId);

    EntityContainer? GetContainer(Guid containerId);

    IEnumerable<EntityContainer> GetContainers(string folderName, int level);

    IEnumerable<EntityContainer> GetContainers(IDataType dataType);

    IEnumerable<EntityContainer> GetContainers(int[] containerIds);

    Attempt<OperationResult?> DeleteContainer(int containerId, int userId = Constants.Security.SuperUserId);

    Attempt<OperationResult<OperationResultType, EntityContainer>?> RenameContainer(int id, string name, int userId = Constants.Security.SuperUserId);

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
    ///     Gets a <see cref="IDataType" /> by its unique guid Id
    /// </summary>
    /// <param name="id">Unique guid Id of the DataType</param>
    /// <returns>
    ///     <see cref="IDataType" />
    /// </returns>
    [Obsolete("Please use GetAsync. Will be removed in V15.")]
    IDataType? GetDataType(Guid id);

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
    ///     Gets all <see cref="IDataType" /> objects or those with the ids passed in
    /// </summary>
    /// <param name="ids">Optional array of Ids</param>
    /// <returns>An enumerable list of <see cref="IDataType" /> objects</returns>
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
    /// <param name="userId">Id of the user issuing the creation</param>
    Task<Attempt<IDataType, DataTypeOperationStatus>> CreateAsync(IDataType dataType, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Updates an existing <see cref="IDataType" />
    /// </summary>
    /// <param name="dataType"><see cref="IDataType" /> to update</param>
    /// <param name="userId">Id of the user issuing the update</param>
    Task<Attempt<IDataType, DataTypeOperationStatus>> UpdateAsync(IDataType dataType, int userId = Constants.Security.SuperUserId);

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
    /// <param name="userId">Id of the user issuing the deletion</param>
    Task<Attempt<IDataType?, DataTypeOperationStatus>> DeleteAsync(Guid id, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets a <see cref="IDataType" /> by its control Id
    /// </summary>
    /// <param name="propertyEditorAlias">Alias of the property editor</param>
    /// <returns>Collection of <see cref="IDataType" /> objects with a matching control id</returns>
    [Obsolete("Please use GetByEditorAliasAsync. Will be removed in V15.")]
    IEnumerable<IDataType> GetByEditorAlias(string propertyEditorAlias);

    /// <summary>
    ///     Gets all <see cref="IDataType" /> for a given property editor
    /// </summary>
    /// <param name="propertyEditorAlias">Alias of the property editor</param>
    /// <returns>Collection of <see cref="IDataType" /> configured for the property editor</returns>
    Task<IEnumerable<IDataType>> GetByEditorAliasAsync(string propertyEditorAlias);

    Attempt<OperationResult<MoveOperationStatusType>?> Move(IDataType toMove, int parentId);

    [Obsolete("Use the method which specifies the userId parameter")]
    Attempt<OperationResult<MoveOperationStatusType, IDataType>?> Copy(IDataType copying, int containerId) => Copy(copying, containerId, Constants.Security.SuperUserId);

    /// <summary>
    /// Copies the give <see cref="IDataType"/> to a given container
    /// We have the default implementation here to avoid breaking changes for the user
    /// </summary>
    /// <param name="copying">The data type that will be copied</param>
    /// <param name="containerId">The container ID under where the data type will be copied</param>
    /// <param name="userId">The user that did the Copy action</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    Attempt<OperationResult<MoveOperationStatusType, IDataType>?> Copy(IDataType copying, int containerId, int userId = Constants.Security.SuperUserId) => throw new NotImplementedException();

    /// <summary>
    /// Performs validation for the configuration data of a given data type.
    /// </summary>
    /// <param name="dataType">The data type whose configuration to validate.</param>
    /// <returns>One or more <see cref="ValidationResult"/> if the configuration data is invalid, an empty collection otherwise.</returns>
    IEnumerable<ValidationResult> ValidateConfigurationData(IDataType dataType);
}
