using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <inheritdoc />
public sealed class SystemTextJsonSerializer : SystemTextJsonSerializerBase
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextConfigurationEditorJsonSerializer" /> class.
    /// </summary>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public SystemTextJsonSerializer()
        : this(
              StaticServiceProvider.Instance.GetRequiredService<IJsonSerializerEncoderFactory>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonSerializer" /> class.
    /// </summary>
    public SystemTextJsonSerializer(IJsonSerializerEncoderFactory jsonSerializerEncoderFactory)
        : base(jsonSerializerEncoderFactory)
        => _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

            Encoder = jsonSerializerEncoderFactory.CreateEncoder<SystemTextJsonSerializer>(),

            Converters =
            {
                new JsonStringEnumConverter(),
                new JsonUdiConverter(),
                new JsonUdiRangeConverter(),
                new JsonObjectConverter(), // Required for block editor values
                new JsonBlockValueConverter()
            }
        };

    /// <inheritdoc/>
    protected override JsonSerializerOptions JsonSerializerOptions => _jsonSerializerOptions;
}
