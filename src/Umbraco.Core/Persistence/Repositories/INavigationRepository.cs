using Umbraco.Cms.Core.Models.Navigation;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface INavigationRepository
{
    /// <summary>
    ///     Retrieves a dictionary of content nodes based on the object type key.
    /// </summary>
    /// <param name="objectTypeKey">The unique identifier for the object type.</param>
    /// <returns>A dictionary of navigation nodes where the key is the unique identifier of the node.</returns>
    public Dictionary<Guid, NavigationNode> GetContentNodesByObjectType(Guid objectTypeKey);

    /// <summary>
    ///     Retrieves a dictionary of trashed content nodes based on the object type key.
    /// </summary>
    /// <param name="objectTypeKey">The unique identifier for the object type.</param>
    /// <returns>A dictionary of navigation nodes where the key is the unique identifier of the node.</returns>
    public Dictionary<Guid, NavigationNode> GetTrashedContentNodesByObjectType(Guid objectTypeKey);
}
