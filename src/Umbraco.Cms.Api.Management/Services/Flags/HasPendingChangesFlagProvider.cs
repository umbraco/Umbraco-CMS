﻿using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Services.Flags;

/// <summary>
/// Implements a <see cref="IFlagProvider"/> that provides flags for documents that have pending changes.
/// </summary>
public class HasPendingChangesFlagProvider : IFlagProvider
{
    private const string Alias = Constants.Conventions.Flags.Prefix + "PendingChanges";

    /// <inheritdoc/>
    public bool CanProvideFlags<TItem>()
        where TItem : IHasFlags =>
        typeof(TItem) == typeof(DocumentVariantItemResponseModel) ||
        typeof(TItem) == typeof(DocumentVariantResponseModel);


    /// <inheritdoc/>
    public Task PopulateFlagsAsync<TItem>(IEnumerable<TItem> items)
        where TItem : IHasFlags
    {
        foreach (TItem item in items)
        {
            if (HasPendingChanges(item))
            {
                item.AddFlag(Alias);
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
