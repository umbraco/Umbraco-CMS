using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services
{

    /// <summary>
    /// Defines the DataType Service, which is an easy access to operations involving <see cref="IDataType"/>
    /// </summary>
    public interface IDataTypeService : IService
    {
        /// <summary>
        /// Returns a dictionary of content type <see cref="Udi"/>s and the property type aliases that use a <see cref="IDataType"/>
        /// </summary>
        IReadOnlyDictionary<Udi, IEnumerable<string>> GetReferences(int id);

        /// <summary>
        /// Attempts to create container
        /// </summary>
        Attempt<OperationResult<OperationResultType, EntityContainer>> CreateContainer(int parentId, Guid key, string name, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Attempts to save a container
        /// </summary>
        Attempt<OperationResult> SaveContainer(EntityContainer container, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Gets a <see cref="EntityContainer"/> by its Id
        /// </summary>
        /// <param name="containerId">Id of the <see cref="EntityContainer"/></param>
        /// <returns><see cref="EntityContainer"/></returns>
        EntityContainer GetContainer(int containerId);

        /// <summary>
        /// Gets a <see cref="EntityContainer"/> by its Id
        /// </summary>
        /// <param name="containerId">Id of the <see cref="EntityContainer"/></param>
        /// <returns><see cref="EntityContainer"/></returns>
        EntityContainer GetContainer(Guid containerId);

        /// <summary>
        /// Gets multiple <see cref="EntityContainer"/> by their ids
        /// </summary>
        /// <param name="folderName">folderName the <see cref="EntityContainer"/> lives in</param>
        /// <param name="level">level of the folder</param>
        /// /// <returns>An enumerable list of <see cref="EntityContainer"/> objects</returns>
        IEnumerable<EntityContainer> GetContainers(string folderName, int level);

        /// <summary>
        /// Gets multiple <see cref="EntityContainer"/> by their dataTypes
        /// </summary>
        /// <param name="dataType"> The datatype in the<see cref="EntityContainer"/></param>
        /// /// <returns>An enumerable list of <see cref="EntityContainer"/> objects</returns>
        IEnumerable<EntityContainer> GetContainers(IDataType dataType);

        /// <summary>
        /// Gets multiple <see cref="EntityContainer"/> by their ids
        /// </summary>
        /// <param name="containerIds">Id of the <see cref="EntityContainer"/></param>
        /// <returns>An enumerable list of <see cref="EntityContainer"/> objects</returns>
        IEnumerable<EntityContainer> GetContainers(int[] containerIds);

        /// <summary>
        /// Deletes a <see cref="EntityContainer"/> by id
        /// </summary>
        /// <param name="containerId">Id of the <see cref="EntityContainer"/></param>
        /// <param name="userId">Id of the user deleting the container</param>
        /// <returns><see cref="OperationResult"/></returns>
        Attempt<OperationResult> DeleteContainer(int containerId, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Attempts to rename a <see cref="EntityContainer"/> by id
        /// </summary>
        /// <param name="id">Id of the <see cref="EntityContainer"/></param>
        /// <param name="name">New name of the <see cref="EntityContainer"/></param>
        /// <returns><see cref="Attempt"/></returns>
        Attempt<OperationResult<OperationResultType, EntityContainer>> RenameContainer(int id, string name, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its Name
        /// </summary>
        /// <param name="name">Name of the <see cref="IDataType"/></param>
        /// <returns><see cref="IDataType"/></returns>
        IDataType GetDataType(string name);

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataType"/></param>
        /// <returns><see cref="IDataType"/></returns>
        IDataType GetDataType(int id);

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its unique guid Id
        /// </summary>
        /// <param name="id">Unique guid Id of the DataType</param>
        /// <returns><see cref="IDataType"/></returns>
        IDataType GetDataType(Guid id);

        /// <summary>
        /// Gets all <see cref="IDataType"/> objects or those with the ids passed in
        /// </summary>
        /// <param name="ids">Optional array of Ids</param>
        /// <returns>An enumerable list of <see cref="IDataType"/> objects</returns>
        IEnumerable<IDataType> GetAll(params int[] ids);

        /// <summary>
        /// Saves an <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataType"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issuing the save</param>
        void Save(IDataType dataType, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Saves a collection of <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issuing the save</param>
        void Save(IEnumerable<IDataType> dataTypeDefinitions, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Deletes an <see cref="IDataType"/>
        /// </summary>
        /// <remarks>
        /// Please note that deleting a <see cref="IDataType"/> will remove
        /// all the <see cref="IPropertyType"/> data that references this <see cref="IDataType"/>.
        /// </remarks>
        /// <param name="dataType"><see cref="IDataType"/> to delete</param>
        /// <param name="userId">Id of the user issuing the deletion</param>
        void Delete(IDataType dataType, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its control Id
        /// </summary>
        /// <param name="propertyEditorAlias">Alias of the property editor</param>
        /// <returns>Collection of <see cref="IDataType"/> objects with a matching control id</returns>
        IEnumerable<IDataType> GetByEditorAlias(string propertyEditorAlias);

        /// <summary>
        /// Attempts to move a<see cref="IDataType"/>
        /// </summary>
        /// <param name="toMove"><see cref="IDataType"/> to move</param>
        /// <param name="parentId">id of the <see cref="IDataType"/> parent</param>
        /// <returns><see cref="Attempt"/></returns>
        Attempt<OperationResult<MoveOperationStatusType>> Move(IDataType toMove, int parentId);
    }
}
