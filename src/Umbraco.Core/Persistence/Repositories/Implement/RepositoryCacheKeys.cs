using System;
using System.Collections.Generic;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Provides cache keys for repositories.
    /// </summary>
    internal static class RepositoryCacheKeys
    {
        private static readonly Dictionary<Type, string> s_keys = new Dictionary<Type, string>();

        public static string GetKey<T>()
        {
            var type = typeof(T);
            return s_keys.TryGetValue(type, out var key) ? key : (s_keys[type] = "uRepo_" + type.Name + "_");
        }

        [Obsolete("Use GetKey<T, TId> instead")]
        public static string GetKey<T>(object id) => GetKey<T>() + id.ToString().ToUpperInvariant();

        public static string GetKey<T, TId>(TId id)
        {
            if (EqualityComparer<TId>.Default.Equals(id, default))
            {
                return string.Empty;
            }

            if (typeof(TId).IsValueType)
            {
                return GetKey<T>() + id;
            }

            return GetKey<T>() + id.ToString().ToUpperInvariant();
        }
    }
}
