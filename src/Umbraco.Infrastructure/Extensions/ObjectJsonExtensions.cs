using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Umbraco.Cms.Core;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Provides object extension methods.
    /// </summary>
    public static class ObjectJsonExtensions
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, object>> ToObjectTypes = new ConcurrentDictionary<Type, Dictionary<string, object>>();

        /// <summary>
        /// Converts an object's properties into a dictionary.
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <param name="namer">A property namer function.</param>
        /// <returns>A dictionary containing each properties.</returns>
        public static Dictionary<string, object> ToObjectDictionary<T>(T obj, Func<PropertyInfo, string>? namer = null)
        {
            if (obj == null) return new Dictionary<string, object>();

            string DefaultNamer(PropertyInfo property)
            {
                var jsonProperty = property.GetCustomAttribute<JsonPropertyAttribute>();
                return jsonProperty?.PropertyName ?? property.Name;
            }

            var t = obj.GetType();

            if (namer == null) namer = DefaultNamer;

            if (!ToObjectTypes.TryGetValue(t, out var properties))
            {
                properties = new Dictionary<string, object>();

                foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
                    properties[namer(p)] = ReflectionUtilities.EmitPropertyGetter<T, object>(p);

                ToObjectTypes[t] = properties;
            }

            return properties.ToDictionary(x => x.Key, x => ((Func<T, object>) x.Value)(obj));
        }

    }
}
