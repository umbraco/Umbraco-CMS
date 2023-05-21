using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface ITrackedReferencesRepository
{
    /// <summary>
    ///     Gets a page of items which are in relation with the current item.
    ///     Basically, shows the items which depend on the current item.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve relations for.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <param name="totalRecords">The total count of the items with reference to the current item.</param>
    /// <returns>An enumerable list of <see cref="RelationItem" /> objects.</returns>
    IEnumerable<RelationItem> GetPagedRelationsForItem(int id, long pageIndex, int pageSize, bool filterMustBeIsDependency, out long totalRecords);

    /// <summary>
    ///     Gets a page of items used in any kind of relation from selected integer ids.
    /// </summary>
    /// <param name="ids">The identifiers of the entities to check for relations.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <param name="totalRecords">The total count of the items in any kind of relation.</param>
    /// <returns>An enumerable list of <see cref="RelationItem" /> objects.</returns>
    IEnumerable<RelationItem> GetPagedItemsWithRelations(int[] ids, long pageIndex, int pageSize, bool filterMustBeIsDependency, out long totalRecords);

    /// <summary>
    ///     Gets a page of the descending items that have any references, given a parent id.
    /// </summary>
    /// <param name="parentId">The unique identifier of the parent to retrieve descendants for.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <param name="totalRecords">The total count of descending items.</param>
    /// <returns>An enumerable list of <see cref="RelationItem" /> objects.</returns>
    IEnumerable<RelationItem> GetPagedDescendantsInReferences(int parentId, long pageIndex, int pageSize, bool filterMustBeIsDependency, out long totalRecords);

    /// <summary>
    ///     Gets a page of items which are in relation with the current item.
    ///     Basically, shows the items which depend on the current item.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve relations for.</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <param name="totalRecords">The total count of the items with reference to the current item.</param>
    /// <returns>An enumerable list of <see cref="RelationItem" /> objects.</returns>
    IEnumerable<RelationItemModel> GetPagedRelationsForItem(
        int id,
        long skip,
        long take,
        bool filterMustBeIsDependency,
        out long totalRecords) =>
        throw new NotImplementedException();

    /// <summary>
    ///     Gets a page of items used in any kind of relation from selected integer ids.
    /// </summary>
    /// <param name="ids">The identifiers of the entities to check for relations.</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <param name="totalRecords">The total count of the items in any kind of relation.</param>
    /// <returns>An enumerable list of <see cref="RelationItem" /> objects.</returns>
    IEnumerable<RelationItemModel> GetPagedItemsWithRelations(
        int[] ids,
        long skip,
        long take,
        bool filterMustBeIsDependency,
        out long totalRecords) =>
        throw new NotImplementedException();

    /// <summary>
    ///     Gets a page of the descending items that have any references, given a parent id.
    /// </summary>
    /// <param name="parentId">The unique identifier of the parent to retrieve descendants for.</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <param name="totalRecords">The total count of descending items.</param>
    /// <returns>An enumerable list of <see cref="RelationItem" /> objects.</returns>
    IEnumerable<RelationItemModel> GetPagedDescendantsInReferences(
        int parentId,
        long skip,
        long take,
        bool filterMustBeIsDependency,
        out long totalRecords) =>
        throw new NotImplementedException();
}
