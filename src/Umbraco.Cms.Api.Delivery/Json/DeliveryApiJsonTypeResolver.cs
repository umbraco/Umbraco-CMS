using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Json;

/// <summary>
///     Custom resolver for JSON type information in the Delivery API serialization.
///     Handles both polymorphism and filters out properties based on API versioning rules.
/// </summary>
/// <remarks>
///     see https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0.
/// </remarks>
public class DeliveryApiJsonTypeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        Type[] derivedTypes = GetDerivedTypes(jsonTypeInfo);
        if (derivedTypes.Length > 0)
        {
            ConfigureJsonPolymorphismOptions(jsonTypeInfo, derivedTypes);
        }

        // Filters out properties based on API versioning rules
        var tempVersion = 3;
        FilterOutVersionedProperties(jsonTypeInfo, tempVersion);

        return jsonTypeInfo;
    }

    protected virtual Type[] GetDerivedTypes(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Type == typeof(IApiContent))
        {
            return new[] { typeof(ApiContent) };
        }

        if (jsonTypeInfo.Type == typeof(IApiContentResponse))
        {
            return new[] { typeof(ApiContentResponse) };
        }

        if (jsonTypeInfo.Type == typeof(IRichTextElement))
        {
            return new[] { typeof(RichTextRootElement), typeof(RichTextGenericElement), typeof(RichTextTextElement) };
        }

        return Array.Empty<Type>();
    }

    protected void ConfigureJsonPolymorphismOptions(JsonTypeInfo jsonTypeInfo, params Type[] derivedTypes)
    {
        jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
        {
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        };

        foreach (Type derivedType in derivedTypes)
        {
            jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derivedType));
        }
    }

    protected void FilterOutVersionedProperties(JsonTypeInfo jsonTypeInfo, int apiVersion)
    {
        var propertiesToRemove = jsonTypeInfo
            .Properties
            .Where(prop => ShouldIncludeProperty(prop, apiVersion) is false)
            .ToArray();

        foreach (JsonPropertyInfo prop in propertiesToRemove)
        {
            jsonTypeInfo.Properties.Remove(prop);
        }
    }

    private bool ShouldIncludeProperty(JsonPropertyInfo prop, int version)
    {
        var attribute = prop
            .AttributeProvider?
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
