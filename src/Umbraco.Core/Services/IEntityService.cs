using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

public interface IEntityService
{
    /// <summary>
    ///     Gets an entity.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    IEntitySlim? Get(int id);

    /// <summary>
    ///     Gets an entity.
    /// </summary>
    /// <param name="key">The unique key of the entity.</param>
    IEntitySlim? Get(Guid key);

    /// <summary>
    ///     Gets an entity.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="objectType">The object type of the entity.</param>
    IEntitySlim? Get(int id, UmbracoObjectTypes objectType);

    /// <summary>
    ///     Gets an entity.
    /// </summary>
    /// <param name="key">The unique key of the entity.</param>
    /// <param name="objectType">The object type of the entity.</param>
    IEntitySlim? Get(Guid key, UmbracoObjectTypes objectType);

    /// <summary>
    ///     Gets an entity.
    /// </summary>
    /// <typeparam name="T">The type used to determine the object type of the entity.</typeparam>
    /// <param name="id">The identifier of the entity.</param>
    IEntitySlim? Get<T>(int id)
        where T : IUmbracoEntity;

    /// <summary>
    ///     Gets an entity.
    /// </summary>
    /// <typeparam name="T">The type used to determine the object type of the entity.</typeparam>
    /// <param name="key">The unique key of the entity.</param>
    IEntitySlim? Get<T>(Guid key)
        where T : IUmbracoEntity;

    /// <summary>
    ///     Determines whether an entity exists.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    bool Exists(int id);

    /// <summary>
    ///     Determines whether an entity exists.
    /// </summary>
    /// <param name="key">The unique key of the entity.</param>
    bool Exists(Guid key);

    /// <summary>
    ///     Gets entities of a given object type.
    /// </summary>
    /// <typeparam name="T">The type used to determine the object type of the entities.</typeparam>
    IEnumerable<IEntitySlim> GetAll<T>()
        where T : IUmbracoEntity;

    /// <summary>
    ///     Gets entities of a given object type.
    /// </summary>
    /// <typeparam name="T">The type used to determine the object type of the entities.</typeparam>
    /// <param name="ids">The identifiers of the entities.</param>
    /// <remarks>If <paramref name="ids" /> is empty, returns all entities.</remarks>
    IEnumerable<IEntitySlim> GetAll<T>(params int[] ids)
        where T : IUmbracoEntity;

    /// <summary>
    ///     Gets entities of a given object type.
    /// </summary>
    /// <param name="objectType">The object type of the entities.</param>
    IEnumerable<IEntitySlim> GetAll(UmbracoObjectTypes objectType);

    /// <summary>
    ///     Gets entities of a given object type.
    /// </summary>
    /// <param name="objectType">The object type of the entities.</param>
    /// <param name="ids">The identifiers of the entities.</param>
    /// <remarks>If <paramref name="ids" /> is empty, returns all entities.</remarks>
    IEnumerable<IEntitySlim> GetAll(UmbracoObjectTypes objectType, params int[] ids);

    /// <summary>
    ///     Gets entities of a given object type.
    /// </summary>
    /// <param name="objectType">The object type of the entities.</param>
    IEnumerable<IEntitySlim> GetAll(Guid objectType);

    /// <summary>
    ///     Gets entities of a given object type.
    /// </summary>
    /// <param name="objectType">The object type of the entities.</param>
    /// <param name="ids">The identifiers of the entities.</param>
    /// <remarks>If <paramref name="ids" /> is empty, returns all entities.</remarks>
    IEnumerable<IEntitySlim> GetAll(Guid objectType, params int[] ids);

    /// <summary>
    ///     Gets entities of a given object type.
    /// </summary>
    /// <typeparam name="T">The type used to determine the object type of the entities.</typeparam>
    /// <param name="keys">The unique identifiers of the entities.</param>
    /// <remarks>If <paramref name="keys" /> is empty, returns all entities.</remarks>
    IEnumerable<IEntitySlim> GetAll<T>(params Guid[] keys)
        where T : IUmbracoEntity;

    /// <summary>
    ///     Gets entities of a given object type.
    /// </summary>
    /// <param name="objectType">The object type of the entities.</param>
    /// <param name="keys">The unique identifiers of the entities.</param>
    /// <remarks>If <paramref name="keys" /> is empty, returns all entities.</remarks>
    IEnumerable<IEntitySlim> GetAll(UmbracoObjectTypes objectType, Guid[] keys);

    /// <summary>
    ///     Gets entities of a given object type.
    /// </summary>
    /// <param name="objectType">The object type of the entities.</param>
    /// <param name="keys">The unique identifiers of the entities.</param>
    /// <remarks>If <paramref name="keys" /> is empty, returns all entities.</remarks>
    IEnumerable<IEntitySlim> GetAll(Guid objectType, params Guid[] keys);

    /// <summary>
    ///     Gets entities at root.
    /// </summary>
    /// <param name="objectType">The object type of the entities.</param>
    IEnumerable<IEntitySlim> GetRootEntities(UmbracoObjectTypes objectType);

    /// <summary>
    ///     Gets the parent of an entity.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    IEntitySlim? GetParent(int id);

    /// <summary>
    ///     Gets the parent of an entity.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="objectType">The object type of the parent.</param>
    IEntitySlim? GetParent(int id, UmbracoObjectTypes objectType);

    /// <summary>
    ///     Gets the children of an entity.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    IEnumerable<IEntitySlim> GetChildren(int id);

    /// <summary>
    ///     Gets the children of an entity.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="objectType">The object type of the children.</param>
    IEnumerable<IEntitySlim> GetChildren(int id, UmbracoObjectTypes objectType);

    /// <summary>
    ///     Gets the descendants of an entity.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    IEnumerable<IEntitySlim> GetDescendants(int id);

    /// <summary>
    ///     Gets the descendants of an entity.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="objectType">The object type of the descendants.</param>
    IEnumerable<IEntitySlim> GetDescendants(int id, UmbracoObjectTypes objectType);

    /// <summary>
    ///     Gets children of an entity.
    /// </summary>
    IEnumerable<IEntitySlim> GetPagedChildren(
        int id,
        UmbracoObjectTypes objectType,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null);

    /// <summary>
    ///     Gets children of an entity.
    /// </summary>
    IEnumerable<IEntitySlim> GetPagedTrashedChildren(
        int id,
        UmbracoObjectTypes objectType,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null)
    {
        totalRecords = 0;
        return Array.Empty<IEntitySlim>();
    }

    /// <summary>
    ///     Gets descendants of an entity.
    /// </summary>
    IEnumerable<IEntitySlim> GetPagedDescendants(
        int id,
        UmbracoObjectTypes objectType,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null);

    /// <summary>
    ///     Gets descendants of entities.
    /// </summary>
    IEnumerable<IEntitySlim> GetPagedDescendants(
        IEnumerable<int> ids,
        UmbracoObjectTypes objectType,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null);

    // TODO: Do we really need this? why not just pass in -1

    /// <summary>
    ///     Gets descendants of root.
    /// </summary>
    IEnumerable<IEntitySlim> GetPagedDescendants(
        UmbracoObjectTypes objectType,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null,
        bool includeTrashed = true);

    /// <summary>
    ///     Gets the object type of an entity.
    /// </summary>
    UmbracoObjectTypes GetObjectType(int id);

    /// <summary>
    ///     Gets the object type of an entity.
    /// </summary>
    UmbracoObjectTypes GetObjectType(Guid key);

    /// <summary>
    ///     Gets the object type of an entity.
    /// </summary>
    UmbracoObjectTypes GetObjectType(IUmbracoEntity entity);

    /// <summary>
    ///     Gets the CLR type of an entity.
    /// </summary>
    Type? GetEntityType(int id);

    /// <summary>
    ///     Gets the integer identifier corresponding to a unique Guid identifier.
    /// </summary>
    Attempt<int> GetId(Guid key, UmbracoObjectTypes objectType);

    /// <summary>
    ///     Gets the integer identifier corresponding to a Udi.
    /// </summary>
    Attempt<int> GetId(Udi udi);

    /// <summary>
    ///     Gets the unique Guid identifier corresponding to an integer identifier.
    /// </summary>
    Attempt<Guid> GetKey(int id, UmbracoObjectTypes umbracoObjectType);

    /// <summary>
    ///     Gets paths for entities.
    /// </summary>
    IEnumerable<TreeEntityPath> GetAllPaths(UmbracoObjectTypes objectType, params int[]? ids);

    /// <summary>
    ///     Gets paths for entities.
    /// </summary>
    IEnumerable<TreeEntityPath> GetAllPaths(UmbracoObjectTypes objectType, params Guid[] keys);

    /// <summary>
    ///     Reserves an identifier for a key.
    /// </summary>
    /// <param name="key">They key.</param>
    /// <returns>The identifier.</returns>
    /// <remarks>When a new content or a media is saved with the key, it will have the reserved identifier.</remarks>
    int ReserveId(Guid key);
}
