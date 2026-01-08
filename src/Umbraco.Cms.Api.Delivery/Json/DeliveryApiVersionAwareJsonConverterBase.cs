using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Json;

public abstract class DeliveryApiVersionAwareJsonConverterBase<T> : JsonConverter<T>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonConverter<T> _defaultConverter = (JsonConverter<T>)JsonSerializerOptions.Default.GetConverter(typeof(T));

    public DeliveryApiVersionAwareJsonConverterBase(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc />
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => _defaultConverter.Read(ref reader, typeToConvert, options);

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        Type type = typeof(T);
        var apiVersion = GetApiVersion();

        // Get the properties in the specified order
        PropertyInfo[] properties = type.GetProperties().OrderBy(GetPropertyOrder).ToArray();

        writer.WriteStartObject();

        foreach (PropertyInfo property in properties)
        {
            // Filter out properties based on the API version
            var include = apiVersion is null || ShouldIncludeProperty(property, apiVersion.Value);

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

    private int? GetApiVersion()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        ApiVersion? apiVersion = httpContext?.GetRequestedApiVersion();

        return apiVersion?.MajorVersion;
    }

    private int GetPropertyOrder(PropertyInfo prop)
    {
        var attribute = prop.GetCustomAttribute<JsonPropertyOrderAttribute>();
        return attribute?.Order ?? 0;
    }

    /// <summary>
    ///     Determines whether a property should be included based on version bounds.
    /// </summary>
    /// <param name="propertyInfo">The property info.</param>
    /// <param name="version">An integer representing an API version.</param>
    /// <returns><c>true</c> if the property should be included; otherwise, <c>false</c>.</returns>
    private bool ShouldIncludeProperty(PropertyInfo propertyInfo, int version)
    {
        var attribute = propertyInfo
            .GetCustomAttributes(typeof(IncludeInApiVersionAttribute), false)
            .FirstOrDefault();

        if (attribute is not IncludeInApiVersionAttribute apiVersionAttribute)
        {
            return true; // No attribute means include the property
        }

        // Check if the version is within the specified bounds
        var isWithinMinVersion = apiVersionAttribute.MinVersion.HasValue is false || version >= apiVersionAttribute.MinVersion.Value;
        var isWithinMaxVersion = apiVersionAttribute.MaxVersion.HasValue is false || version <= apiVersionAttribute.MaxVersion.Value;

        return isWithinMinVersion && isWithinMaxVersion;
    }
}
