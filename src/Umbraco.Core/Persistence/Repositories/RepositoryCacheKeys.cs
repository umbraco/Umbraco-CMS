namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Provides cache keys for repositories.
/// </summary>
public static class RepositoryCacheKeys
{
    // used to cache keys so we don't keep allocating strings
    private static readonly Dictionary<Type, string> s_keys = new();

    public static string GetKey<T>()
    {
        Type type = typeof(T);
        return s_keys.TryGetValue(type, out var key) ? key : s_keys[type] = "uRepo_" + type.Name + "_";
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
