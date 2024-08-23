using System.Collections.Concurrent;
using Umbraco.Cms.Core.Models.Navigation;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class NavigationFactory
{
    /// <summary>
    ///     Builds a dictionary of NavigationNode objects from a given dataset.
    /// </summary>
    /// <param name="dataset">The dataset of <see cref="NavigationDto" /> objects used to build the navigation nodes dictionary.</param>
    /// <returns>A dictionary of <see cref="NavigationNode" /> objects with key corresponding to their unique guid.</returns>
    public static ConcurrentDictionary<Guid, NavigationNode> BuildNavigationDictionary(IEnumerable<NavigationDto> dataset)
    {
        var nodesStructure = new ConcurrentDictionary<Guid, NavigationNode>();
        var datasetList = dataset.ToList();
        var idToKeyMap = datasetList.ToDictionary(x => x.Id, x => x.Key);

        foreach (NavigationDto dto in datasetList)
        {
            var node = new NavigationNode(dto.Key);
            nodesStructure[dto.Key] = node;

            // We don't set the parent for items under root, it will stay null
            if (dto.ParentId == -1)
            {
                continue;
            }

            if (idToKeyMap.TryGetValue(dto.ParentId, out Guid parentKey) is false)
            {
                continue;
            }

            // If the parent node exists in the nodesStructure, add the node to the parent's children (parent is set as well)
            if (nodesStructure.TryGetValue(parentKey, out NavigationNode? parentNode))
            {
                parentNode.AddChild(node);
            }
        }

        return nodesStructure;
    }
}
