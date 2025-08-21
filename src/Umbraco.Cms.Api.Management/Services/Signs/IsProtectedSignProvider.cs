using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Services.Signs;

/// <summary>
/// Implements a <see cref="ISignProvider"/> that provides signs for documents that are protected.
/// </summary>
internal class IsProtectedSignProvider : ISignProvider
{
    private const string Alias = Constants.Conventions.Signs.Prefix + "IsProtected";

    /// <inheritdoc/>>
    public bool CanProvideSigns<TItem>()
        where TItem : IHasSigns =>
        typeof(IIsProtected).IsAssignableFrom(typeof(TItem));

    /// <inheritdoc/>>
    public Task PopulateSignsAsync<TItem>(IEnumerable<TItem> itemViewModels)
        where TItem : IHasSigns
    {
        foreach (TItem item in itemViewModels)
        {
            if (item is IIsProtected { IsProtected: true })
            {
                item.AddSign(Alias);
            }
        }

        return Task.CompletedTask;
    }
}
