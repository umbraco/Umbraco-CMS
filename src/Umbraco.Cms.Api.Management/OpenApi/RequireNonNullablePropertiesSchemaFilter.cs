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
        var additionalRequiredProps = model.Properties?
            .Where(x => !model.Required?.Contains(x.Key) == true)
            .Select(x => x.Key);
        if (additionalRequiredProps != null)
        {
            foreach (var propKey in additionalRequiredProps)
            {
                model.Required?.Add(propKey);
            }
        }
    }
}
