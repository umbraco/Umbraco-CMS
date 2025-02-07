using System.Runtime.InteropServices;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Provides cache keys for repositories.
/// </summary>
public static class RepositoryCacheKeys
{
    // used to cache keys so we don't keep allocating strings
    private static readonly Dictionary<Type, string> Keys = new();

    public static string GetKey<T>()
    {
        Type type = typeof(T);

        // look up the existing value or get a reference to the newly created default value
        ref string? key = ref CollectionsMarshal.GetValueRefOrAddDefault(Keys, type, out _);

        // as we have the reference, we can just assign it if null, without the expensive write back to the dictionary
        return key ??= "uRepo_" + type.Name + "_";
    }

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
