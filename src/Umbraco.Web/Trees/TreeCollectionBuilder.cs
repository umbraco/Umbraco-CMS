using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Trees
{
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

            // no all TreeControllerBase are meant to be used here,
            // ignore those that don't have the attribute

            var attribute = controllerType.GetCustomAttribute<TreeAttribute>(false);
            if (attribute == null) return;
            var tree = new Tree(attribute.SortOrder, attribute.SectionAlias, attribute.TreeGroup, attribute.TreeAlias, attribute.TreeTitle, attribute.TreeUse, controllerType, attribute.IsSingleNodeTree);
            _trees.Add(tree);
        }

        public void AddTreeControllers(IEnumerable<Type> controllerTypes)
        {
            foreach (var controllerType in controllerTypes)
                AddTreeController(controllerType);
        }
    }
}
