using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Trees
{
    // todo
    // this is a weird collection builder because it actually contains trees, not types
    // and it does not really rely on DI to instantiate anything - but meh
    // can we have trees that don't have a controller, or something? looks like, no
    // and then, we should not register trees here, and only create them when creating
    // the collection!

    /// <summary>
    /// Builds a <see cref="TreeCollection"/>.
    /// </summary>
    public class TreeCollectionBuilder : ICollectionBuilder<TreeCollection, Tree>
    {
        private readonly List<Tree> _trees = new List<Tree>();

        public TreeCollection CreateCollection(IFactory factory) => new TreeCollection(_trees);

        public void RegisterWith(IRegister register) => register.Register(CreateCollection, Lifetime.Singleton);

        public void AddTreeController<TController>()
            where TController : TreeControllerBase
            => AddTreeController(typeof(TController));

        public void AddTreeController(Type controllerType)
        {
            if (!typeof(TreeControllerBase).IsAssignableFrom(controllerType))
                throw new ArgumentException($"Type {controllerType} does not inherit from {typeof(TreeControllerBase).FullName}.");

            var attribute = controllerType.GetCustomAttribute<TreeAttribute>(false);
            if (attribute == null) return; // todo - shouldn't we throw or at least log?
            var tree = new Tree(attribute.SortOrder, attribute.SectionAlias, attribute.TreeGroup, attribute.TreeAlias, attribute.TreeTitle, attribute.TreeUse, controllerType, attribute.IsSingleNodeTree);
            _trees.Add(tree);
        }

        public void AddTreeControllers(IEnumerable<Type> controllerTypes)
        {
            foreach (var controllerType in controllerTypes)
                AddTreeController(controllerType);
        }

        // todo - do we want to support this?
        public void AddTree(Tree tree)
            => _trees.Add(tree);

        // todo - do we want to support this?
        public void AddTrees(IEnumerable<Tree> tree)
            => _trees.AddRange(tree);
    }
}
