using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.OpenApi;

public class SupportedDerivedTypesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // See if the schema has any properties with our custom attribute
        IEnumerable<PropertyInfo> props = context.Type.GetProperties().Where(x => x.HasAttribute<SupportedDerivedTypesAttribute>());
        foreach (PropertyInfo prop in props)
        {
            var jsonPropNameAttr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
            var propName = jsonPropNameAttr != null ? jsonPropNameAttr.Name : prop.Name.ToFirstLower();

            // Need a better way to get from property name to schema property name
            if (schema.Properties.ContainsKey(propName))
            {
                // Convert the derivative types to a list of schemas (we assume they are all schemas by now)
                OpenApiSchema[] typesSchemas = prop.GetCustomAttribute<SupportedDerivedTypesAttribute>()!
                    .Types
                    .Select(x => context.SchemaRepository.TryLookupByType(x, out OpenApiSchema? s) ? s : null)
                    .Where(x => x != null)
                    .Cast<OpenApiSchema>()
                    .ToArray();

                // Remove all but the derivative types from the schema
                if (typesSchemas.Length > 0)
                {
                    schema.Properties[propName].OneOf
                        .RemoveAll(x => typesSchemas.All(y => y.Reference.Id != x.Reference.Id));
                }
            }
        }
    }
}
