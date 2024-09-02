using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface INavigationRepository
{
    /// <summary>
    ///     Retrieves a collection of content nodes as navigation models based on the object type key.
    /// </summary>
    /// <param name="objectTypeKey">The unique identifier for the object type.</param>
    /// <returns>A collection of navigation models.</returns>
    IEnumerable<INavigationModel> GetContentNodesByObjectType(Guid objectTypeKey);

    /// <summary>
    ///     Retrieves a collection of trashed content nodes as navigation models based on the object type key.
    /// </summary>
    /// <param name="objectTypeKey">The unique identifier for the object type.</param>
    /// <returns>A collection of navigation models.</returns>
    IEnumerable<INavigationModel> GetTrashedContentNodesByObjectType(Guid objectTypeKey);
}
