// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     The action collection builder
/// </summary>
public class ActionCollectionBuilder : LazyCollectionBuilderBase<ActionCollectionBuilder, ActionCollection, IAction>
{
    /// <inheritdoc />
    protected override ActionCollectionBuilder This => this;

    /// <inheritdoc />
    protected override IEnumerable<IAction> CreateItems(IServiceProvider factory)
    {
        var items = base.CreateItems(factory).ToList();

        // Validate the items, no actions should exist that do not either expose notifications or permissions
        var invalidItems = items.Where(x => !x.CanBePermissionAssigned && !x.ShowInNotifier).ToList();
        if (invalidItems.Count == 0)
        {
            return items;
        }

        var invalidActions = string.Join(", ", invalidItems.Select(x => "'" + x.Alias + "'"));
        throw new InvalidOperationException(
            $"Invalid actions {invalidActions}'. All {typeof(IAction)} implementations must be true for either {nameof(IAction.CanBePermissionAssigned)} or {nameof(IAction.ShowInNotifier)}.");
    }
}
