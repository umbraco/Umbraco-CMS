using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for tracking references between entities in Umbraco.
/// </summary>
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
    [Obsolete("Use GetPagedRelationsForItemAsync which returns an Attempt with operation status. Scheduled for removal in Umbraco 19.")]
    Task<PagedModel<RelationItemModel>> GetPagedRelationsForItemAsync(Guid key, long skip, long take, bool filterMustBeIsDependency);

    /// <summary>
    ///     Gets a paged result of items which are in relation with the current item.
    ///     Basically, shows the items which depend on the current item.
    /// </summary>
    /// <param name="key">The identifier of the entity to retrieve relations for.</param>
    /// <param name="objectType">The Umbraco object type of the parent.</param>
    /// <param name="skip">The amount of items to skip</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <returns>A paged result of <see cref="RelationItemModel" /> objects.</returns>
    async Task<Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus>> GetPagedRelationsForItemAsync(Guid key, UmbracoObjectTypes objectType, long skip, long take, bool filterMustBeIsDependency)
#pragma warning disable CS0618 // Type or member is obsolete
        => Attempt.SucceedWithStatus(GetReferencesOperationStatus.Success, await GetPagedRelationsForItemAsync(key, skip, take, filterMustBeIsDependency));
#pragma warning restore CS0618 // Type or member is obsolete

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
    [Obsolete("Use GetPagedDescendantsInReferencesAsync which returns an Attempt with operation status. Scheduled for removal in Umbraco 19.")]
    Task<PagedModel<RelationItemModel>> GetPagedDescendantsInReferencesAsync(Guid parentKey, long skip, long take, bool filterMustBeIsDependency);

    /// <summary>
    ///     Gets a paged result of the descending items that have any references, given a parent id.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent to retrieve descendants for.</param>
    /// <param name="objectType">The Umbraco object type of the parent.</param>
    /// <param name="skip">The amount of items to skip</param>
    /// <param name="take">The amount of items to take.</param>
    /// <param name="filterMustBeIsDependency">
    ///     A boolean indicating whether to filter only the RelationTypes which are
    ///     dependencies (isDependency field is set to true).
    /// </param>
    /// <returns>An <see cref="Attempt"/> wrapping a paged result of <see cref="RelationItemModel" /> objects.</returns>
    async Task<Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus>> GetPagedDescendantsInReferencesAsync(Guid parentKey, UmbracoObjectTypes objectType, long skip, long take, bool filterMustBeIsDependency)
#pragma warning disable CS0618 // Type or member is obsolete
        => Attempt.SucceedWithStatus(GetReferencesOperationStatus.Success, await GetPagedDescendantsInReferencesAsync(parentKey, skip, take, filterMustBeIsDependency));
#pragma warning restore CS0618 // Type or member is obsolete

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

    /// <summary>
    ///     Gets a paged result of entity keys that have dependent references.
    /// </summary>
    /// <param name="keys">The set of entity keys to check for dependent references.</param>
    /// <param name="nodeObjectTypeId">The object type identifier of the nodes.</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <returns>A paged result of entity keys that have dependent references.</returns>
    Task<PagedModel<Guid>> GetPagedKeysWithDependentReferencesAsync(ISet<Guid> keys, Guid nodeObjectTypeId, long skip, long take);
}
