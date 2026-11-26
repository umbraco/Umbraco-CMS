using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides services for retrieving content items in a list view format.
/// </summary>
public interface IContentListViewService
{
    /// <summary>
    ///     Gets content items for display in a list view.
    /// </summary>
    /// <param name="user">The user requesting the list view items.</param>
    /// <param name="key">The unique identifier of the parent content item.</param>
    /// <param name="dataTypeKey">The optional data type key to filter the list view configuration.</param>
    /// <param name="orderBy">The property alias to order by.</param>
    /// <param name="orderCulture">The culture to use for ordering culture-variant properties.</param>
    /// <param name="orderDirection">The direction of the ordering (ascending or descending).</param>
    /// <param name="filter">An optional filter string to apply to the results.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <returns>An attempt containing the paged list view result or an error status.</returns>
    [Obsolete("Please use the overload that accepts an originDataTypeKey parameter. Scheduled for removal in Umbraco 20.")]
    Task<Attempt<ListViewPagedModel<IContent>?, ContentCollectionOperationStatus>> GetListViewItemsByKeyAsync(
        IUser user,
        Guid key,
        Guid? dataTypeKey,
        string orderBy,
        string? orderCulture,
        Direction orderDirection,
        string? filter,
        int skip,
        int take);

    /// <summary>
    ///     Gets content items for display in a list view, for a request originating from a data type.
    /// </summary>
    /// <param name="user">The user requesting the list view items.</param>
    /// <param name="key">The unique identifier of the parent content item.</param>
    /// <param name="dataTypeKey">The optional data type key to filter the list view configuration.</param>
    /// <param name="orderBy">The property alias to order by.</param>
    /// <param name="orderCulture">The culture to use for ordering culture-variant properties.</param>
    /// <param name="orderDirection">The direction of the ordering (ascending or descending).</param>
    /// <param name="filter">An optional filter string to apply to the results.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <param name="originDataTypeKey">
    ///     The key of the data type the request originates from, if any, such as a picker browsing the list view.
    ///     When that data type is configured to ignore user start nodes, the user's start nodes are not applied to the items.
    /// </param>
    /// <returns>An attempt containing the paged list view result or an error status.</returns>
    // TODO (V20): Remove the default implementation when the obsolete overload is removed.
    Task<Attempt<ListViewPagedModel<IContent>?, ContentCollectionOperationStatus>> GetListViewItemsByKeyAsync(
        IUser user,
        Guid key,
        Guid? dataTypeKey,
        string orderBy,
        string? orderCulture,
        Direction orderDirection,
        string? filter,
        int skip,
        int take,
        Guid? originDataTypeKey)
#pragma warning disable CS0618 // Type or member is obsolete
        => GetListViewItemsByKeyAsync(user, key, dataTypeKey, orderBy, orderCulture, orderDirection, filter, skip, take);
#pragma warning restore CS0618 // Type or member is obsolete
}
