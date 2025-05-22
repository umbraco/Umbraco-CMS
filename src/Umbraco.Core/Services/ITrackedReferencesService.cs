using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface ITrackedReferencesService
{
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
    Task<PagedModel<RelationItemModel>> GetPagedRelationsForRecycleBinAsync(UmbracoObjectTypes objectType, long skip, long take, bool filterMustBeIsDependency);

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
    Task<PagedModel<RelationItemModel>> GetPagedItemsWithRelationsAsync(ISet<Guid> keys, long skip, long take, bool filterMustBeIsDependency);

    Task<PagedModel<Guid>> GetPagedKeysWithDependentReferencesAsync(ISet<Guid> keys, Guid nodeObjectTypeId, long skip, long take);
}
