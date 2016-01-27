using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using umbraco.interfaces;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the DataType Service, which is an easy access to operations involving <see cref="IDataTypeDefinition"/> 
    /// </summary>
    public interface IDataTypeService : IService
    {
        Attempt<OperationStatus<EntityContainer, OperationStatusType>> CreateContainer(int parentId, string name, int userId = 0);
        Attempt<OperationStatus> SaveContainer(EntityContainer container, int userId = 0);
        EntityContainer GetContainer(int containerId);
        EntityContainer GetContainer(Guid containerId);
        IEnumerable<EntityContainer> GetContainers(string folderName, int level);
        IEnumerable<EntityContainer> GetContainers(IDataTypeDefinition dataTypeDefinition);
        IEnumerable<EntityContainer> GetContainers(int[] containerIds);
        Attempt<OperationStatus> DeleteContainer(int containerId, int userId = 0);

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its Name
        /// </summary>
        /// <param name="name">Name of the <see cref="IDataTypeDefinition"/></param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        IDataTypeDefinition GetDataTypeDefinitionByName(string name);

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataTypeDefinition"/></param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        IDataTypeDefinition GetDataTypeDefinitionById(int id);

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its unique guid Id
        /// </summary>
        /// <param name="id">Unique guid Id of the DataType</param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        IDataTypeDefinition GetDataTypeDefinitionById(Guid id);

        /// <summary>
        /// Gets all <see cref="IDataTypeDefinition"/> objects or those with the ids passed in
        /// </summary>
        /// <param name="ids">Optional array of Ids</param>
        /// <returns>An enumerable list of <see cref="IDataTypeDefinition"/> objects</returns>
        IEnumerable<IDataTypeDefinition> GetAllDataTypeDefinitions(params int[] ids);

        /// <summary>
        /// Saves an <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="dataTypeDefinition"><see cref="IDataTypeDefinition"/> to save</param>
        /// <param name="userId">Id of the user issueing the save</param>
        void Save(IDataTypeDefinition dataTypeDefinition, int userId = 0);

        /// <summary>
        /// Saves a collection of <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataTypeDefinition"/> to save</param>
        /// <param name="userId">Id of the user issueing the save</param>
        void Save(IEnumerable<IDataTypeDefinition> dataTypeDefinitions, int userId = 0);

        /// <summary>
        /// Saves a collection of <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="dataTypeDefinitions"><see cref="IDataTypeDefinition"/> to save</param>
        /// <param name="userId">Id of the user issueing the save</param>
        /// <param name="raiseEvents">Boolean indicating whether or not to raise events</param>
        void Save(IEnumerable<IDataTypeDefinition> dataTypeDefinitions, int userId, bool raiseEvents);

        /// <summary>
        /// Deletes an <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <remarks>
        /// Please note that deleting a <see cref="IDataTypeDefinition"/> will remove
        /// all the <see cref="PropertyType"/> data that references this <see cref="IDataTypeDefinition"/>.
        /// </remarks>
        /// <param name="dataTypeDefinition"><see cref="IDataTypeDefinition"/> to delete</param>
        /// <param name="userId">Id of the user issueing the deletion</param>
        void Delete(IDataTypeDefinition dataTypeDefinition, int userId = 0);

        /// <summary>
        /// Gets the <see cref="IDataType"/> specified by it's unique ID
        /// </summary>
        /// <param name="id">Id of the DataType, which corresponds to the Guid Id of the control</param>
        /// <returns><see cref="IDataType"/> object</returns>
        [Obsolete("IDataType is obsolete and is no longer used, it will be removed from the codebase in future versions")]
        IDataType GetDataTypeById(Guid id);
        
        /// <summary>
        /// Gets a complete list of all registered <see cref="IDataType"/>'s
        /// </summary>
        /// <returns>An enumerable list of <see cref="IDataType"/> objects</returns>
        [Obsolete("IDataType is obsolete and is no longer used, it will be removed from the codebase in future versions")]
        IEnumerable<IDataType> GetAllDataTypes();

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its control Id
        /// </summary>
        /// <param name="id">Id of the DataType control</param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        [Obsolete("Property editor's are defined by a string alias from version 7 onwards, use the overload GetDataTypeDefinitionByPropertyEditorAlias instead")]
        IEnumerable<IDataTypeDefinition> GetDataTypeDefinitionByControlId(Guid id);

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its control Id
        /// </summary>
        /// <param name="propertyEditorAlias">Alias of the property editor</param>
        /// <returns>Collection of <see cref="IDataTypeDefinition"/> objects with a matching contorl id</returns>
        IEnumerable<IDataTypeDefinition> GetDataTypeDefinitionByPropertyEditorAlias(string propertyEditorAlias);

        /// <summary>
        /// Gets all values for an <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataTypeDefinition"/> to retrieve prevalues from</param>
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
        /// <param name="dataTypeDefinition">The DataTypeDefinition to save PreValues for</param>
        /// <param name="values">List of key/value pairs to save</param>
        void SavePreValues(IDataTypeDefinition dataTypeDefinition, IDictionary<string, PreValue> values);

        /// <summary>
        /// Saves the data type and it's prevalues
        /// </summary>
        /// <param name="dataTypeDefinition"></param>
        /// <param name="values"></param>
        /// <param name="userId"></param>
        void SaveDataTypeAndPreValues(IDataTypeDefinition dataTypeDefinition, IDictionary<string, PreValue> values, int userId = 0);

        /// <summary>
        /// Gets a specific PreValue by its Id
        /// </summary>
        /// <param name="id">Id of the PreValue to retrieve the value from</param>
        /// <returns>PreValue as a string</returns>
        string GetPreValueAsString(int id);

        Attempt<OperationStatus<MoveOperationStatusType>> Move(IDataTypeDefinition toMove, int parentId);
        
    }
}