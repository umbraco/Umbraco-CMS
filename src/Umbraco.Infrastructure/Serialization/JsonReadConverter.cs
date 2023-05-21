using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
///     Provides a base class for custom <see cref="JsonConverter" /> implementations.
/// </summary>
/// <typeparam name="T">The type of the converted object.</typeparam>
public abstract class JsonReadConverter<T> : JsonConverter
{
    /// <inheritdoc />
    public override bool CanConvert(Type objectType) => typeof(T).IsAssignableFrom(objectType);

    /// <summary>
    ///     Create an instance of objectType, based properties in the JSON object
    /// </summary>
    /// <param name="objectType">type of object expected</param>
    /// <param name="path">The path of the current json token.</param>
    /// <param name="jObject">contents of JSON object that will be deserialized</param>
    /// <returns></returns>
    protected abstract T Create(Type objectType, string path, JObject jObject);

    /// <inheritdoc />
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        // Load JObject from stream
        var jObject = JObject.Load(reader);

        // Create target object based on JObject
        T target = Create(objectType, reader.Path, jObject);

        Deserialize(jObject, target, serializer);

        return target;
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
        throw new NotSupportedException("JsonReadConverter instances do not support writing.");

    protected virtual void Deserialize(JObject jobject, T target, JsonSerializer serializer) =>

        // Populate the object properties
        serializer.Populate(jobject.CreateReader(), target!);
}
