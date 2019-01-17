using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Trees
{
    //fixme - how will we allow users to modify these items? they will need to be able to change the ApplicationTree's registered (i.e. sort order, section)
    public class TreeCollectionBuilder : CollectionBuilderBase<TreeCollectionBuilder, TreeCollection, Tree>
    {
        private readonly List<Tree> _instances = new List<Tree>();

        public void AddTree(Tree tree)
        {
            _instances.Add(tree);
        }

        protected override IEnumerable<Tree> CreateItems(IFactory factory) => _instances;
    }
}
