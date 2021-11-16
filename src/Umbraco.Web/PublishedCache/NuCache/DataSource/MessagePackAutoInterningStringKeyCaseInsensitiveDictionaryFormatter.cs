using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// A messagepack formatter (deserializer) for a string key dictionary that uses OrdinalIgnoreCase for the key string comparison
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public sealed class MessagePackAutoInterningStringKeyCaseInsensitiveDictionaryFormatter<TValue> : DictionaryFormatterBase<string, TValue, Dictionary<string, TValue>, Dictionary<string, TValue>.Enumerator, Dictionary<string, TValue>>
    {
        protected override void Add(Dictionary<string, TValue> collection, int index, string key, TValue value, MessagePackSerializerOptions options)
        {
            string.Intern(key);
            collection.Add(key, value);
        }

        protected override Dictionary<string, TValue> Complete(Dictionary<string, TValue> intermediateCollection)
        {
            return intermediateCollection;
        }


        protected override Dictionary<string, TValue>.Enumerator GetSourceEnumerator(Dictionary<string, TValue> source)
        {
            return source.GetEnumerator();
        }

        protected override Dictionary<string, TValue> Create(int count, MessagePackSerializerOptions options)
        {
            return new Dictionary<string, TValue>(count, StringComparer.OrdinalIgnoreCase);
        }
    }
}



