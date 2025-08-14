using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Services.Signs;

public class HasCollectionSignProvider : ISignProvider
{
    private const string Alias = Constants.Conventions.Signs.Prefix + "HasCollection";

    public bool CanProvideSigns<TItem>()
        where TItem : IHasSigns =>
        typeof(TItem) == typeof(DocumentTreeItemResponseModel);

    public Task PopulateSignsAsync<TItem>(IEnumerable<TItem> itemViewModels)
        where TItem : IHasSigns
    {
        foreach (TItem item in itemViewModels)
        {
            if (item is DocumentTreeItemResponseModel treeItem &&
                treeItem.DocumentType?.Collection?.Id != Guid.Empty)
            {
                item.AddSign(Alias);
            }
        }

        return Task.CompletedTask;
    }
}
