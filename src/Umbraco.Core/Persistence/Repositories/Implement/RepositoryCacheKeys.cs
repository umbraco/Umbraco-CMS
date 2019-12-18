using System;
using System.Collections.Generic;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Provides cache keys for repositories.
    /// </summary>
    internal static class RepositoryCacheKeys
    {
        private static readonly Dictionary<Type, string> Keys = new Dictionary<Type, string>();

        public static string GetKey<T>()
        {
            var type = typeof(T);
            return Keys.TryGetValue(type, out var key) ? key : (Keys[type] = "uRepo_" + type.Name + "_");
        }

        public static string GetKey<T>(object id) => GetKey<T>() + id;
    }
}
