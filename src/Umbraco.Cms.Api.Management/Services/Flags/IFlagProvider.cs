using Umbraco.Cms.Api.Management.ViewModels;

namespace Umbraco.Cms.Api.Management.Services.Flags;

/// <summary>
/// Defines operation for the provision of presentation flags for item, tree and collection nodes.
/// </summary>
public interface IFlagProvider
{
    /// <summary>
    /// Gets a value indicating whether this provider can provide flags for the specified item type.
    /// </summary>
    /// <typeparam name="TItem">Type of view model supporting flags.</typeparam>
    bool CanProvideFlags<TItem>()
        where TItem : IHasFlags;

    /// <summary>
    /// Populates the provided item view models with flags.
    /// </summary>
    /// <typeparam name="TItem">Type of item view model supporting flags.</typeparam>
    Task PopulateFlagsAsync<TItem>(IEnumerable<TItem> items)
        where TItem : IHasFlags;
}
