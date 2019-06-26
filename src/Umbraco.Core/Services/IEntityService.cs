using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Services
{
    public interface IEntityService
    {
        /// <summary>
        /// Returns true if the entity exists
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Exists(Guid key);

        /// <summary>
        /// Returns true if the entity exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Exists(int id);

        /// <summary>
        /// Returns the integer id for a given GUID
        /// </summary>
        /// <param name="key"></param>
        /// <param name="umbracoObjectType"></param>
        /// <returns></returns>
        Attempt<int> GetIdForKey(Guid key, UmbracoObjectTypes umbracoObjectType);

        /// <summary>
        /// Returns the integer id for a given Udi
        /// </summary>
        /// <param name="udi"></param>
        /// <returns></returns>
        Attempt<int> GetIdForUdi(Udi udi);

        /// <summary>
        /// Returns the GUID for a given integer id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="umbracoObjectType"></param>
        /// <returns></returns>
        Attempt<Guid> GetKeyForId(int id, UmbracoObjectTypes umbracoObjectType);

        /// <summary>
        /// Gets an UmbracoEntity by its Id, and optionally loads the complete object graph.
        /// </summary>
        /// <returns>
        /// By default this will load the base type <see cref="IUmbracoEntity"/> with a minimum set of properties.
        /// </returns>
        /// <param name="key">Unique Id of the object to retrieve</param>
        /// <returns>An <see cref="IUmbracoEntity"/></returns>
        IUmbracoEntity GetByKey(Guid key);

        /// <summary>
        /// Gets an UmbracoEntity by its Id, and optionally loads the complete object graph.
        /// </summary>
        /// <returns>
        /// By default this will load the base type <see cref="IUmbracoEntity"/> with a minimum set of properties.
        /// </returns>
        /// <param name="id">Id of the object to retrieve</param>
       /// <returns>An <see cref="IUmbracoEntity"/></returns>
        IUmbracoEntity Get(int id);

        /// <summary>
        /// Gets an UmbracoEntity by its Id and UmbracoObjectType, and optionally loads the complete object graph.
        /// </summary>
        /// <returns>
        /// By default this will load the base type <see cref="IUmbracoEntity"/> with a minimum set of properties.
        /// </returns>
        /// <param name="key">Unique Id of the object to retrieve</param>
        /// <param name="umbracoObjectType">UmbracoObjectType of the entity to retrieve</param>
        /// <returns>An <see cref="IUmbracoEntity"/></returns>
        IUmbracoEntity GetByKey(Guid key, UmbracoObjectTypes umbracoObjectType);

        /// <summary>
        /// Gets an UmbracoEntity by its Id and UmbracoObjectType, and optionally loads the complete object graph.
        /// </summary>
        /// <returns>
        /// By default this will load the base type <see cref="IUmbracoEntity"/> with a minimum set of properties.
        /// </returns>
        /// <param name="id">Id of the object to retrieve</param>
        /// <param name="umbracoObjectType">UmbracoObjectType of the entity to retrieve</param>
        /// <returns>An <see cref="IUmbracoEntity"/></returns>
        IUmbracoEntity Get(int id, UmbracoObjectTypes umbracoObjectType);


        /// <summary>
        /// Gets an UmbracoEntity by its Id and specified Type. Optionally loads the complete object graph.
        /// </summary>
        /// <returns>
        /// By default this will load the base type <see cref="IUmbracoEntity"/> with a minimum set of properties.
        /// </returns>
        /// <typeparam name="T">Type of the model to retrieve. Must be based on an <see cref="IUmbracoEntity"/></typeparam>
        /// <param name="id">Id of the object to retrieve</param>
        /// <returns>An <see cref="IUmbracoEntity"/></returns>
        IUmbracoEntity Get<T>(int id) where T : IUmbracoEntity;

        /// <summary>
        /// Gets the parent of entity by its id
        /// </summary>
        /// <param name="id">Id of the entity to retrieve the Parent for</param>
        /// <returns>An <see cref="IUmbracoEntity"/></returns>
        IUmbracoEntity GetParent(int id);

        /// <summary>
        /// Gets the parent of entity by its id and UmbracoObjectType
        /// </summary>
        /// <param name="id">Id of the entity to retrieve the Parent for</param>
        /// <param name="umbracoObjectType">UmbracoObjectType of the parent to retrieve</param>
        /// <returns>An <see cref="IUmbracoEntity"/></returns>
        IUmbracoEntity GetParent(int id, UmbracoObjectTypes umbracoObjectType);

        /// <summary>
        /// Gets a collection of children by the parents Id
        /// </summary>
        /// <param name="parentId">Id of the parent to retrieve children for</param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        IEnumerable<IUmbracoEntity> GetChildren(int parentId);

        /// <summary>
        /// Gets a collection of children by the parents Id and UmbracoObjectType
        /// </summary>
        /// <param name="parentId">Id of the parent to retrieve children for</param>
        /// <param name="umbracoObjectType">UmbracoObjectType of the children to retrieve</param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        IEnumerable<IUmbracoEntity> GetChildren(int parentId, UmbracoObjectTypes umbracoObjectType);

        /// <summary>
        /// Returns a paged collection of children
        /// </summary>
        /// <param name="parentId">The parent id to return children for</param>
        /// <param name="umbracoObjectType"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        IEnumerable<IUmbracoEntity> GetPagedChildren(int parentId, UmbracoObjectTypes umbracoObjectType, long pageIndex, int pageSize, out long totalRecords,
            string orderBy = "SortOrder", Direction orderDirection = Direction.Ascending, string filter = "");

        /// <summary>
        /// Returns a paged collection of descendants
        /// </summary>
        /// <param name="id"></param>
        /// <param name="umbracoObjectType"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        IEnumerable<IUmbracoEntity> GetPagedDescendants(int id, UmbracoObjectTypes umbracoObjectType, long pageIndex, int pageSize, out long totalRecords,
            string orderBy = "path", Direction orderDirection = Direction.Ascending, string filter = "");

        /// <summary>
        /// Returns a paged collection of descendants
        /// </summary>
        IEnumerable<IUmbracoEntity> GetPagedDescendants(IEnumerable<int> ids, UmbracoObjectTypes umbracoObjectType, long pageIndex, int pageSize, out long totalRecords,
            string orderBy = "path", Direction orderDirection = Direction.Ascending, string filter = "");

        /// <summary>
        /// Returns a paged collection of descendants from the root
        /// </summary>
        /// <param name="umbracoObjectType"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="filter"></param>
        /// <param name="includeTrashed">true/false to include trashed objects</param>
        /// <returns></returns>
        IEnumerable<IUmbracoEntity> GetPagedDescendantsFromRoot(UmbracoObjectTypes umbracoObjectType, long pageIndex, int pageSize, out long totalRecords,
            string orderBy = "path", Direction orderDirection = Direction.Ascending, string filter = "", bool includeTrashed = true);

        /// <summary>
        /// Gets a collection of descendents by the parents Id
        /// </summary>
        /// <param name="id">Id of entity to retrieve descendents for</param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        IEnumerable<IUmbracoEntity> GetDescendents(int id);

        /// <summary>
        /// Gets a collection of descendents by the parents Id
        /// </summary>
        /// <param name="id">Id of entity to retrieve descendents for</param>
        /// <param name="umbracoObjectType">UmbracoObjectType of the descendents to retrieve</param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        IEnumerable<IUmbracoEntity> GetDescendents(int id, UmbracoObjectTypes umbracoObjectType);

        /// <summary>
        /// Gets a collection of the entities at the root, which corresponds to the entities with a Parent Id of -1.
        /// </summary>
        /// <param name="umbracoObjectType">UmbracoObjectType of the root entities to retrieve</param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        IEnumerable<IUmbracoEntity> GetRootEntities(UmbracoObjectTypes umbracoObjectType);

        /// <summary>
        /// Gets a collection of all <see cref="IUmbracoEntity"/> of a given type.
        /// </summary>
        /// <typeparam name="T">Type of the entities to retrieve</typeparam>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        IEnumerable<IUmbracoEntity> GetAll<T>(params int[] ids) where T : IUmbracoEntity;

        /// <summary>
        /// Gets a collection of all <see cref="IUmbracoEntity"/> of a given type.
        /// </summary>
        /// <param name="umbracoObjectType">UmbracoObjectType of the entities to return</param>
        /// <param name="ids"></param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        IEnumerable<IUmbracoEntity> GetAll(UmbracoObjectTypes umbracoObjectType, params int[] ids);

        /// <summary>
        /// Gets a collection of all <see cref="IUmbracoEntity"/> of a given type.
        /// </summary>
        /// <param name="umbracoObjectType">UmbracoObjectType of the entities to return</param>
        /// <param name="keys"></param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        IEnumerable<IUmbracoEntity> GetAll(UmbracoObjectTypes umbracoObjectType, Guid[] keys);

        /// <summary>
        /// Gets a collection of <see cref="IUmbracoEntity"/>
        /// </summary>
        /// <param name="objectTypeId">Guid id of the UmbracoObjectType</param>
        /// <param name="ids"></param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        IEnumerable<IUmbracoEntity> GetAll(Guid objectTypeId, params int[] ids);

        /// <summary>
        /// Gets paths for entities.
        /// </summary>
        IEnumerable<EntityPath> GetAllPaths(UmbracoObjectTypes umbracoObjectType, params int[] ids);

        /// <summary>
        /// Gets paths for entities.
        /// </summary>
        IEnumerable<EntityPath> GetAllPaths(UmbracoObjectTypes umbracoObjectType, params Guid[] keys);

        /// <summary>
        /// Gets the UmbracoObjectType from the integer id of an IUmbracoEntity.
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <returns><see cref="UmbracoObjectTypes"/></returns>
        UmbracoObjectTypes GetObjectType(int id);

        /// <summary>
        /// Gets the UmbracoObjectType from an IUmbracoEntity.
        /// </summary>
        /// <param name="entity"><see cref="IUmbracoEntity"/></param>
        /// <returns><see cref="UmbracoObjectTypes"/></returns>
        UmbracoObjectTypes GetObjectType(IUmbracoEntity entity);

        /// <summary>
        /// Gets the Type of an entity by its Id
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <returns>Type of the entity</returns>
        Type GetEntityType(int id);

        /// <summary>
        /// Gets the Type of an entity by its <see cref="UmbracoObjectTypes"/>
        /// </summary>
        /// <param name="umbracoObjectType"><see cref="UmbracoObjectTypes"/></param>
        /// <returns>Type of the entity</returns>
        Type GetEntityType(UmbracoObjectTypes umbracoObjectType);

        /// <summary>
        /// Reserves an identifier for a key.
        /// </summary>
        /// <param name="key">They key.</param>
        /// <returns>The identifier.</returns>
        /// <remarks>When a new content or a media is saved with the key, it will have the reserved identifier.</remarks>
        int ReserveId(Guid key);
    }
}
