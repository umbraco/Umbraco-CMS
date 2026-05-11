using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Umbraco.Cms.Core.Models.DeliveryApi;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
/// Serves as the base class for resolving JSON types during content serialization and deserialization in Umbraco.
/// Implementations of this class provide logic to map JSON data to specific content types.
/// </summary>
public abstract class ContentJsonTypeResolverBase : DefaultJsonTypeInfoResolver
{
    /// <summary>
    /// Gets the <see cref="System.Text.Json.Serialization.JsonTypeInfo"/> for the specified <see cref="System.Type"/>,
    /// configuring polymorphic serialization options for derived types if applicable.
    /// </summary>
    /// <param name="type">The type to get the <see cref="System.Text.Json.Serialization.JsonTypeInfo"/> for.</param>
    /// <param name="options">The <see cref="System.Text.Json.JsonSerializerOptions"/> to use when getting the type info.</param>
    /// <returns>The <see cref="System.Text.Json.Serialization.JsonTypeInfo"/> for the specified type, with polymorphism options configured if derived types are present.</returns>
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        Type[] derivedTypes = GetDerivedTypes(jsonTypeInfo);
        if (derivedTypes.Length > 0)
        {
            ConfigureJsonPolymorphismOptions(jsonTypeInfo, derivedTypes);
        }

        return jsonTypeInfo;
    }

    /// <summary>
    /// Returns the concrete types that are derived from the type described by the specified <see cref="System.Text.Json.Serialization.JsonTypeInfo" />.
    /// </summary>
    /// <param name="jsonTypeInfo">The JSON type information representing the base type for which to resolve derived types.</param>
    /// <returns>
    /// An array of <see cref="System.Type" /> objects representing the known derived types for the given JSON type info.
    /// Returns an empty array if there are no known derived types.
    /// </returns>
    public virtual Type[] GetDerivedTypes(JsonTypeInfo jsonTypeInfo)
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

    /// <summary>
    /// Configures polymorphic serialization options for the specified <see cref="JsonTypeInfo" />.
    /// Sets handling for unknown derived types and registers the provided derived types for polymorphic serialization.
    /// </summary>
    /// <param name="jsonTypeInfo">The <see cref="JsonTypeInfo"/> instance to configure.</param>
    /// <param name="derivedTypes">The derived types to register for polymorphic serialization.</param>
    public void ConfigureJsonPolymorphismOptions(JsonTypeInfo jsonTypeInfo, params Type[] derivedTypes)
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
}
