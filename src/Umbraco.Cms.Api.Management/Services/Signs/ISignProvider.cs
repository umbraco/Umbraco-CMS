using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Services.Signs;

/// <summary>
/// Defines operation for the provision of presentation signs for tree and collection nodes.
/// </summary>
public interface ISignProvider
{
    /// <summary>
    /// Gets a value indicating whether this provider can provide signs for the specified item type.
    /// </summary>
    /// <typeparam name="TItem">Type of view model supporting signs.</typeparam>
    bool CanProvideSigns<TItem>()
        where TItem : IHasSigns;

    /// <summary>
    /// Populates the provided tree item view models with signs.
    /// </summary>
    /// <typeparam name="TItem">Type of tree item view model supporting signs.</typeparam>
    /// <param name="itemViewModels">The collection of tree item view models to be populated with signs.</param>
    Task PopulateTreeSignsAsync<TItem>(IEnumerable<TItem> itemViewModels)
        where TItem : EntityTreeItemResponseModel, IHasSigns;

    /// <summary>
    /// Populates the provided collection view models with signs.
    /// </summary>
    /// <typeparam name="TItem">Type of collection view model supporting signs.</typeparam>
    /// <param name="itemViewModels">The collection of view models to be populated with signs.</param>
    Task PopulateCollectionSignsAsync<TItem>(IEnumerable<TItem> itemViewModels)
        where TItem : IHasSigns;
}
