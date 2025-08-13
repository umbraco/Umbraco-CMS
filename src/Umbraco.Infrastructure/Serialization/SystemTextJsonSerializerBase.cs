using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Serialization;

public abstract class SystemTextJsonSerializerBase : IJsonSerializer
{
    private readonly IJsonSerializerEncoderFactory _jsonSerializerEncoderFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonSerializerBase" /> class.
    /// </summary>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    protected SystemTextJsonSerializerBase()
        : this(
              StaticServiceProvider.Instance.GetRequiredService<IJsonSerializerEncoderFactory>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonSerializerBase"/> class.
    /// </summary>
    /// <param name="jsonSerializerEncoderFactory">The <see cref="IJsonSerializerEncoderFactory"/> for creating the <see cref="JavaScriptEncoder"/>.</param>
    protected SystemTextJsonSerializerBase(IJsonSerializerEncoderFactory jsonSerializerEncoderFactory)
        => _jsonSerializerEncoderFactory = jsonSerializerEncoderFactory;

    /// <summary>
    /// Gets the <see cref="System.Text.Json.JsonSerializerOptions"/>.
    /// </summary>
    protected abstract JsonSerializerOptions JsonSerializerOptions { get; }

    /// <inheritdoc />
    public string Serialize(object? input) => JsonSerializer.Serialize(input, JsonSerializerOptions);

    /// <inheritdoc />
    public T? Deserialize<T>(string input) => JsonSerializer.Deserialize<T>(input, JsonSerializerOptions);

    /// <inheritdoc />
    public bool TryDeserialize<T>(object input, [NotNullWhen(true)] out T? value)
        where T : class
    {
        var jsonString = input switch
        {
            JsonNode jsonNodeValue => jsonNodeValue.ToJsonString(),
            string stringValue when stringValue.DetectIsJson() => stringValue,
            _ => null
        };

        value = jsonString.IsNullOrWhiteSpace()
            ? null
            : Deserialize<T>(jsonString);
        return value != null;
    }
}
