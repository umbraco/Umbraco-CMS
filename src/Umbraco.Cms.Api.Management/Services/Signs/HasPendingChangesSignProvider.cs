using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
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
        typeof(TItem) == typeof(DocumentVariantItemResponseModel) ||
        typeof(TItem) == typeof(DocumentVariantResponseModel);


    /// <inheritdoc/>
    public Task PopulateSignsAsync<TItem>(IEnumerable<TItem> items)
        where TItem : IHasSigns
    {
        foreach (TItem item in items)
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
    private static bool HasPendingChanges(object item) => item switch
    {
        DocumentVariantItemResponseModel variant => variant.State == DocumentVariantState.PublishedPendingChanges,
        DocumentVariantResponseModel variant => variant.State == DocumentVariantState.PublishedPendingChanges,
        _ => false,
    };
}
