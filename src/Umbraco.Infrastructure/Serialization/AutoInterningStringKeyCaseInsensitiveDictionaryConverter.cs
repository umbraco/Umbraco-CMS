using System.Collections;
using Newtonsoft.Json;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
///     When applied to a dictionary with a string key, will ensure the deserialized string keys are interned
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <remarks>
///     borrowed from https://stackoverflow.com/a/36116462/694494
/// </remarks>
public class AutoInterningStringKeyCaseInsensitiveDictionaryConverter<TValue> : CaseInsensitiveDictionaryConverter<TValue>
{
    public AutoInterningStringKeyCaseInsensitiveDictionaryConverter()
    {
    }

    public AutoInterningStringKeyCaseInsensitiveDictionaryConverter(StringComparer comparer)
        : base(comparer)
    {
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartObject)
        {
            IDictionary dictionary = Create(objectType);
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        var key = string.Intern(reader.Value!.ToString()!);

                        if (!reader.Read())
                        {
                            throw new Exception("Unexpected end when reading object.");
                        }

                        TValue? v = serializer.Deserialize<TValue>(reader);
                        dictionary[key] = v;
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        return dictionary;
                }
            }
        }

        return null;
    }
}
