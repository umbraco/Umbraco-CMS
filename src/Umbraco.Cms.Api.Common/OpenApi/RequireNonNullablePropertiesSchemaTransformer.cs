using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Ensures that all non-nullable properties are marked as required in the OpenAPI schema.
/// </summary>
/// <remarks>By default, only properties marked with the required keyword will actually show as required.
/// Non-nullable reference types were not taken into account.</remarks>
internal class RequireNonNullablePropertiesSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        IEnumerable<string> additionalRequiredProps = schema.Properties?
            .Where(p => schema.Required?.Contains(p.Key) != true) // If it's already required, skip
            .Where(x => IsRequiredProperty(schema, context.JsonTypeInfo, x.Key))
            .Select(x => x.Key)
          ?? [];
        schema.Required ??= new HashSet<string>();
        foreach (var propKey in additionalRequiredProps)
        {
            schema.Required.Add(propKey);
        }

        return Task.CompletedTask;
    }

    private static bool IsRequiredProperty(OpenApiSchema schema, JsonTypeInfo jsonTypeInfo, string propertyName)
    {
        if (jsonTypeInfo.Properties.FirstOrDefault(p => p.Name == propertyName) is not { } property)
        {
            // If we can't find the property in the type (e.g. discriminator )'$type', use the schema type information
            IOpenApiSchema? schemaProperty = schema.Properties?[propertyName];
            return schemaProperty?.Type is { } propertyType && propertyType.HasFlag(JsonSchemaType.Null) is false;
        }

        return property.IsGetNullable is false;
    }
}
