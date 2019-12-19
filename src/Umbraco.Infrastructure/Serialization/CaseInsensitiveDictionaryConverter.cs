using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;

namespace Umbraco.Core.Serialization
{
    /// <summary>
    /// Marks dictionaries so they are deserialized as case-insensitive.
    /// </summary>
    /// <example>
    /// [JsonConverter(typeof(CaseInsensitiveDictionaryConverter{PropertyData[]}))]
    /// public Dictionary{string, PropertyData[]} PropertyData {{ get; set; }}
    /// </example>
    public class CaseInsensitiveDictionaryConverter<T> : CustomCreationConverter<IDictionary>
    {
        public override bool CanWrite => false;

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) => typeof(IDictionary<string,T>).IsAssignableFrom(objectType);

        public override IDictionary Create(Type objectType) => new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
    }
}
