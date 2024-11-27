using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <inheritdoc />
public sealed class SystemTextWebhookJsonSerializer : SystemTextJsonSerializerBase, IWebhookJsonSerializer
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextWebhookJsonSerializer" /> class.
    /// </summary>
    public SystemTextWebhookJsonSerializer()
        => _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(),
                new JsonUdiConverter(),
                new JsonUdiRangeConverter(),
                new JsonObjectConverter(), // Required for block editor values
                new JsonBlockValueConverter()
            },
            TypeInfoResolver = new WebhookJsonTypeResolver(),
        };

    protected override JsonSerializerOptions JsonSerializerOptions => _jsonSerializerOptions;
}
