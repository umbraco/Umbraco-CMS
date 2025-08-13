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
    /// Populates the provided item view models with signs.
    /// </summary>
    /// <typeparam name="TItem">Type of item view model supporting signs.</typeparam>
    /// <param name="itemViewModels">The collection of item view models to be populated with signs.</param>
    Task PopulateSignsAsync<TItem>(IEnumerable<TItem> itemViewModels)
        where TItem : IHasSigns;
}
