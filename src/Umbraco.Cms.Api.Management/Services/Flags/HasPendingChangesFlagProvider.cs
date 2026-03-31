using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Services.Flags;

/// <summary>
/// Implements a <see cref="IFlagProvider"/> that provides flags for documents and elements that have pending changes.
/// </summary>
public class HasPendingChangesFlagProvider : IFlagProvider
{
    private const string Alias = Constants.Conventions.Flags.Prefix + "PendingChanges";

    /// <inheritdoc/>
    public bool CanProvideFlags<TItem>()
        where TItem : IHasFlags =>
        typeof(TItem) == typeof(DocumentVariantItemResponseModel) ||
        typeof(TItem) == typeof(DocumentVariantResponseModel) ||
        typeof(TItem) == typeof(ElementVariantItemResponseModel) ||
        typeof(TItem) == typeof(ElementVariantResponseModel);

    /// <inheritdoc/>
    public Task PopulateFlagsAsync<TItem>(IEnumerable<TItem> items)
        where TItem : IHasFlags
    {
        foreach (TItem item in items)
        {
            DocumentVariantState? state = item switch
            {
                DocumentVariantItemResponseModel variant => variant.State,
                DocumentVariantResponseModel variant => variant.State,
                ElementVariantItemResponseModel variant => variant.State,
                ElementVariantResponseModel variant => variant.State,
                _ => null,
            };

            if (state == DocumentVariantState.PublishedPendingChanges)
            {
                item.AddFlag(Alias);
            }
        }

        return Task.CompletedTask;
    }
}
