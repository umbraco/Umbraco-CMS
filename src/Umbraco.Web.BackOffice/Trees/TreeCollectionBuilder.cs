using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Trees;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    /// <summary>
    /// Builds a <see cref="TreeCollection"/>.
    /// </summary>
    public class TreeCollectionBuilder : ICollectionBuilder<TreeCollection, Tree>
    {
        private readonly List<Tree> _trees = new List<Tree>();

        public TreeCollection CreateCollection(IServiceProvider factory) => new TreeCollection(() => _trees);

        public void RegisterWith(IServiceCollection services) => services.Add(new ServiceDescriptor(typeof(TreeCollection), CreateCollection, ServiceLifetime.Singleton));


        /// <summary>
        /// Registers a custom tree definition
        /// </summary>
        /// <param name="treeDefinition"></param>
        /// <remarks>
        /// This is useful if a developer wishes to have a single tree controller for different tree aliases. In this case the tree controller
        /// cannot be decorated with the TreeAttribute (since then it will be auto-registered).
        /// </remarks>
        public void AddTree(Tree treeDefinition)
        {
            if (treeDefinition == null) throw new ArgumentNullException(nameof(treeDefinition));
            _trees.Add(treeDefinition);
        }

        public void AddTreeController<TController>()
            where TController : TreeControllerBase
            => AddTreeController(typeof(TController));

        public void AddTreeController(Type controllerType)
        {
            if (!typeof(TreeControllerBase).IsAssignableFrom(controllerType))
                throw new ArgumentException($"Type {controllerType} does not inherit from {typeof(TreeControllerBase).FullName}.");

            // not all TreeControllerBase are meant to be used here,
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

        public void RemoveTreeController<T>() => RemoveTreeController(typeof(T));

        public void RemoveTreeController(Type type)
        {
            var tree = _trees.FirstOrDefault(it => it.TreeControllerType == type);
            if (tree != null)
            {
                _trees.Remove(tree);
            }
        }
    }
}
