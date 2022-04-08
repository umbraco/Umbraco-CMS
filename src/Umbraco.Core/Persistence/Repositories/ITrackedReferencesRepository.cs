using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface ITrackedReferencesRepository
    {
        IEnumerable<RelationItem> GetPagedRelationsForItems(int[] ids, long pageIndex, int pageSize, bool filterMustBeIsDependency,out long totalRecords);
        IEnumerable<RelationItem> GetPagedItemsWithRelations(int[] ids, long pageIndex, int pageSize, bool filterMustBeIsDependency,out long totalRecords);
        IEnumerable<RelationItem> GetPagedDescendantsInReferences(int parentId, long pageIndex, int pageSize, bool filterMustBeIsDependency,out long totalRecords);
    }
}
