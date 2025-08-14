using NPoco;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

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
                        }
                    }

                    break;

                case DocumentCollectionResponseModel collectionItem:
                    foreach (DocumentVariantItemResponseModel variant in collectionItem.Variants.OfType<DocumentVariantItemResponseModel>())
                    {
                        DocumentVariantState state = variant.State;
                        if (state == DocumentVariantState.PublishedPendingChanges)
                        {
                            item.AddSign(Alias);
                        }
                    }

                    break;
            }
        }

        return Task.CompletedTask;
    }
}
