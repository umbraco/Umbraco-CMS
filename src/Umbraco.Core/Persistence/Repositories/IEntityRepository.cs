using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for lightweight entity operations.
/// </summary>
public interface IEntityRepository : IRepository
{
    /// <summary>
    ///     Gets an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns>The entity if found; otherwise, <c>null</c>.</returns>
    IEntitySlim? Get(int id);

    /// <summary>
    ///     Gets an entity by its unique key.
    /// </summary>
    /// <param name="key">The unique key of the entity.</param>
    /// <returns>The entity if found; otherwise, <c>null</c>.</returns>
    IEntitySlim? Get(Guid key);

    /// <summary>
    ///     Gets an entity by its identifier and object type.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="objectTypeId">The object type identifier.</param>
    /// <returns>The entity if found; otherwise, <c>null</c>.</returns>
    IEntitySlim? Get(int id, Guid objectTypeId);

    /// <summary>
    ///     Gets an entity by its unique key and object type.
    /// </summary>
    /// <param name="key">The unique key of the entity.</param>
    /// <param name="objectTypeId">The object type identifier.</param>
    /// <returns>The entity if found; otherwise, <c>null</c>.</returns>
    IEntitySlim? Get(Guid key, Guid objectTypeId);

    /// <summary>
    ///     Gets all entities of a specific object type by their identifiers.
    /// </summary>
    /// <param name="objectType">The object type identifier.</param>
    /// <param name="ids">The identifiers of the entities.</param>
    /// <returns>A collection of entities.</returns>
    IEnumerable<IEntitySlim> GetAll(Guid objectType, params int[] ids);

    /// <summary>
    ///     Gets entities of multiple object types.
    /// </summary>
    /// <param name="objectTypes">The object types of the entities.</param>
    /// <param name="ids">The identifiers of the entities.</param>
    /// <remarks>If <paramref name="ids" /> is empty, returns all entities of the specified types.</remarks>
    IEnumerable<IEntitySlim> GetAll(IEnumerable<Guid> objectTypes, params int[] ids)
        => throw new NotImplementedException(); // TODO (V19): Remove default implementation.

    /// <summary>
    ///     Gets all entities of a specific object type by their unique keys.
    /// </summary>
    /// <param name="objectType">The object type identifier.</param>
    /// <param name="keys">The unique keys of the entities.</param>
    /// <returns>A collection of entities.</returns>
    IEnumerable<IEntitySlim> GetAll(Guid objectType, params Guid[] keys);

    /// <summary>
    ///     Gets entities of multiple object types.
    /// </summary>
    /// <param name="objectTypes">The object types of the entities.</param>
    /// <param name="keys">The unique identifiers of the entities.</param>
    /// <remarks>If <paramref name="keys" /> is empty, returns all entities of the specified types.</remarks>
    IEnumerable<IEntitySlim> GetAll(IEnumerable<Guid> objectTypes, params Guid[] keys)
        => throw new NotImplementedException(); // TODO (V19): Remove default implementation.

    /// <summary>
    /// Gets sibling entities of a specified target entity, within a given range before and after the target, ordered as specified.
    /// </summary>
    /// <param name="objectTypes">The object type keys of the entities.</param>
    /// <param name="targetKey">The key of the target entity whose siblings are to be retrieved.</param>
    /// <param name="before">The number of siblings to retrieve before the target entity.</param>
    /// <param name="after">The number of siblings to retrieve after the target entity.</param>
    /// <param name="filter">An optional filter to apply to the result set.</param>
    /// <param name="ordering">The ordering to apply to the siblings.</param>
    /// <param name="totalBefore">Outputs the total number of siblings before the target entity.</param>
    /// <param name="totalAfter">Outputs the total number of siblings after the target entity.</param>
    /// <returns>Enumerable of sibling entities.</returns>
    IEnumerable<IEntitySlim> GetSiblings(
        ISet<Guid> objectTypes,
        Guid targetKey,
        int before,
        int after,
        IQuery<IUmbracoEntity>? filter,
        Ordering ordering,
        out long totalBefore,
        out long totalAfter)
    {
        totalBefore = 0;
        totalAfter = 0;
        return [];
    }

    /// <summary>
    /// Gets trashed sibling entities of a specified target entity, within a given range before and after the target, ordered as specified.
    /// </summary>
    /// <param name="objectTypes">The object type keys of the entities.</param>
    /// <param name="targetKey">The key of the target entity whose siblings are to be retrieved.</param>
    /// <param name="before">The number of siblings to retrieve before the target entity.</param>
    /// <param name="after">The number of siblings to retrieve after the target entity.</param>
    /// <param name="filter">An optional filter to apply to the result set.</param>
    /// <param name="ordering">The ordering to apply to the siblings.</param>
    /// <param name="totalBefore">Outputs the total number of siblings before the target entity.</param>
    /// <param name="totalAfter">Outputs the total number of siblings after the target entity.</param>
    /// <returns>Enumerable of trashed sibling entities.</returns>
    IEnumerable<IEntitySlim> GetTrashedSiblings(
        ISet<Guid> objectTypes,
        Guid targetKey,
        int before,
        int after,
        IQuery<IUmbracoEntity>? filter,
        Ordering ordering,
        out long totalBefore,
        out long totalAfter)
    {
        totalBefore = 0;
        totalAfter = 0;
        return [];
    }

    /// <summary>
    ///     Gets entities for a query.
    /// </summary>
    /// <param name="query">The query to apply.</param>
    /// <returns>A collection of entities matching the query.</returns>
    IEnumerable<IEntitySlim> GetByQuery(IQuery<IUmbracoEntity> query);

    /// <summary>
    ///     Gets entities for a query and a specific object type, allowing the query to be slightly more optimized.
    /// </summary>
    /// <param name="query">The query to apply.</param>
    /// <param name="objectType">The object type identifier.</param>
    /// <returns>A collection of entities matching the query.</returns>
    IEnumerable<IEntitySlim> GetByQuery(IQuery<IUmbracoEntity> query, Guid objectType);

    /// <summary>
    ///     Gets the object type for an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns>The object type of the entity.</returns>
    UmbracoObjectTypes GetObjectType(int id);

    /// <summary>
    ///     Gets the object type for an entity by its unique key.
    /// </summary>
    /// <param name="key">The unique key of the entity.</param>
    /// <returns>The object type of the entity.</returns>
    UmbracoObjectTypes GetObjectType(Guid key);

    /// <summary>
    ///     Reserves an identifier for a given unique key.
    /// </summary>
    /// <param name="key">The unique key to reserve an identifier for.</param>
    /// <returns>The reserved identifier.</returns>
    int ReserveId(Guid key);

    /// <summary>
    ///     Gets all entity paths for a specific object type.
    /// </summary>
    /// <param name="objectType">The object type identifier.</param>
    /// <param name="ids">Optional identifiers to filter the results.</param>
    /// <returns>A collection of entity paths.</returns>
    IEnumerable<TreeEntityPath> GetAllPaths(Guid objectType, params int[]? ids);

    /// <summary>
    ///     Gets all entity paths for a specific object type.
    /// </summary>
    /// <param name="objectType">The object type identifier.</param>
    /// <param name="keys">The unique keys to filter the results.</param>
    /// <returns>A collection of entity paths.</returns>
    IEnumerable<TreeEntityPath> GetAllPaths(Guid objectType, params Guid[] keys);

    /// <summary>
    ///     Checks whether an entity with the specified identifier exists.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns><c>true</c> if the entity exists; otherwise, <c>false</c>.</returns>
    bool Exists(int id);

    /// <summary>
    ///     Checks whether an entity with the specified unique key exists.
    /// </summary>
    /// <param name="key">The unique key of the entity.</param>
    /// <returns><c>true</c> if the entity exists; otherwise, <c>false</c>.</returns>
    bool Exists(Guid key);

    /// <summary>
    ///     Checks whether entities with the specified unique keys exist.
    /// </summary>
    /// <param name="keys">The unique keys of the entities.</param>
    /// <returns><c>true</c> if all entities exist; otherwise, <c>false</c>.</returns>
    bool Exists(IEnumerable<Guid> keys);

    /// <summary>
    /// Asserts if an entity with the given object type exists.
    /// </summary>
    /// <param name="key">The Key of the entity to find.</param>
    /// <param name="objectType">The object type key of the entity.</param>
    /// <returns>True if an entity with the given key and object type exists.</returns>
    bool Exists(Guid key, Guid objectType) => throw new NotImplementedException();

    /// <summary>
    /// Asserts if an entity with the given object type exists.
    /// </summary>
    /// <param name="id">The id of the entity to find.</param>
    /// <param name="objectType">The object type key of the entity.</param>
    /// <returns>True if an entity with the given id and object type exists.</returns>
    bool Exists(int id, Guid objectType) => throw new NotImplementedException();

    /// <summary>
    ///     Gets paged entities for a query and a specific object type.
    /// </summary>
    /// <param name="query">The query to apply.</param>
    /// <param name="objectType">The object type identifier.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalRecords">Returns the total number of records.</param>
    /// <param name="filter">An optional filter query.</param>
    /// <param name="ordering">The ordering to apply, or <c>null</c> for default ordering.</param>
    /// <returns>A collection of entities for the specified page.</returns>
    IEnumerable<IEntitySlim> GetPagedResultsByQuery(
        IQuery<IUmbracoEntity> query,
        Guid objectType,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter,
        Ordering? ordering) =>
        GetPagedResultsByQuery(query, new HashSet<Guid>(){objectType}, pageIndex, pageSize, out totalRecords, filter, ordering);

    /// <summary>
    ///     Gets paged entities for a query and multiple object types.
    /// </summary>
    /// <param name="query">The query to apply.</param>
    /// <param name="objectTypes">The object type identifiers.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalRecords">Returns the total number of records.</param>
    /// <param name="filter">An optional filter query.</param>
    /// <param name="ordering">The ordering to apply, or <c>null</c> for default ordering.</param>
    /// <returns>A collection of entities for the specified page.</returns>
    IEnumerable<IEntitySlim> GetPagedResultsByQuery(
        IQuery<IUmbracoEntity> query,
        ISet<Guid> objectTypes,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter,
        Ordering? ordering);

    /// <summary>
    ///     Counts entities matching a query for a specific object type.
    /// </summary>
    /// <param name="query">The query to apply.</param>
    /// <param name="objectType">The object type identifier.</param>
    /// <param name="filter">An optional filter query.</param>
    /// <returns>The count of entities matching the query.</returns>
    int CountByQuery(IQuery<IUmbracoEntity> query, Guid objectType, IQuery<IUmbracoEntity>? filter) =>
        CountByQuery(query, new HashSet<Guid>() { objectType }, filter);

    /// <summary>
    ///     Counts entities matching a query for multiple object types.
    /// </summary>
    /// <param name="query">The query to apply.</param>
    /// <param name="objectTypes">The object type identifiers.</param>
    /// <param name="filter">An optional filter query.</param>
    /// <returns>The count of entities matching the query.</returns>
    int CountByQuery(IQuery<IUmbracoEntity> query, IEnumerable<Guid> objectTypes, IQuery<IUmbracoEntity>? filter);
}
