using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Umbraco.Cms.Api.Common.OpenApi;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema model, SchemaFilterContext context)
    {
        if (model is not OpenApiSchema schema || context.Type.IsEnum is false)
        {
            return;
        }

        schema.Type = JsonSchemaType.String;
        schema.Format = null;
        schema.Enum = new List<JsonNode>();
        foreach (var name in Enum.GetNames(context.Type))
        {
            var actualName = context.Type.GetField(name)?.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? name;
            schema.Enum.Add(actualName);
        }
    }
}
