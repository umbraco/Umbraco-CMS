using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
/// Converts an <see cref="Udi" /> to or from JSON.
/// </summary>
public sealed class JsonUdiConverter : JsonConverter<Udi>
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
        => typeof(Udi).IsAssignableFrom(typeToConvert);

    /// <inheritdoc />
    public override Udi? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.GetString() is string value && string.IsNullOrWhiteSpace(value) is false)
        {
            return UdiParser.Parse(value);
        }

        return null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Udi value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
