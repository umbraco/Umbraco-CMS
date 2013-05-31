using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using Umbraco.Core;

namespace Umbraco.Web
{
    
    public static class FormDataCollectionExtensions
    {
        /// <summary>
        /// Converts a dictionary object to a query string representation such as:
        /// firstname=shannon&lastname=deminick
        /// </summary>
        /// <param name="items"></param>
        /// <param name="keysToIgnore">Any keys found in this collection will be removed from the output</param>
        /// <returns></returns>
        public static string ToQueryString(this FormDataCollection items, params string[] keysToIgnore)
        {
            if (items == null) return "";
            if (items.Any() == false) return "";

            var builder = new StringBuilder();
            foreach (var i in items.Where(i => keysToIgnore.InvariantContains(i.Key) == false))
            {
                builder.Append(string.Format("{0}={1}&", i.Key, i.Value));
            }
            return builder.ToString().TrimEnd('&');
        }

        /// <summary>
        /// Converts the FormCollection to a dictionary
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(this FormDataCollection items)
        {
            return items.ToDictionary(x => x.Key, x => (object)x.Value);
        }

        /// <summary>
        /// Returns the value of a mandatory item in the FormCollection
        /// </summary>
        /// <param name="items"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetRequiredString(this FormDataCollection items, string key)
        {
            if (items.HasKey(key) == false)
                throw new ArgumentNullException("The " + key + " query string parameter was not found but is required");
            return items.Single(x => x.Key.InvariantEquals(key)).Value;
        }

        /// <summary>
        /// Checks if the collection contains the key
        /// </summary>
        /// <param name="items"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool HasKey(this FormDataCollection items, string key)
        {
            return items.Any(x => x.Key.InvariantEquals(key));
        }

        /// <summary>
        /// Returns the object based in the collection based on it's key. This does this with a conversion so if it doesn't convert a null object is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetValue<T>(this FormDataCollection items, string key)
        {
            var val = items.Get(key);
            if (string.IsNullOrEmpty(val)) return default(T);

            var converted = val.TryConvertTo<T>();
            return converted.Success 
                ? converted.Result 
                : default(T);
        }
    }
}
