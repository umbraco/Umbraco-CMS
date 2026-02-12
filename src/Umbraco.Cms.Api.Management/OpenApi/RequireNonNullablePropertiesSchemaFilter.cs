using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Umbraco.Cms.Api.Management.OpenApi;

public class RequireNonNullablePropertiesSchemaFilter : ISchemaFilter
{
    /// <summary>
    /// Add to model.Required all properties where Nullable is false.
    /// </summary>
    public void Apply(IOpenApiSchema model, SchemaFilterContext context)
    {
        if (model is not OpenApiSchema schema)
        {
            return;
        }

        IEnumerable<string> additionalRequiredProps = schema.Properties
            ?.Where(x => x.Value.Type?.HasFlag(JsonSchemaType.Null) is not true && model.Required?.Contains(x.Key) is not true)
            .Select(x => x.Key)
            ?? [];
        schema.Required ??= new SortedSet<string>();
        foreach (var propKey in additionalRequiredProps)
        {
            schema.Required.Add(propKey);
        }
    }
}
