using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Services.Signs;

/// <summary>
/// Implements a <see cref="ISignProvider"/> that provides signs for documents that have pending changes.
/// </summary>
public class HasPendingChangesSignProvider : ISignProvider
{
    private const string Alias = Constants.Conventions.Signs.Prefix + "PendingChanges";

    /// <inheritdoc/>
    public bool CanProvideSigns<TItem>()
        where TItem : IHasSigns =>
        typeof(TItem) == typeof(DocumentTreeItemResponseModel) ||
        typeof(TItem) == typeof(DocumentCollectionResponseModel) ||
        typeof(TItem) == typeof(DocumentItemResponseModel);

    /// <inheritdoc/>
    public Task PopulateSignsAsync<TItem>(IEnumerable<TItem> itemViewModels)
        where TItem : IHasSigns
    {
        foreach (TItem item in itemViewModels)
        {
            switch (item)
            {
                case DocumentTreeItemResponseModel response:
                    if (response.Variants.Any(variant => variant.State == DocumentVariantState.PublishedPendingChanges))
                    {
                        item.AddSign(Alias);
                    }

                    break;

                case DocumentCollectionResponseModel response:
                    if (response.Variants.Any(variant => variant.State == DocumentVariantState.PublishedPendingChanges))
                    {
                        item.AddSign(Alias);
                    }

                    break;

                case DocumentItemResponseModel response:
                    if (response.Variants.Any(variant => variant.State == DocumentVariantState.PublishedPendingChanges))
                    {
                        item.AddSign(Alias);
                    }

                    break;
            }
        }

        return Task.CompletedTask;
    }
}
