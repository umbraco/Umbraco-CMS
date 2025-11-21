using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Umbraco.Cms.Api.Common.OpenApi;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema model, SchemaFilterContext context)
    {
        if (context.Type.IsEnum && model is OpenApiSchema schema )
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = null;
            schema.Enum?.Clear();
            foreach (var name in Enum.GetNames(context.Type))
            {
                var actualName = context.Type.GetField(name)?.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? name;
                model.Enum?.Add(actualName);
            }
        }
    }


}
