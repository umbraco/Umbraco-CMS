using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
/// Converts an <see cref="UdiRange" /> to or from JSON.
/// </summary>
public sealed class JsonUdiRangeConverter : JsonConverter<UdiRange>
{
    /// <inheritdoc />
    public override UdiRange? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetString() is string value
        ? UdiRange.Parse(value)
        : null;

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, UdiRange value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
