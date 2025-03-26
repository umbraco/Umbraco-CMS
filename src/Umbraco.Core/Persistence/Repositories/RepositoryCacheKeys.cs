using System.Runtime.InteropServices;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Provides cache keys for repositories.
/// </summary>
public static class RepositoryCacheKeys
{
    /// <summary>
    /// A cache for the keys we don't keep allocating strings.
    /// </summary>
    private static readonly Dictionary<Type, string> Keys = new();

    /// <summary>
    /// Gets the repository cache key for the provided type.
    /// </summary>
    public static string GetKey<T>()
    {
        Type type = typeof(T);

        // The following code is a micro-optimization to avoid an unnecessary lookup in the Keys dictionary, when writing the newly created key.
        // Previously, the code was:
        //   return Keys.TryGetValue(type, out var key)
        //     ? key
        //     : Keys[type] = "uRepo_" + type.Name + "_";

        // Look up the existing value or get a reference to the newly created default value.
        ref string? key = ref CollectionsMarshal.GetValueRefOrAddDefault(Keys, type, out _);

        // As we have the reference, we can just assign it if null, without the expensive write back to the dictionary.
        return key ??= "uRepo_" + type.Name + "_";
    }

    /// <summary>
    /// Gets the repository cache key for the provided type and Id.
    /// </summary>
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
