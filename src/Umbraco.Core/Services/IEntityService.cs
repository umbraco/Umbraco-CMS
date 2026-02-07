using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides methods for working with entities across Umbraco, including content, media, members, and other entity types.
/// </summary>
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
    ///     Determines whether an entity exists.
    /// </summary>
    /// <param name="keys">The unique keys of the entities.</param>
    bool Exists(IEnumerable<Guid> keys);

    /// <summary>
    /// Determines whether and entity of a certain object type exists.
    /// </summary>
    /// <param name="key">The unique key of the entity.</param>
    /// <param name="objectType">The object type to look for.</param>
    /// <returns>True if the entity exists, false if it does not.</returns>
    bool Exists(Guid key, UmbracoObjectTypes objectType) => throw new NotImplementedException();

    /// <summary>
    /// Determines whether and entity of a certain object type exists.
    /// </summary>
    /// <param name="id">The id of the entity.</param>
    /// <param name="objectType">The object type to look for.</param>
    /// <returns>True if the entity exists, false if it does not.</returns>
    bool Exists(int id, UmbracoObjectTypes objectType) => throw new NotImplementedException();

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
    /// Gets non-trashed sibling entities of a specified target entity, within a given range before and after the target, ordered as specified.
    /// </summary>
    /// <param name="key">The key of the target entity whose siblings are to be retrieved.</param>
    /// <param name="objectTypes">The object types of the entities.</param>
    /// <param name="before">The number of siblings to retrieve before the target entity. Needs to be greater or equal to 0.</param>
    /// <param name="after">The number of siblings to retrieve after the target entity. Needs to be greater or equal to 0.</param>
    /// <param name="filter">An optional filter to apply to the result set.</param>
    /// <param name="ordering">The ordering to apply to the siblings.</param>
    /// <param name="totalBefore">Outputs the total number of siblings before the target entity.</param>
    /// <param name="totalAfter">Outputs the total number of siblings after the target entity.</param>
    /// <returns>Enumerable of non-trashed sibling entities.</returns>
    IEnumerable<IEntitySlim> GetSiblings(
        Guid key,
        IEnumerable<UmbracoObjectTypes> objectTypes,
        int before,
        int after,
        out long totalBefore,
        out long totalAfter,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null)
    {
        totalBefore = 0;
        totalAfter = 0;
        return [];
    }

    /// <summary>
    /// Gets trashed sibling entities of a specified target entity, within a given range before and after the target, ordered as specified.
    /// </summary>
    /// <param name="key">The key of the target entity whose siblings are to be retrieved.</param>
    /// <param name="objectTypes">The object types of the entities.</param>
    /// <param name="before">The number of siblings to retrieve before the target entity. Needs to be greater or equal to 0.</param>
    /// <param name="after">The number of siblings to retrieve after the target entity. Needs to be greater or equal to 0.</param>
    /// <param name="filter">An optional filter to apply to the result set.</param>
    /// <param name="ordering">The ordering to apply to the siblings.</param>
    /// <param name="totalBefore">Outputs the total number of siblings before the target entity.</param>
    /// <param name="totalAfter">Outputs the total number of siblings after the target entity.</param>
    /// <returns>Enumerable of trashed sibling entities.</returns>
    IEnumerable<IEntitySlim> GetTrashedSiblings(
        Guid key,
        IEnumerable<UmbracoObjectTypes> objectTypes,
        int before,
        int after,
        out long totalBefore,
        out long totalAfter,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null)
    {
        totalBefore = 0;
        totalAfter = 0;
        return [];
    }

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
    ///     Gets the children of an entity.
    /// </summary>
    /// <param name="key">The unique key of the parent entity, or null for root level entities.</param>
    /// <param name="objectType">The object type of the children.</param>
    /// <returns>An enumerable collection of child entities.</returns>
    IEnumerable<IEntitySlim> GetChildren(Guid? key, UmbracoObjectTypes objectType)
    {
        return Array.Empty<IEntitySlim>();
    }

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
    ///     Gets children of an entity with paging support.
    /// </summary>
    /// <param name="parentKey">The unique key of the parent entity, or null for root level entities.</param>
    /// <param name="childObjectType">The object type of the children.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <param name="totalRecords">Outputs the total number of records.</param>
    /// <param name="filter">An optional filter to apply to the result set.</param>
    /// <param name="ordering">The ordering to apply to the children.</param>
    /// <returns>An enumerable collection of child entities.</returns>
    IEnumerable<IEntitySlim> GetPagedChildren(
        Guid? parentKey,
        UmbracoObjectTypes childObjectType,
        int skip,
        int take,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null)
        => GetPagedChildren(
            parentKey,
            new[] { childObjectType },
            childObjectType,
            skip,
            take,
            out totalRecords,
            filter,
            ordering);

    /// <summary>
    ///     Gets children of an entity with paging support, filtering by parent object types.
    /// </summary>
    /// <param name="parentKey">The unique key of the parent entity, or null for root level entities.</param>
    /// <param name="parentObjectTypes">The object types of the parent entities.</param>
    /// <param name="childObjectType">The object type of the children.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <param name="totalRecords">Outputs the total number of records.</param>
    /// <param name="filter">An optional filter to apply to the result set.</param>
    /// <param name="ordering">The ordering to apply to the children.</param>
    /// <returns>An enumerable collection of child entities.</returns>
    IEnumerable<IEntitySlim> GetPagedChildren(
        Guid? parentKey,
        IEnumerable<UmbracoObjectTypes> parentObjectTypes,
        UmbracoObjectTypes childObjectType,
        int skip,
        int take,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null)
    {
        totalRecords = 0;
        return Array.Empty<IEntitySlim>();
    }

    /// <summary>
    ///     Gets children of an entity with paging support, filtering by parent and child object types and trashed state.
    /// </summary>
    /// <param name="parentKey">The unique key of the parent entity, or null for root level entities.</param>
    /// <param name="parentObjectTypes">The object types of the parent entities.</param>
    /// <param name="childObjectTypes">The object types of the children.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <param name="trashed">A value indicating whether to include trashed entities.</param>
    /// <param name="totalRecords">Outputs the total number of records.</param>
    /// <param name="filter">An optional filter to apply to the result set.</param>
    /// <param name="ordering">The ordering to apply to the children.</param>
    /// <returns>An enumerable collection of child entities.</returns>
    IEnumerable<IEntitySlim> GetPagedChildren(
        Guid? parentKey,
        IEnumerable<UmbracoObjectTypes> parentObjectTypes,
        IEnumerable<UmbracoObjectTypes> childObjectTypes,
        int skip,
        int take,
        bool trashed,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null)
    {
        totalRecords = 0;
        return Array.Empty<IEntitySlim>();
    }

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
    ///     Gets children of an entity.
    /// </summary>
    IEnumerable<IEntitySlim> GetPagedTrashedChildren(
        Guid? key,
        UmbracoObjectTypes objectType,
        int skip,
        int take,
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

    /// <summary>
    /// Gets the GUID keys for an entity's path (provided as a comma separated list of integer Ids).
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="omitSelf">A value indicating whether to omit the entity's own key from the result.</param>
    /// <returns>The path with each ID converted to a GUID.</returns>
    Guid[] GetPathKeys(ITreeEntity entity, bool omitSelf = false) => [];

    IEnumerable<IEntitySlim> GetPagedDescendants(
        IEnumerable<UmbracoObjectTypes> objectTypes,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter = null,
        Ordering? ordering = null,
        bool includeTrashed = true);
}
