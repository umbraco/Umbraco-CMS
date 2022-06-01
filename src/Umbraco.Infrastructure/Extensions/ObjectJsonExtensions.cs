using System.Collections.Concurrent;
using System.Reflection;
using Newtonsoft.Json;
using Umbraco.Cms.Core;

namespace Umbraco.Extensions;

/// <summary>
///     Provides object extension methods.
/// </summary>
public static class ObjectJsonExtensions
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, object>> _toObjectTypes = new();

    /// <summary>
    ///     Converts an object's properties into a dictionary.
    /// </summary>
    /// <param name="obj">The object to convert.</param>
    /// <param name="namer">A property namer function.</param>
    /// <returns>A dictionary containing each properties.</returns>
    public static Dictionary<string, object> ToObjectDictionary<T>(T obj, Func<PropertyInfo, string>? namer = null)
    {
        if (obj == null)
        {
            return new Dictionary<string, object>();
        }

        string DefaultNamer(PropertyInfo property)
        {
            JsonPropertyAttribute? jsonProperty = property.GetCustomAttribute<JsonPropertyAttribute>();
            return jsonProperty?.PropertyName ?? property.Name;
        }

        Type t = obj.GetType();

        if (namer == null)
        {
            namer = DefaultNamer;
        }

        if (!_toObjectTypes.TryGetValue(t, out Dictionary<string, object>? properties))
        {
            properties = new Dictionary<string, object>();

            foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance |
                                                       BindingFlags.FlattenHierarchy))
            {
                properties[namer(p)] = ReflectionUtilities.EmitPropertyGetter<T, object>(p);
            }

            _toObjectTypes[t] = properties;
        }

        return properties.ToDictionary(x => x.Key, x => ((Func<T, object>)x.Value)(obj));
    }
}
