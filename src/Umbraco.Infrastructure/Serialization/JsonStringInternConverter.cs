using System.Text.Json;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
/// Converts a string to or from JSON, interning the string when reading.
/// </summary>
public sealed class JsonStringInternConverter : ReadOnlyJsonConverter<string>
{
    /// <inheritdoc />
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetString() is string value
        ? string.Intern(value)
        : null;
}
