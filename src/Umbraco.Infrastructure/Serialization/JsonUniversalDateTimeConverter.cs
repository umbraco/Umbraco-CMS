using System.Text.Json;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
/// Converts a DateTime value to or from JSON, always converting the value to Coordinated Universal Time (UTC) when reading.
/// </summary>
/// <remarks>
/// In order to match the existing behaviour, and that of MessagePack, we need to ensure that DateTimes are always read as UTC.
/// This is not the case by default for System.Text.Json, see: https://github.com/dotnet/runtime/issues/1566.
/// </remarks>
public sealed class JsonUniversalDateTimeConverter : ReadOnlyJsonConverter<DateTime>
{
    /// <inheritdoc />
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetDateTime().ToUniversalTime();
}
