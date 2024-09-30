using System.Collections.Concurrent;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Navigation;

namespace Umbraco.Cms.Core.Factories;

internal static class NavigationFactory
{
    /// <summary>
    ///     Builds a dictionary of NavigationNode objects from a given dataset.
    /// </summary>
    /// <param name="nodesStructure">A dictionary of <see cref="NavigationNode" /> objects with key corresponding to their unique Guid.</param>
    /// <param name="entities">The <see cref="INavigationModel" /> objects used to build the navigation nodes dictionary.</param>
    public static void BuildNavigationDictionary(ConcurrentDictionary<Guid, NavigationNode> nodesStructure, IEnumerable<INavigationModel> entities)
    {
        var entityList = entities.ToList();
        Dictionary<int, Guid> idToKeyMap = entityList.ToDictionary(x => x.Id, x => x.Key);

        foreach (INavigationModel entity in entityList)
        {
            var node = new NavigationNode(entity.Key);
            nodesStructure[entity.Key] = node;

            // We don't set the parent for items under root, it will stay null
            if (entity.ParentId == -1)
            {
                continue;
            }

            if (idToKeyMap.TryGetValue(entity.ParentId, out Guid parentKey) is false)
            {
                continue;
            }

            // If the parent node exists in the nodesStructure, add the node to the parent's children (parent is set as well)
            if (nodesStructure.TryGetValue(parentKey, out NavigationNode? parentNode))
            {
                parentNode.AddChild(node);
            }
        }
    }
}
