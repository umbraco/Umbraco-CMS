﻿using Umbraco.Cms.Core.Models;
using Umbraco.New.Cms.Core.Models.TrackedReferences;

namespace Umbraco.New.Cms.Core.Persistence.Repositories;

public interface ITrackedReferencesSkipTakeRepository
{
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
    IEnumerable<RelationItemModel> GetPagedRelationsForItem(int id, long skip, long take, bool filterMustBeIsDependency, out long totalRecords);

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
    IEnumerable<RelationItemModel> GetPagedItemsWithRelations(int[] ids, long skip, long take, bool filterMustBeIsDependency, out long totalRecords);

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
    IEnumerable<RelationItemModel> GetPagedDescendantsInReferences(int parentId, long skip, long take, bool filterMustBeIsDependency, out long totalRecords);
}
