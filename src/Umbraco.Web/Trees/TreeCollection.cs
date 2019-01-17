using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Trees
{
    public class TreeCollection : BuilderCollectionBase<ApplicationTree>
    {
        public TreeCollection(IEnumerable<ApplicationTree> items)
            : base(items)
        { }
    }

    public class TreeCollectionBuilder : LazyCollectionBuilderBase<TreeCollectionBuilder, TreeCollection, ApplicationTree>
    {
        protected override TreeCollectionBuilder This => this;

        private readonly List<ApplicationTree> _instances = new List<ApplicationTree>();

        public void AddTree(ApplicationTree tree)
        {
            _instances.Add(tree);
        }

        protected override IEnumerable<ApplicationTree> CreateItems(IFactory factory)
        {
            return _instances;

            //var items = base.CreateItems(factory).ToList();
            //throw new NotImplementedException();
            ////validate the items, no actions should exist that do not either expose notifications or permissions
            //var invalidItems = items.Where(x => !x.CanBePermissionAssigned && !x.ShowInNotifier).ToList();
            //if (invalidItems.Count == 0) return items;

            //var invalidActions = string.Join(", ", invalidItems.Select(x => "'" + x.Alias + "'"));
            //throw new InvalidOperationException($"Invalid actions {invalidActions}'. All {typeof(IAction)} implementations must be true for either {nameof(IAction.CanBePermissionAssigned)} or {nameof(IAction.ShowInNotifier)}.");
        }
    }

}
