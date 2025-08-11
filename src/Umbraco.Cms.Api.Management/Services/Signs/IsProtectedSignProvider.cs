using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Services.Signs;

internal class IsProtectedSignProvider : ISignProvider
{
    private const string Alias = Constants.Conventions.Signs.Prefix + "IsProtected";

    /// <inheritdoc/>>
    public bool CanProvideTreeSigns<TItem>() => typeof(TItem) == typeof(DocumentTreeItemResponseModel);

    /// <inheritdoc/>>
    public Task PopulateTreeSignsAsync<TItem>(TItem[] treeItemViewModels, IEnumerable<IEntitySlim> entities)
        where TItem : EntityTreeItemResponseModel, new()
    {
        foreach (TItem item in treeItemViewModels)
        {
            if (item is DocumentTreeItemResponseModel { IsProtected: true })
            {
                item.AddSign(Alias);
            }
        }

        return Task.CompletedTask;
    }
}
