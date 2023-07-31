using System.Collections;
using System.ComponentModel;

namespace Umbraco.Search.Extensions;

public static class ObjectExtensions
{
    /// <summary>
    /// Turns object into dictionary
    /// </summary>
    /// <param name="o"></param>
    /// <param name="ignoreProperties">Properties to ignore</param>
    /// <returns></returns>
    public static IDictionary<string, object> ConvertObjectToDictionary(object o, params string[] ignoreProperties)
    {
        if (o != null)
        {
            if (o is IDictionary)
                throw new InvalidOperationException($"The input object is already of type {typeof(IDictionary)}");

            var props = TypeDescriptor.GetProperties(o);
            var d = new Dictionary<string, object>();
            foreach (var prop in props.Cast<PropertyDescriptor>().Where(x => !ignoreProperties.Contains(x.Name)))
            {
                var val = prop.GetValue(o);
                if (val != null)
                {
                    d.Add(prop.Name, val);
                }
            }
            return d;
        }
        return new Dictionary<string, object>();
    }
}
