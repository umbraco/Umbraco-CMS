using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the DataType Service, which is an easy access to operations involving <see cref="IDataType"/>
    /// </summary>
    public interface IDataTypeService : IService
    {
        Attempt<OperationResult<OperationResultType, EntityContainer>> CreateContainer(int parentId, string name, int userId = 0);
        Attempt<OperationResult> SaveContainer(EntityContainer container, int userId = 0);
        EntityContainer GetContainer(int containerId);
        EntityContainer GetContainer(Guid containerId);
        IEnumerable<EntityContainer> GetContainers(string folderName, int level);
        IEnumerable<EntityContainer> GetContainers(IDataType dataType);
        IEnumerable<EntityContainer> GetContainers(int[] containerIds);
        Attempt<OperationResult> DeleteContainer(int containerId, int userId = 0);
        Attempt<OperationResult<OperationResultType, EntityContainer>> RenameContainer(int id, string name, int userId = 0);

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
        void Save(IDataType dataType, int userId = 0);

        /// <summary>
        /// Saves a collection of <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issuing the save</param>
        void Save(IEnumerable<IDataType> dataTypeDefinitions, int userId = 0);

        /// <summary>
        /// Saves a collection of <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issuing the save</param>
        /// <param name="raiseEvents">Boolean indicating whether or not to raise events</param>
        void Save(IEnumerable<IDataType> dataTypeDefinitions, int userId, bool raiseEvents);

        /// <summary>
        /// Deletes an <see cref="IDataType"/>
        /// </summary>
        /// <remarks>
        /// Please note that deleting a <see cref="IDataType"/> will remove
        /// all the <see cref="PropertyType"/> data that references this <see cref="IDataType"/>.
        /// </remarks>
        /// <param name="dataType"><see cref="IDataType"/> to delete</param>
        /// <param name="userId">Id of the user issuing the deletion</param>
        void Delete(IDataType dataType, int userId = 0);

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its control Id
        /// </summary>
        /// <param name="propertyEditorAlias">Alias of the property editor</param>
        /// <returns>Collection of <see cref="IDataType"/> objects with a matching control id</returns>
        IEnumerable<IDataType> GetByEditorAlias(string propertyEditorAlias);

        Attempt<OperationResult<MoveOperationStatusType>> Move(IDataType toMove, int parentId);
    }
}
