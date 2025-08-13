using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
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
        typeof(TItem) == typeof(DocumentTreeItemResponseModel) ||
        typeof(TItem) == typeof(DocumentCollectionResponseModel);

    /// <inheritdoc/>
    public Task PopulateTreeSignsAsync<TItem>(IEnumerable<TItem> itemViewModels)
        where TItem : EntityTreeItemResponseModel, IHasSigns
        => PopulateSigns(itemViewModels, x => x is DocumentTreeItemResponseModel { IsProtected: true });

    /// <inheritdoc/>
    public Task PopulateCollectionSignsAsync<TItem>(IEnumerable<TItem> itemViewModels)
        where TItem : IHasSigns
        => PopulateSigns(itemViewModels, x => x is DocumentCollectionResponseModel { IsProtected: true });

    private static Task PopulateSigns<TItem>(IEnumerable<TItem> itemViewModels, Func<TItem, bool> discrimator)
        where TItem : IHasSigns
    {
        foreach (TItem item in itemViewModels)
        {
            if (discrimator(item))
            {
                item.AddSign(Alias);
            }
        }

        return Task.CompletedTask;
    }
}
