using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Ensures that all non-nullable properties are marked as required in the OpenAPI schema.
/// </summary>
public class RequireNonNullablePropertiesSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        IEnumerable<string> additionalRequiredProps = schema.Properties?
          .Where(x => (x.Value.Type & JsonSchemaType.Null) == 0 && schema.Required?.Contains(x.Key) != true)
          .Select(x => x.Key)
          ?? [];
        schema.Required ??= new HashSet<string>();
        foreach (var propKey in additionalRequiredProps)
        {
            schema.Required.Add(propKey);
        }

        return Task.CompletedTask;
    }
}
