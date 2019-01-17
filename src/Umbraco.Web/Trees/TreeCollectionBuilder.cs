using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Trees
{
    public class TreeCollectionBuilder : ICollectionBuilder<TreeCollection, Tree>
    {
        /// <summary>
        /// expose the list of trees which developers can manipulate before the collection is created
        /// </summary>
        public List<Tree> Trees { get; } = new List<Tree>();

        public TreeCollection CreateCollection(IFactory factory) => new TreeCollection(Trees);

        public void RegisterWith(IRegister register) => register.Register(CreateCollection, Lifetime.Singleton);
    }
}
