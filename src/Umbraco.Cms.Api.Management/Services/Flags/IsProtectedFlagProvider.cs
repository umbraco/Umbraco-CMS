using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Services.Flags;

/// <summary>
/// Implements a <see cref="IFlagProvider"/> that provides flags for documents that are protected.
/// </summary>
internal class IsProtectedFlagProvider : IFlagProvider
{
    private const string Alias = Constants.Conventions.Flags.Prefix + "IsProtected";

    /// <inheritdoc/>>
    public bool CanProvideFlags<TItem>()
        where TItem : IHasFlags =>
        typeof(IIsProtected).IsAssignableFrom(typeof(TItem));

    /// <inheritdoc/>>
    public Task PopulateFlagsAsync<TItem>(IEnumerable<TItem> itemViewModels)
        where TItem : IHasFlags
    {
        foreach (TItem item in itemViewModels)
        {
            if (item is IIsProtected { IsProtected: true })
            {
                item.AddFlag(Alias);
            }
        }

        return Task.CompletedTask;
    }
}
