using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Actions
{
    internal class ActionCollectionBuilder : LazyCollectionBuilderBase<ActionCollectionBuilder, ActionCollection, IAction>
    {
        protected override ActionCollectionBuilder This => this;

        protected override IEnumerable<IAction> CreateItems(IFactory factory)
        {
            var items = base.CreateItems(factory).ToList();

            //validate the items, no actions should exist that do not either expose notifications or permissions
            var invalidItems = items.Where(x => !x.CanBePermissionAssigned && !x.ShowInNotifier).ToList();
            if (invalidItems.Count == 0) return items;

            var invalidActions = string.Join(", ", invalidItems.Select(x => "'" + x.Alias + "'"));
            throw new InvalidOperationException($"Invalid actions {invalidActions}'. All {typeof(IAction)} implementations must be true for either {nameof(IAction.CanBePermissionAssigned)} or {nameof(IAction.ShowInNotifier)}.");
        }
    }
}
