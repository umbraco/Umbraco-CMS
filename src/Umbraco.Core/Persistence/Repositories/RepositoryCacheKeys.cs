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

    /// <summary>
    /// Gets the repository cache key for the provided type.
    /// </summary>
    /// <typeparam name="T">The entity type to get the cache key for.</typeparam>
    /// <returns>A cache key string in the format "uRepo_{TypeName}_".</returns>
    public static string GetKey<T>()
        => _keys.GetOrAdd(typeof(T), static type => "uRepo_" + type.Name + "_");

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
}
