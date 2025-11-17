using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Ensures that all non-nullable properties are marked as required in the OpenAPI schema.
/// </summary>
internal class RequireNonNullablePropertiesSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        IEnumerable<string> additionalRequiredProps = schema.Properties?
          .Where(x => ((x.Value.Type & JsonSchemaType.Null) == 0 || IsNonNullableProperty(context.JsonTypeInfo, x.Key)) && schema.Required?.Contains(x.Key) != true)
          .Select(x => x.Key)
          ?? [];
        schema.Required ??= new HashSet<string>();
        foreach (var propKey in additionalRequiredProps)
        {
            schema.Required.Add(propKey);
        }

        return Task.CompletedTask;
    }

    private static bool IsNonNullableProperty(JsonTypeInfo jsonTypeInfo, string propertyName)
    {
        if (jsonTypeInfo.Properties.FirstOrDefault(p => p.Name == propertyName) is not { } property)
        {
            return false;
        }

        // For value types, check if it's not Nullable<T>
        if (property.PropertyType.IsValueType)
        {
            return Nullable.GetUnderlyingType(property.PropertyType) == null;
        }

        // For reference types, use NullabilityInfoContext to check nullable annotations
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        MemberInfo? member = jsonTypeInfo.Type.GetProperty(property.Name, flags)
            ?? (MemberInfo?)jsonTypeInfo.Type.GetField(property.Name, flags);

        if (member == null)
        {
            return false;
        }

        var context = new NullabilityInfoContext();
        NullabilityInfo nullability = member switch
        {
            PropertyInfo p => context.Create(p),
            FieldInfo f => context.Create(f),
            _ => throw new InvalidOperationException($"Unexpected member type: {member.GetType()}"),
        };

        return nullability.ReadState == NullabilityState.NotNull;
    }
}
