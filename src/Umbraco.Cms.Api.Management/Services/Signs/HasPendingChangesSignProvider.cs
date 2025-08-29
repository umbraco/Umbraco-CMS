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
            foreach (IHasSigns variant in HasPendingChanges(item))
            {
                variant.AddSign(Alias);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Determines if the given item has any variant that has pending changes.
    /// </summary>
    private static IEnumerable<IHasSigns> HasPendingChanges(object item) => item switch
    {
        DocumentTreeItemResponseModel { Variants: var v } => v.Where(x => x.State == DocumentVariantState.PublishedPendingChanges),
        DocumentCollectionResponseModel { Variants: var v } => v.Where(x => x.State == DocumentVariantState.PublishedPendingChanges),
        DocumentItemResponseModel { Variants: var v } => v.Where(x => x.State == DocumentVariantState.PublishedPendingChanges),
        _ => [],
    };
}
