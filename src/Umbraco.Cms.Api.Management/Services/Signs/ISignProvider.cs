using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Services.Signs;

/// <summary>
/// Defines operation for the provision of presentation signs for tree and collection nodes.
/// </summary>
public interface ISignProvider
{
    /// <summary>
    /// Gets a value indicating whether this provider can provide tree signs for the specified item type.
    /// </summary>
    /// <typeparam name="TItem">Type of tree item view model.</typeparam>
    /// <returns></returns>
    bool CanProvideTreeSigns<TItem>();

    /// <summary>
    /// Populates the provided tree item view models with signs.
    /// </summary>
    /// <typeparam name="TItem">Type of tree item view model.</typeparam>
    /// <param name="treeItemViewModels">The collection of tree item view models populated with signs.</param>
    /// <param name="entities">The entities from which the collection of tree item view models was populated.</param>
    Task PopulateTreeSignsAsync<TItem>(TItem[] treeItemViewModels, IEnumerable<IEntitySlim> entities)
        where TItem : EntityTreeItemResponseModel, new();
}
