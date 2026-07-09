using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides methods for retrieving media items in a list view format with paging and filtering support.
/// </summary>
public interface IMediaListViewService
{
    /// <summary>
    ///     Gets media items for a list view by their container key with paging and filtering support.
    /// </summary>
    /// <param name="user">The user requesting the list view items.</param>
    /// <param name="key">The key of the parent container, or null for root level items.</param>
    /// <param name="dataTypeKey">The optional data type key to filter by.</param>
    /// <param name="orderBy">The property to order results by.</param>
    /// <param name="orderDirection">The direction to order results.</param>
    /// <param name="filter">An optional filter string to apply.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <returns>An <see cref="Attempt{TResult,TStatus}"/> containing the paged media items if successful, or an error status if not.</returns>
    Task<Attempt<ListViewPagedModel<IMedia>?, ContentCollectionOperationStatus>> GetListViewItemsByKeyAsync(
        IUser user,
        Guid? key,
        Guid? dataTypeKey,
        string orderBy,
        Direction orderDirection,
        string? filter,
        int skip,
        int take);
}
