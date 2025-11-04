using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Transforms enum schemas to use string representation with enum member names.
/// </summary>
public class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (!context.JsonTypeInfo.Type.IsEnum)
        {
            return Task.CompletedTask;
        }

        schema.Type = JsonSchemaType.String;
        schema.Format = null;
        schema.Enum?.Clear();
        schema.Enum ??= new List<JsonNode>();
        foreach (var name in Enum.GetNames(context.JsonTypeInfo.Type))
        {
            var actualName = context.JsonTypeInfo.Type.GetField(name)?.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? name;
            schema.Enum.Add(JsonValue.Create(actualName));
        }

        return Task.CompletedTask;
    }
}
