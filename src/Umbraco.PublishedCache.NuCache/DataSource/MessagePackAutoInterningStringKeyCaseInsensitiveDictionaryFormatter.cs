using MessagePack;
using MessagePack.Formatters;

namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

/// <summary>
///     A MessagePack formatter (deserializer) for a string key dictionary that uses
///     <see cref="StringComparer.OrdinalIgnoreCase" /> for the key string comparison.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <seealso
///     cref="DictionaryFormatterBase&lt;string, TValue, Dictionary&lt;string, TValue&gt;, Dictionary&lt;string, TValue&gt;.Enumerator, Dictionary&lt;string, TValue&gt;&gt;" />
public sealed class MessagePackAutoInterningStringKeyCaseInsensitiveDictionaryFormatter<TValue> :
    DictionaryFormatterBase<string, TValue, Dictionary<string, TValue>, Dictionary<string, TValue>.Enumerator,
        Dictionary<string, TValue>>
{
    protected override void Add(Dictionary<string, TValue> collection, int index, string key, TValue value,
        MessagePackSerializerOptions options)
    {
        string.Intern(key);
        collection.Add(key, value);
    }

    protected override Dictionary<string, TValue> Complete(Dictionary<string, TValue> intermediateCollection) =>
        intermediateCollection;

    protected override Dictionary<string, TValue>.Enumerator GetSourceEnumerator(Dictionary<string, TValue> source) =>
        source.GetEnumerator();

    protected override Dictionary<string, TValue> Create(int count, MessagePackSerializerOptions options) =>
        new(count, StringComparer.OrdinalIgnoreCase);
}
