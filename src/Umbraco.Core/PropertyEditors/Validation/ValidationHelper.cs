using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
/// Provides helper methods for validation of property editor values based on data type configuration.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Checks if a provided value is valid based on the configured step and minimum values.
    /// </summary>
    /// <param name="value">The provided value.</param>
    /// <param name="min">The configured minimum value.</param>
    /// <param name="step">The configured step value.</param>
    /// <returns>True if the value is valid otherwise false.</returns>
    public static bool IsValueValidForStep(decimal value, decimal min, decimal step)
    {
        if (value < min)
        {
            return true; // Outside of the range, so we expect another validator will have picked this up.
        }

        return (value - min) % step == 0;
    }

    /// <summary>
    /// Checks if all provided entities has the start node as an ancestor.
    /// </summary>
    /// <param name="entityKeys">Keys to check.</param>
    /// <param name="startNode">The configured start node.</param>
    /// <param name="navigationQueryService">The navigation query service to use for the checks.</param>
    /// <returns>True if the startnode key is in the ancestry tree.</returns>
    public static bool HasValidStartNode(IEnumerable<Guid> entityKeys, Guid startNode, INavigationQueryService navigationQueryService)
    {
        List<Guid> uniqueParentKeys = [];
        foreach (Guid distinctMediaKey in entityKeys.Distinct())
        {
            if (navigationQueryService.TryGetParentKey(distinctMediaKey, out Guid? parentKey) is false)
            {
                continue;
            }

            // If there is a start node, the media must have a parent.
            if (parentKey is null)
            {
                return false;
            }

            uniqueParentKeys.Add(parentKey.Value);
        }

        IEnumerable<Guid> parentKeysNotInStartNode = uniqueParentKeys.Where(x => x != startNode);
        foreach (Guid parentKey in parentKeysNotInStartNode)
        {
            if (navigationQueryService.TryGetAncestorsKeys(parentKey, out IEnumerable<Guid> foundAncestorKeys) is false)
            {
                // We couldn't find the parent node, so we fail.
                return false;
            }

            Guid[] ancestorKeys = foundAncestorKeys.ToArray();
            if (ancestorKeys.Length == 0 || ancestorKeys.Contains(startNode) is false)
            {
                return false;
            }
        }

        return true;
    }
}
