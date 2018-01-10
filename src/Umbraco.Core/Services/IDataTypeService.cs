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
        IDataType GetDataTypeDefinitionByName(string name);

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataType"/></param>
        /// <returns><see cref="IDataType"/></returns>
        IDataType GetDataTypeDefinitionById(int id);

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its unique guid Id
        /// </summary>
        /// <param name="id">Unique guid Id of the DataType</param>
        /// <returns><see cref="IDataType"/></returns>
        IDataType GetDataTypeDefinitionById(Guid id);

        /// <summary>
        /// Gets all <see cref="IDataType"/> objects or those with the ids passed in
        /// </summary>
        /// <param name="ids">Optional array of Ids</param>
        /// <returns>An enumerable list of <see cref="IDataType"/> objects</returns>
        IEnumerable<IDataType> GetAllDataTypeDefinitions(params int[] ids);

        /// <summary>
        /// Saves an <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataType"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issueing the save</param>
        void Save(IDataType dataType, int userId = 0);

        /// <summary>
        /// Saves a collection of <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issueing the save</param>
        void Save(IEnumerable<IDataType> dataTypeDefinitions, int userId = 0);

        /// <summary>
        /// Saves a collection of <see cref="IDataType"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataType"/> to save</param>
        /// <param name="userId">Id of the user issueing the save</param>
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
        /// <param name="userId">Id of the user issueing the deletion</param>
        void Delete(IDataType dataType, int userId = 0);

        /// <summary>
        /// Gets a <see cref="IDataType"/> by its control Id
        /// </summary>
        /// <param name="propertyEditorAlias">Alias of the property editor</param>
        /// <returns>Collection of <see cref="IDataType"/> objects with a matching contorl id</returns>
        IEnumerable<IDataType> GetDataTypeDefinitionByPropertyEditorAlias(string propertyEditorAlias);

        /// <summary>
        /// Gets all values for an <see cref="IDataType"/>
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataType"/> to retrieve prevalues from</param>
        /// <returns>An enumerable list of string values</returns>
        IEnumerable<string> GetPreValuesByDataTypeId(int id);

        /// <summary>
        /// Gets a pre-value collection by data type id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        PreValueCollection GetPreValuesCollectionByDataTypeId(int id);

        /// <summary>
        /// Saves a list of PreValues for a given DataTypeDefinition
        /// </summary>
        /// <param name="dataTypeId">Id of the DataTypeDefinition to save PreValues for</param>
        /// <param name="values">List of string values to save</param>
        [Obsolete("This should no longer be used, use the alternative SavePreValues or SaveDataTypeAndPreValues methods instead. This will only insert pre-values without keys")]
        void SavePreValues(int dataTypeId, IEnumerable<string> values);

        /// <summary>
        /// Saves a list of PreValues for a given DataTypeDefinition
        /// </summary>
        /// <param name="dataTypeId">Id of the DataTypeDefinition to save PreValues for</param>
        /// <param name="values">List of key/value pairs to save</param>
        void SavePreValues(int dataTypeId, IDictionary<string, PreValue> values);

        /// <summary>
        /// Saves a list of PreValues for a given DataTypeDefinition
        /// </summary>
        /// <param name="dataType">The DataTypeDefinition to save PreValues for</param>
        /// <param name="values">List of key/value pairs to save</param>
        void SavePreValues(IDataType dataType, IDictionary<string, PreValue> values);

        /// <summary>
        /// Saves the data type and it's prevalues
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="values"></param>
        /// <param name="userId"></param>
        void SaveDataTypeAndPreValues(IDataType dataType, IDictionary<string, PreValue> values, int userId = 0);

        /// <summary>
        /// Gets a specific PreValue by its Id
        /// </summary>
        /// <param name="id">Id of the PreValue to retrieve the value from</param>
        /// <returns>PreValue as a string</returns>
        string GetPreValueAsString(int id);

        Attempt<OperationResult<MoveOperationStatusType>> Move(IDataType toMove, int parentId);

    }
}
