using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services
{
    public interface ITrackedReferencesService
    {
        PagedResult<RelationItem> GetPagedRelationsForItems(int[] ids, long pageIndex, int pageSize, bool filterMustBeIsDependency);


        PagedResult<RelationItem> GetPagedDescendantsInReferences(int parentId, long pageIndex, int pageSize, bool filterMustBeIsDependency);


        PagedResult<RelationItem> GetPagedItemsWithRelations(int[] ids, long pageIndex, int pageSize, bool filterMustBeIsDependency);
    }
}
