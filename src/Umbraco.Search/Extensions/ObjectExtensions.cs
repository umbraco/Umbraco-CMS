using System.Collections;
using System.ComponentModel;
using Umbraco.Extensions;

namespace Umbraco.Search.Extensions;

public static class ObjectExtensions
{
    /// <summary>
    /// Turns object into dictionary
    /// </summary>
    /// <param name="sourceObject"></param>
    /// <param name="ignoreProperties">Properties to ignore</param>
    /// <returns></returns>
    public static IDictionary<string, object> ConvertObjectToDictionary(
        object? sourceObject,
        params string[] ignoreProperties)
    {
        if (sourceObject == null)
        {
            return new Dictionary<string, object>();
        }

        if (sourceObject is IDictionary sourceDictionary)
        {
            return sourceDictionary.Keys.Cast<object>().ToDictionary(
                key => key.ToString()!,
                key => sourceDictionary[key]!);
        }

        var props = TypeDescriptor.GetProperties(sourceObject);
        var d = new Dictionary<string, object>();
        foreach (var prop in props.Cast<PropertyDescriptor>().Where(x => !ignoreProperties.Contains(x.Name)))
        {
            var val = prop.GetValue(sourceObject);
            if (val != null)
            {
                d.Add(prop.Name, val);
            }
        }

        return d;
    }
}
