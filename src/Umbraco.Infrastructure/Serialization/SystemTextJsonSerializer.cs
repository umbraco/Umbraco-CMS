using System.Text.Json;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <inheritdoc />
public sealed class SystemTextJsonSerializer : SystemTextJsonSerializerBase
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonSerializer" /> class.
    /// </summary>
    public SystemTextJsonSerializer()
        => _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(),
                new JsonUdiConverter(),
                new JsonUdiRangeConverter(),
                new JsonObjectConverter(), // Required for block editor values
                new JsonBlockValueConverter()
            }
        };

    protected override JsonSerializerOptions JsonSerializerOptions => _jsonSerializerOptions;
}
