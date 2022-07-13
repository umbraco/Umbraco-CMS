using System.Collections;
using Newtonsoft.Json.Converters;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
///     Marks dictionaries so they are deserialized as case-insensitive.
/// </summary>
/// <example>
///     [JsonConverter(typeof(CaseInsensitiveDictionaryConverter{PropertyData[]}))]
///     public Dictionary{string, PropertyData[]} PropertyData {{ get; set; }}
/// </example>
public class CaseInsensitiveDictionaryConverter<T> : CustomCreationConverter<IDictionary>
{
    private readonly StringComparer _comparer;

    public CaseInsensitiveDictionaryConverter()
        : this(StringComparer.OrdinalIgnoreCase)
    {
    }

    public CaseInsensitiveDictionaryConverter(StringComparer comparer) =>
        _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

    public override bool CanWrite => false;

    public override bool CanRead => true;

    public override bool CanConvert(Type objectType) => typeof(IDictionary<string, T>).IsAssignableFrom(objectType);

    public override IDictionary Create(Type objectType) => new Dictionary<string, T>(_comparer);
}
