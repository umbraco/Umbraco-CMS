using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IEntityRepository : IRepository
{
    IEntitySlim? Get(int id);

    IEntitySlim? Get(Guid key);

    IEntitySlim? Get(int id, Guid objectTypeId);

    IEntitySlim? Get(Guid key, Guid objectTypeId);

    IEnumerable<IEntitySlim> GetAll(Guid objectType, params int[] ids);

    IEnumerable<IEntitySlim> GetAll(Guid objectType, params Guid[] keys);

    /// <summary>
    /// Gets sibling entities of a specified target entity, within a given range before and after the target, ordered as specified.
    /// </summary>
    /// <param name="objectType">The object type key of the entities.</param>
    /// <param name="targetKey">The key of the target entity whose siblings are to be retrieved.</param>
    /// <param name="before">The number of siblings to retrieve before the target entity.</param>
    /// <param name="after">The number of siblings to retrieve after the target entity.</param>
    /// <param name="ordering">The ordering to apply to the siblings.</param>
    /// <returns>Enumerable of sibling entities.</returns>
    IEnumerable<IEntitySlim> GetSiblings(Guid objectType, Guid targetKey, int before, int after, Ordering ordering) => [];

    /// <summary>
    ///     Gets entities for a query
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    IEnumerable<IEntitySlim> GetByQuery(IQuery<IUmbracoEntity> query);

    /// <summary>
    ///     Gets entities for a query and a specific object type allowing the query to be slightly more optimized
    /// </summary>
    /// <param name="query"></param>
    /// <param name="objectType"></param>
    /// <returns></returns>
    IEnumerable<IEntitySlim> GetByQuery(IQuery<IUmbracoEntity> query, Guid objectType);

    UmbracoObjectTypes GetObjectType(int id);

    UmbracoObjectTypes GetObjectType(Guid key);

    int ReserveId(Guid key);

    IEnumerable<TreeEntityPath> GetAllPaths(Guid objectType, params int[]? ids);

    IEnumerable<TreeEntityPath> GetAllPaths(Guid objectType, params Guid[] keys);

    bool Exists(int id);

    bool Exists(Guid key);

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
    ///     Gets paged entities for a query and a specific object type
    /// </summary>
    /// <param name="query"></param>
    /// <param name="objectType"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalRecords"></param>
    /// <param name="filter"></param>
    /// <param name="ordering"></param>
    /// <returns></returns>
    IEnumerable<IEntitySlim> GetPagedResultsByQuery(
        IQuery<IUmbracoEntity> query,
        Guid objectType,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter,
        Ordering? ordering) =>
        GetPagedResultsByQuery(query, new HashSet<Guid>(){objectType}, pageIndex, pageSize, out totalRecords, filter, ordering);

    IEnumerable<IEntitySlim> GetPagedResultsByQuery(
        IQuery<IUmbracoEntity> query,
        ISet<Guid> objectTypes,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IUmbracoEntity>? filter,
        Ordering? ordering);

    int CountByQuery(IQuery<IUmbracoEntity> query, Guid objectType, IQuery<IUmbracoEntity>? filter) =>
        CountByQuery(query, new HashSet<Guid>() { objectType }, filter);
    int CountByQuery(IQuery<IUmbracoEntity> query, IEnumerable<Guid> objectTypes, IQuery<IUmbracoEntity>? filter);
}
