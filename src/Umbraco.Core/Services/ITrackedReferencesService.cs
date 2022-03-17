using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services
{
    public interface ITrackedReferencesService
    {
        /// <summary>
        /// Gets a paged result of items which are in relation with the current item.
        /// Basically, shows the items which depend on the current item.
        /// </summary>
        /// <param name="ids">The identifier of the entity to retrieve relations for.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="filterMustBeIsDependency">A boolean indicating whether to filter only the RelationTypes which are dependencies (isDependency field is set to true).</param>
        /// <returns>A paged result of <see cref="RelationItem"/> objects.</returns>
        PagedResult<RelationItem> GetPagedRelationsForItems(int[] ids, long pageIndex, int pageSize, bool filterMustBeIsDependency);

        /// <summary>
        /// Gets a paged result of the descending items that have any references, given a parent id.
        /// </summary>
        /// <param name="parentId">The unique identifier of the parent to retrieve descendants for.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="filterMustBeIsDependency">A boolean indicating whether to filter only the RelationTypes which are dependencies (isDependency field is set to true).</param>
        /// <returns>A paged result of <see cref="RelationItem"/> objects.</returns>
        PagedResult<RelationItem> GetPagedDescendantsInReferences(int parentId, long pageIndex, int pageSize, bool filterMustBeIsDependency);

        /// <summary>
        /// Gets a paged result of items used in any kind of relation from selected integer ids.
        /// </summary>
        /// <param name="ids">The identifiers of the entities to check for relations.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="filterMustBeIsDependency">A boolean indicating whether to filter only the RelationTypes which are dependencies (isDependency field is set to true).</param>
        /// <returns>A paged result of <see cref="RelationItem"/> objects.</returns>
        PagedResult<RelationItem> GetPagedItemsWithRelations(int[] ids, long pageIndex, int pageSize, bool filterMustBeIsDependency);
    }
}
