using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IRelation" /> entities.
/// </summary>
public interface IRelationRepository : IReadWriteQueryRepository<int, IRelation>
{
    /// <summary>
    ///     Gets paged relations matching the specified query.
    /// </summary>
    /// <param name="query">The query to apply, or <c>null</c> to get all relations.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalRecords">Returns the total number of records.</param>
    /// <param name="ordering">The ordering to apply, or <c>null</c> for default ordering.</param>
    /// <returns>A collection of relations for the specified page.</returns>
    IEnumerable<IRelation> GetPagedRelationsByQuery(IQuery<IRelation>? query, long pageIndex, int pageSize, out long totalRecords, Ordering? ordering);

    /// <summary>
    ///     Persist multiple <see cref="IRelation" /> at once
    /// </summary>
    /// <param name="relations"></param>
    void Save(IEnumerable<IRelation> relations);

    /// <summary>
    ///     Persist multiple <see cref="IRelation" /> at once but Ids are not returned on created relations
    /// </summary>
    /// <param name="relations"></param>
    void SaveBulk(IEnumerable<ReadOnlyRelation> relations);

    /// <summary>
    ///     Deletes all relations for a parent for any specified relation type alias
    /// </summary>
    /// <param name="parentId"></param>
    /// <param name="relationTypeAliases">
    ///     A list of relation types to match for deletion, if none are specified then all relations for this parent id are deleted.
    /// </param>
    void DeleteByParent(int parentId, params string[] relationTypeAliases);

    /// <summary>
    ///     Gets paged parent entities for a child entity.
    /// </summary>
    /// <param name="childId">The identifier of the child entity.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalRecords">Returns the total number of records.</param>
    /// <param name="entityTypes">The entity types to filter by.</param>
    /// <returns>A collection of parent entities for the specified page.</returns>
    IEnumerable<IUmbracoEntity> GetPagedParentEntitiesByChildId(int childId, long pageIndex, int pageSize, out long totalRecords, params Guid[] entityTypes);

    /// <summary>
    ///     Gets paged child entities for a parent entity.
    /// </summary>
    /// <param name="parentId">The identifier of the parent entity.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalRecords">Returns the total number of records.</param>
    /// <param name="entityTypes">The entity types to filter by.</param>
    /// <returns>A collection of child entities for the specified page.</returns>
    IEnumerable<IUmbracoEntity> GetPagedChildEntitiesByParentId(int parentId, long pageIndex, int pageSize, out long totalRecords, params Guid[] entityTypes);

    /// <summary>
    ///     Gets paged relations by child entity key.
    /// </summary>
    /// <param name="childKey">The unique key of the child entity.</param>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The number of records to take.</param>
    /// <param name="relationTypeAlias">The relation type alias to filter by, or <c>null</c> for all types.</param>
    /// <returns>A paged model of relations.</returns>
    Task<PagedModel<IRelation>> GetPagedByChildKeyAsync(Guid childKey, int skip, int take, string? relationTypeAlias);
}
