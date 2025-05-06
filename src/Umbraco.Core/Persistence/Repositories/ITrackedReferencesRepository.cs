using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface ITrackedReferencesRepository
{
    /// <summary>
    ///     Gets a page of items which are in relation with the current item.
    ///     Basically, shows the items which depend on the current item.
    /// </summary>
    /// <param name="key">The identifier of the entity to retrieve relations for.</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <param name="totalRecords">The total count of the items with reference to the current item.</param>
    /// <returns>An enumerable list of <see cref="RelationItem" /> objects.</returns>
    IEnumerable<RelationItemModel> GetPagedRelationsForItem(
        Guid key,
        long skip,
        long take,
        bool filterMustBeIsDependency,
        out long totalRecords);

    /// <summary>
    ///     Gets a paged result of items which are in relation with an item in the recycle bin.
    /// </summary>
    /// <param name="objectTypeKey">The Umbraco object type that has recycle bin support (currently Document or Media).</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <param name="totalRecords">The total count of the items with reference to the current item.</param>
    /// <returns>An enumerable list of <see cref="RelationItem" /> objects.</returns>
    IEnumerable<RelationItemModel> GetPagedRelationsForRecycleBin(
        Guid objectTypeKey,
        long skip,
        long take,
        bool filterMustBeIsDependency,
        out long totalRecords);

    /// <summary>
    ///     Gets a page of items used in any kind of relation from selected integer ids.
    /// </summary>
    /// <param name="keys">The identifiers of the entities to check for relations.</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <param name="totalRecords">The total count of the items in any kind of relation.</param>
    /// <returns>An enumerable list of <see cref="RelationItem" /> objects.</returns>
    IEnumerable<RelationItemModel> GetPagedItemsWithRelations(
        ISet<Guid> keys,
        long skip,
        long take,
        bool filterMustBeIsDependency,
        out long totalRecords);

    /// <summary>
    ///     Gets a page of the descending items that have any references, given a parent id.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent to retrieve descendants for.</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <param name="totalRecords">The total count of descending items.</param>
    /// <returns>An enumerable list of <see cref="RelationItem" /> objects.</returns>
    IEnumerable<RelationItemModel> GetPagedDescendantsInReferences(
        Guid parentKey,
        long skip,
        long take,
        bool filterMustBeIsDependency,
        out long totalRecords);

    /// <summary>
    /// Gets a paged collection of node keys that have dependant references.
    /// </summary>
    /// <param name="keys">The keys to check for relations.</param>
    /// <param name="nodeObjectTypeId">The node object Id.</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <returns></returns>
    Task<PagedModel<Guid>> GetPagedNodeKeysWithDependantReferencesAsync(
        ISet<Guid> keys,
        Guid nodeObjectTypeId,
        long skip,
        long take);
}
