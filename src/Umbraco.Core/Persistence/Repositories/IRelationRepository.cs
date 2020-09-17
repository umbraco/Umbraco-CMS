using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Services;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IRelationRepository : IReadWriteQueryRepository<int, IRelation>
    {
        IEnumerable<IRelation> GetPagedRelationsByQuery(IQuery<IRelation> query, long pageIndex, int pageSize, out long totalRecords, Ordering ordering);

        /// <summary>
        /// Persist multiple <see cref="IRelation"/> at once
        /// </summary>
        /// <param name="relations"></param>
        void Save(IEnumerable<IRelation> relations);

        /// <summary>
        /// Deletes all relations for a parent for any specified relation type alias
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="relationTypeAliases">
        /// A list of relation types to match for deletion, if none are specified then all relations for this parent id are deleted
        /// </param>
        void DeleteByParent(int parentId, params string[] relationTypeAliases);

        IEnumerable<IUmbracoEntity> GetPagedParentEntitiesByChildId(int childId, long pageIndex, int pageSize, out long totalRecords, params Guid[] entityTypes);

        IEnumerable<IUmbracoEntity> GetPagedParentEntitiesByChildId(int childId, long pageIndex, int pageSize, out long totalRecords, int[] relationTypes, params Guid[] entityTypes);

        IEnumerable<IUmbracoEntity> GetPagedChildEntitiesByParentId(int parentId, long pageIndex, int pageSize, out long totalRecords, params Guid[] entityTypes);

        IEnumerable<IUmbracoEntity> GetPagedChildEntitiesByParentId(int parentId, long pageIndex, int pageSize, out long totalRecords, int[] relationTypes, params Guid[] entityTypes);
    }
}
