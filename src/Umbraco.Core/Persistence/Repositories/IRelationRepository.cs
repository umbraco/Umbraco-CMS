using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IRelationRepository : IReadWriteQueryRepository<int, IRelation>
{
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

    IEnumerable<IUmbracoEntity> GetPagedParentEntitiesByChildId(int childId, long pageIndex, int pageSize, out long totalRecords, params Guid[] entityTypes);

    IEnumerable<IUmbracoEntity> GetPagedChildEntitiesByParentId(int parentId, long pageIndex, int pageSize, out long totalRecords, params Guid[] entityTypes);
}
