using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <inheritdoc />
public sealed class SystemTextConfigurationEditorJsonSerializer : SystemTextJsonSerializerBase, IConfigurationEditorJsonSerializer
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextConfigurationEditorJsonSerializer" /> class.
    /// </summary>
    public SystemTextConfigurationEditorJsonSerializer()
        => _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

            // In some cases, configs aren't camel cased in the DB, so we have to resort to case insensitive
            // property name resolving when creating configuration objects (deserializing DB configs).
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters =
            {
                new JsonStringEnumConverter(),
                new JsonObjectConverter(),
                new JsonUdiConverter(),
                new JsonUdiRangeConverter(),
                new JsonBooleanConverter(),
            },

            // Properties of data type configuration objects are annotated with [ConfigurationField] attributes
            // that provide the serialized name. Rather than decorating them as well with [JsonPropertyName] attributes
            // when they differ from the property name, we'll define a custom type info resolver to use the
            // existing attribute.
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
                .WithAddedModifier(UseAttributeConfiguredPropertyNames()),
        };

    protected override JsonSerializerOptions JsonSerializerOptions => _jsonSerializerOptions;

    /// <summary>
    /// A custom action used to provide property names when they are overridden by
    /// <see cref="ConfigurationField"/> attributes.
    /// </summary>
    /// <remarks>
    /// Hat-tip: https://stackoverflow.com/a/78063664
    /// </remarks>
    private static Action<JsonTypeInfo> UseAttributeConfiguredPropertyNames() => typeInfo =>
    {
        if (typeInfo.Kind is not JsonTypeInfoKind.Object)
        {
            return;
        }

        foreach (JsonPropertyInfo property in typeInfo.Properties)
        {
            if (property.AttributeProvider?.GetCustomAttributes(typeof(ConfigurationFieldAttribute), true) is { } attributes)
            {
                foreach (ConfigurationFieldAttribute attribute in attributes)
                {
                    property.Name = attribute.Key;
                }
            }
        }
    };
}
