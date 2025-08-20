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
            if (HasPendingChanges(item))
            {
                item.AddSign(Alias);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Determines if the given item has any variant that has pending changes.
    /// </summary>
    private bool HasPendingChanges(object item) => item switch
    {
        DocumentTreeItemResponseModel { Variants: var v } when v.Any(x => x.State == DocumentVariantState.PublishedPendingChanges) => true,
        DocumentCollectionResponseModel { Variants: var v } when v.Any(x => x.State == DocumentVariantState.PublishedPendingChanges) => true,
        DocumentItemResponseModel { Variants: var v } when v.Any(x => x.State == DocumentVariantState.PublishedPendingChanges) => true,
        _ => false,
    };
}
