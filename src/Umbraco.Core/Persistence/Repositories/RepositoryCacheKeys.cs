using System.Collections.Concurrent;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Provides cache keys for repositories.
/// </summary>
public static class RepositoryCacheKeys
{
    /// <summary>
    /// A thread-safe cache for the keys so we don't keep allocating strings.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, string> _keys = new();

    private static readonly ConcurrentDictionary<Type, string> _guidKeys = new();

    /// <summary>
    /// Gets the repository cache key for the provided type.
    /// </summary>
    /// <typeparam name="T">The entity type to get the cache key for.</typeparam>
    /// <returns>A cache key string in the format "uRepo_{TypeName}_".</returns>
    public static string GetKey<T>()
        => _keys.GetOrAdd(typeof(T), static type => "uRepo_" + type.Name + "_");

    /// <summary>
    /// Gets the GUID-specific repository cache key for the provided type.
    /// Uses a distinct prefix so that GUID-keyed entries don't interfere with
    /// the int-keyed repository's prefix-based search and count validation.
    /// </summary>
    /// <typeparam name="T">The entity type to get the cache key for.</typeparam>
    /// <returns>A cache key string in the format "uRepoGuid_{TypeName}_".</returns>
    public static string GetGuidKey<T>()
        => _guidKeys.GetOrAdd(typeof(T), static type => "uRepoGuid_" + type.Name + "_");

    /// <summary>
    /// Gets the repository cache key for the provided type and Id.
    /// </summary>
    /// <typeparam name="T">The entity type to get the cache key for.</typeparam>
    /// <typeparam name="TId">The type of the entity identifier.</typeparam>
    /// <param name="id">The entity identifier.</param>
    /// <returns>A cache key string in the format "uRepo_{TypeName}_{Id}", or an empty string if the id is the default value.</returns>
    public static string GetKey<T, TId>(TId? id)
    {
        if (EqualityComparer<TId?>.Default.Equals(id, default))
        {
            return string.Empty;
        }

        if (typeof(TId).IsValueType)
        {
            return GetKey<T>() + id;
        }

        return GetKey<T>() + id?.ToString()?.ToUpperInvariant();
    }

    /// <summary>
    /// Gets the GUID-specific repository cache key for the provided type and GUID.
    /// </summary>
    /// <typeparam name="T">The entity type to get the cache key for.</typeparam>
    /// <param name="id">The entity GUID identifier.</param>
    /// <returns>A cache key string in the format "uRepoGuid_{TypeName}_{Guid}", or an empty string if the id is <see cref="Guid.Empty"/>.</returns>
    public static string GetGuidKey<T>(Guid id)
    {
        if (id == Guid.Empty)
        {
            return string.Empty;
        }

        return GetGuidKey<T>() + id;
    }
}
