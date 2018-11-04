using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightInject;
using Umbraco.Core.Composing;


namespace Umbraco.Web.Actions
{
    internal class ActionCollectionBuilder : LazyCollectionBuilderBase<ActionCollectionBuilder, ActionCollection, IAction>
    {
        public ActionCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override ActionCollectionBuilder This => this;

        protected override IEnumerable<IAction> CreateItems(params object[] args)
        {
            var items = base.CreateItems(args).ToList();
            //validate the items, no actions should exist that do not either expose notifications or permissions
            var invalid = items.Where(x => !x.CanBePermissionAssigned && !x.ShowInNotifier).ToList();
            if (invalid.Count > 0)
            {
                throw new InvalidOperationException($"Invalid actions '{string.Join(", ", invalid.Select(x => x.Alias))}'. All {typeof(IAction)} implementations must be true for either {nameof(IAction.CanBePermissionAssigned)} or {nameof(IAction.ShowInNotifier)}");
            }
            return items;
        }
    }
}
