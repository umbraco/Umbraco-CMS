using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Json;

public abstract class DeliveryApiVersionAwareJsonConverterBase<T> : JsonConverter<T>
{
    private readonly JsonConverter<T> _defaultConverter = (JsonConverter<T>)JsonSerializerOptions.Default.GetConverter(typeof(T));

    private const int ApiVersion = 4; // TODO: remove

    /// <inheritdoc />
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => _defaultConverter.Read(ref reader, typeToConvert, options);

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        Type type = typeof(T);
        PropertyInfo[] properties = type.GetProperties();

        writer.WriteStartObject();

        // Serialize properties in the specified order
        foreach (PropertyInfo property in properties.OrderBy(GetPropertyOrder))
        {
            // Filter out properties based on the API version
            var include = ShouldIncludeProperty(property, ApiVersion);

            if (include is false)
            {
                continue;
            }

            var propertyName = property.Name;
            writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName);
            JsonSerializer.Serialize(writer, property.GetValue(value), options);
        }

        writer.WriteEndObject();
    }

    private int GetPropertyOrder(PropertyInfo prop)
    {
        var attribute = prop.GetCustomAttribute<JsonPropertyOrderAttribute>();
        return attribute?.Order ?? 0;
    }

    private bool ShouldIncludeProperty(PropertyInfo prop, int version)
    {
        var attribute = prop
            .GetCustomAttributes(typeof(IncludeInApiVersionAttribute), false)
            .FirstOrDefault();

        if (attribute is not IncludeInApiVersionAttribute apiVersionAttribute)
        {
            return true; // No attribute means include the property
        }

        // Check if the version is in the specified versions
        if (apiVersionAttribute.Versions.Length > 0)
        {
            return apiVersionAttribute.Versions.Contains(version);
        }

        // Check if the version is within the specified bounds
        var isWithinMinVersion = apiVersionAttribute.MinVersion.HasValue is false || version >= apiVersionAttribute.MinVersion.Value;
        var isWithinMaxVersion = apiVersionAttribute.MaxVersion.HasValue is false || version <= apiVersionAttribute.MaxVersion.Value;

        return isWithinMinVersion && isWithinMaxVersion;
    }
}
