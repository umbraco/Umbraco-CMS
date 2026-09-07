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
    /// Adds known ID/key pairs to the cache, so subsequent lookups do not need to hit the database.
    /// </summary>
    /// <param name="pairs">The ID/key pairs to cache. Must be a materialized collection.</param>
    /// <param name="umbracoObjectType">The type of the Umbraco objects the pairs belong to.</param>
    /// <remarks>
    /// Only supply pairs read from persisted entities: the mapping is assumed to be unique and permanent.
    /// </remarks>
    // TODO (V19): Remove the default implementation.
    void PopulateCache(IReadOnlyCollection<(int Id, Guid Key)> pairs, UmbracoObjectTypes umbracoObjectType)
    {
    }

    /// <summary>
    /// Adds a known ID/key pair to the cache, so subsequent lookups do not need to hit the database.
    /// </summary>
    /// <param name="id">The integer identifier of the entity.</param>
    /// <param name="key">The unique GUID key of the entity.</param>
    /// <param name="umbracoObjectType">The type of the Umbraco object.</param>
    void PopulateCache(int id, Guid key, UmbracoObjectTypes umbracoObjectType)
        => PopulateCache([(id, key)], umbracoObjectType);

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
