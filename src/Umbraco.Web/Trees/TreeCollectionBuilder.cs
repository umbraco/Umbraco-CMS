using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Trees
{
    //fixme - how will we allow users to modify these items? they will need to be able to change the ApplicationTree's registered (i.e. sort order, section)
    public class TreeCollectionBuilder : CollectionBuilderBase<TreeCollectionBuilder, TreeCollection, ApplicationTree>
    {
        private readonly List<ApplicationTree> _instances = new List<ApplicationTree>();

        public void AddTree(ApplicationTree tree)
        {
            _instances.Add(tree);
        }

        protected override IEnumerable<ApplicationTree> CreateItems(IFactory factory) => _instances;
    }
}
