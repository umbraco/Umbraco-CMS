using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    [Obsolete("This interface will be merged with IRelationRepository in Umbraco 11")]
    public interface IRelationWithRelationTypesRepository : IRelationRepository
    {
        IEnumerable<IUmbracoEntity> GetPagedParentEntitiesByChildId(int childId, long pageIndex, int pageSize, out long totalRecords, int[] relationTypes, params Guid[] entityTypes);

        IEnumerable<IUmbracoEntity> GetPagedParentEntitiesByChildIds(int[] childIds, long pageIndex, int pageSize, out long totalRecords, int[] relationTypes, params Guid[] entityTypes);

        IEnumerable<IUmbracoEntity> GetPagedChildEntitiesByParentId(int parentId, long pageIndex, int pageSize, out long totalRecords, int[] relationTypes, params Guid[] entityTypes);

        IEnumerable<IUmbracoEntity> GetPagedEntitiesForItemsInRelation(int[] itemIds, long pageIndex, int pageSize, out long totalRecords, params Guid[] entityTypes);
    }
}
