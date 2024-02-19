using System.Text.Json;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
/// In order to match the existing behaviour, and that os messagepack, we need to ensure that DateTimes are always read as UTC.
/// This is not the case by default for System.Text.Json, see: https://github.com/dotnet/runtime/issues/1566
/// </summary>
public class ForceUtcDateTimeConverter : JsonConverter<DateTime>
{
    private readonly JsonConverter<DateTime> _fallBackConverter = (JsonConverter<DateTime>)JsonSerializerOptions.Default.GetConverter(typeof(DateTime));

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetDateTime().ToUniversalTime();

    // The existing behaviour is fine for writing, it's only reading that's an issue.
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => _fallBackConverter.Write(writer, value, options);
}
