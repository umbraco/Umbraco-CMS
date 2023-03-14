// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Specialized;
using Umbraco.Cms.Core;

namespace Umbraco.Extensions;

public static class NameValueCollectionExtensions
{
    public static IEnumerable<KeyValuePair<string?, string?>> AsEnumerable(this NameValueCollection nvc)
    {
        foreach (var key in nvc.AllKeys)
        {
            yield return new KeyValuePair<string?, string?>(key, nvc[key]);
        }
    }

    public static bool ContainsKey(this NameValueCollection collection, string key) =>
        collection.Keys.Cast<object>().Any(k => (string)k == key);

    public static T? GetValue<T>(this NameValueCollection collection, string key, T defaultIfNotFound)
    {
        if (collection.ContainsKey(key) == false)
        {
            return defaultIfNotFound;
        }

        var val = collection[key];
        if (val == null)
        {
            return defaultIfNotFound;
        }

        Attempt<T> result = val.TryConvertTo<T>();

        return result.Success ? result.Result : defaultIfNotFound;
    }
}
