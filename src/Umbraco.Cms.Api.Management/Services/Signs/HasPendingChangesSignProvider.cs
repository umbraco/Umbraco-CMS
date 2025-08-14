using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;


namespace Umbraco.Cms.Api.Management.Services.Signs;

public class HasPendingChangesSignProvider : ISignProvider
{
    private const string Alias = Constants.Conventions.Signs.Prefix + "PendingChanges";

    public bool CanProvideSigns<TItem>()
        where TItem : IHasSigns =>
        typeof(TItem) == typeof(DocumentTreeItemResponseModel) ||
        typeof(TItem) == typeof(DocumentCollectionResponseModel);

    public Task PopulateSignsAsync<TItem>(IEnumerable<TItem> itemViewModels)
        where TItem : IHasSigns
    {
        foreach (TItem item in itemViewModels)
        {
            switch (item)
            {
                case DocumentTreeItemResponseModel treeItem:
                    foreach (DocumentVariantItemResponseModel variant in treeItem.Variants)
                    {
                        DocumentVariantState state = variant.State;
                        if (state == DocumentVariantState.PublishedPendingChanges)
                        {
                            item.AddSign(Alias);
                            break;
                        }
                    }

                    break;

                case DocumentCollectionResponseModel collectionItem:
                    foreach (DocumentVariantResponseModel variant in collectionItem.Variants)
                    {
                        var state = variant.State;
                        if (state == DocumentVariantState.PublishedPendingChanges)
                        {
                            item.AddSign(Alias);
                            break;
                        }
                    }

                    break;
            }
        }

        return Task.CompletedTask;
    }
}
