using MessagePack;
using MessagePack.Formatters;

namespace Umbraco.Cms.Infrastructure.HybridCache.Serialization;

/// <summary>
/// A MessagePack formatter (deserializer) for a string key dictionary that uses <see cref="StringComparer.OrdinalIgnoreCase" /> for the key string comparison and interns the string.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public sealed class MessagePackDictionaryStringInternIgnoreCaseFormatter<TValue> : DictionaryFormatterBase<string, TValue, Dictionary<string, TValue>, Dictionary<string, TValue>.Enumerator, Dictionary<string, TValue>>
{
    /// <inheritdoc />
    protected override void Add(Dictionary<string, TValue> collection, int index, string key, TValue value, MessagePackSerializerOptions options)
        => collection.Add(string.Intern(key), value);

    /// <inheritdoc />
    protected override Dictionary<string, TValue> Complete(Dictionary<string, TValue> intermediateCollection)
        => intermediateCollection;

    /// <inheritdoc />
    protected override Dictionary<string, TValue>.Enumerator GetSourceEnumerator(Dictionary<string, TValue> source)
        => source.GetEnumerator();

    /// <inheritdoc />
    protected override Dictionary<string, TValue> Create(int count, MessagePackSerializerOptions options)
        => new(count, StringComparer.OrdinalIgnoreCase);
}
