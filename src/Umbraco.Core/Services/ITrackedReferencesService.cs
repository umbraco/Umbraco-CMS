using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface ITrackedReferencesService
{
    /// <summary>
    ///     Gets a paged result of items which are in relation with the current item.
    ///     Basically, shows the items which depend on the current item.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve relations for.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <returns>A paged result of <see cref="RelationItem" /> objects.</returns>
    [Obsolete("Use overload that takes key instead of id. This will be removed in Umbraco 15.")]
    PagedResult<RelationItem> GetPagedRelationsForItem(int id, long pageIndex, int pageSize, bool filterMustBeIsDependency);

    /// <summary>
    ///     Gets a paged result of the descending items that have any references, given a parent id.
    /// </summary>
    /// <param name="parentId">The unique identifier of the parent to retrieve descendants for.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <returns>A paged result of <see cref="RelationItem" /> objects.</returns>
    [Obsolete("Use overload that takes key instead of id. This will be removed in Umbraco 15.")]
    PagedResult<RelationItem> GetPagedDescendantsInReferences(int parentId, long pageIndex, int pageSize, bool filterMustBeIsDependency);

    /// <summary>
    ///     Gets a paged result of items used in any kind of relation from selected integer ids.
    /// </summary>
    /// <param name="ids">The identifiers of the entities to check for relations.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <returns>A paged result of <see cref="RelationItem" /> objects.</returns>
    [Obsolete("Use method that takes key (Guid) instead of id (int). This will be removed in Umbraco 15.")]
    PagedResult<RelationItem> GetPagedItemsWithRelations(int[] ids, long pageIndex, int pageSize, bool filterMustBeIsDependency);

    [Obsolete("Use method that takes key (Guid) instead of id (int). This will be removed in Umbraco 15.")]
    PagedModel<RelationItemModel> GetPagedRelationsForItem(int id, long skip, long take, bool filterMustBeIsDependency);

    /// <summary>
    ///     Gets a paged result of items which are in relation with the current item.
    ///     Basically, shows the items which depend on the current item.
    /// </summary>
    /// <param name="key">The identifier of the entity to retrieve relations for.</param>
    /// <param name="skip">The amount of items to skip</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <returns>A paged result of <see cref="RelationItemModel" /> objects.</returns>
    Task<PagedModel<RelationItemModel>> GetPagedRelationsForItemAsync(Guid key, long skip, long take, bool filterMustBeIsDependency);

    /// <summary>
    ///     Gets a paged result of items which are in relation with an item in the recycle bin.
    /// </summary>
    /// <param name="objectType">The Umbraco object type that has recycle bin support (currently Document or Media).</param>
    /// <param name="skip">The amount of items to skip</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <returns>A paged result of <see cref="RelationItemModel" /> objects.</returns>
    Task<PagedModel<RelationItemModel>> GetPagedRelationsForRecycleBinAsync(UmbracoObjectTypes objectType, long skip, long take, bool filterMustBeIsDependency)
        => Task.FromResult(new PagedModel<RelationItemModel>(0, []));

    [Obsolete("Use method that takes key (Guid) instead of id (int). This will be removed in Umbraco 15.")]
    PagedModel<RelationItemModel> GetPagedDescendantsInReferences(int parentId, long skip, long take, bool filterMustBeIsDependency);

    /// <summary>
    ///     Gets a paged result of the descending items that have any references, given a parent id.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent to retrieve descendants for.</param>
    /// <param name="skip">The amount of items to skip</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <returns>A paged result of <see cref="RelationItemModel" /> objects.</returns>
    Task<PagedModel<RelationItemModel>> GetPagedDescendantsInReferencesAsync(Guid parentKey, long skip, long take, bool filterMustBeIsDependency);

    [Obsolete("Use method that takes keys (Guid) instead of ids (int). This will be removed in Umbraco 15.")]
    PagedModel<RelationItemModel> GetPagedItemsWithRelations(int[] ids, long skip, long take,
        bool filterMustBeIsDependency);

    /// <summary>
    ///     Gets a paged result of items used in any kind of relation from selected integer ids.
    /// </summary>
    /// <param name="keys">The identifiers of the entities to check for relations.</param>
    /// <param name="skip">The amount of items to skip</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <returns>A paged result of <see cref="RelationItemModel" /> objects.</returns>
    Task<PagedModel<RelationItemModel>> GetPagedItemsWithRelationsAsync(ISet<Guid> keys, long skip, long take,
        bool filterMustBeIsDependency);

    Task<PagedModel<Guid>> GetPagedKeysWithDependentReferencesAsync(ISet<Guid> keys, Guid nodeObjectTypeId, long skip, long take)
    {
        PagedModel<RelationItemModel> pagedItems = GetPagedItemsWithRelationsAsync(keys, skip, take, true).GetAwaiter().GetResult();
        return Task.FromResult(new PagedModel<Guid>(pagedItems.Total, pagedItems.Items.Select(i => i.NodeKey)));
    }
}
