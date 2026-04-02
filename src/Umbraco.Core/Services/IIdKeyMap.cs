using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides a mapping between integer IDs, GUIDs (keys), and UDIs for Umbraco entities.
/// </summary>
/// <remarks>
/// This service maintains a cache of ID/key mappings for efficient lookups.
/// </remarks>
public interface IIdKeyMap
{
    /// <summary>
    /// Gets the integer ID for a given GUID key and object type.
    /// </summary>
    /// <param name="key">The unique GUID key of the entity.</param>
    /// <param name="umbracoObjectType">The type of the Umbraco object.</param>
    /// <returns>An attempt containing the integer ID if found.</returns>
    Attempt<int> GetIdForKey(Guid key, UmbracoObjectTypes umbracoObjectType);

    /// <summary>
    /// Gets the integer ID for a given UDI.
    /// </summary>
    /// <param name="udi">The Umbraco Document Identifier.</param>
    /// <returns>An attempt containing the integer ID if found.</returns>
    Attempt<int> GetIdForUdi(Udi udi);

    /// <summary>
    /// Gets the UDI for a given integer ID and object type.
    /// </summary>
    /// <param name="id">The integer identifier of the entity.</param>
    /// <param name="umbracoObjectType">The type of the Umbraco object.</param>
    /// <returns>An attempt containing the UDI if found.</returns>
    Attempt<Udi?> GetUdiForId(int id, UmbracoObjectTypes umbracoObjectType);

    /// <summary>
    /// Gets the GUID key for a given integer ID and object type.
    /// </summary>
    /// <param name="id">The integer identifier of the entity.</param>
    /// <param name="umbracoObjectType">The type of the Umbraco object.</param>
    /// <returns>An attempt containing the GUID key if found.</returns>
    Attempt<Guid> GetKeyForId(int id, UmbracoObjectTypes umbracoObjectType);

    /// <summary>
    /// Clears the entire ID/key mapping cache.
    /// </summary>
    void ClearCache();

    /// <summary>
    /// Clears the cache entry for a specific integer ID.
    /// </summary>
    /// <param name="id">The integer identifier to remove from the cache.</param>
    void ClearCache(int id);

    /// <summary>
    /// Clears the cache entry for a specific GUID key.
    /// </summary>
    /// <param name="key">The GUID key to remove from the cache.</param>
    void ClearCache(Guid key);
}
