using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for mapping between entity identifiers and unique keys.
/// </summary>
public interface IIdKeyMapRepository
{
    /// <summary>
    ///     Gets the integer identifier for a given unique key.
    /// </summary>
    /// <param name="key">The unique key of the entity.</param>
    /// <param name="umbracoObjectType">The type of the Umbraco object.</param>
    /// <returns>The integer identifier if found; otherwise, <c>null</c>.</returns>
    int? GetIdForKey(Guid key, UmbracoObjectTypes umbracoObjectType);

    /// <summary>
    ///     Gets the unique key for a given integer identifier.
    /// </summary>
    /// <param name="id">The integer identifier of the entity.</param>
    /// <param name="umbracoObjectType">The type of the Umbraco object.</param>
    /// <returns>The unique key if found; otherwise, <c>null</c>.</returns>
    Guid? GetIdForKey(int id, UmbracoObjectTypes umbracoObjectType);
}
