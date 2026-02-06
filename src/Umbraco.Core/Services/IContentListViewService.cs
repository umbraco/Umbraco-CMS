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
}
