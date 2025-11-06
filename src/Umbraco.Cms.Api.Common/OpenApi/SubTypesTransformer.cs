using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.Serialization;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// An OpenAPI schema transformer that handles subtypes by using the "oneOf" construct for types with more than one subtype.
/// </summary>
/// <remarks>
/// This transformer uses <see cref="IUmbracoJsonTypeInfoResolver"/> to find subtypes of a given type.<br/>
/// If only one subtype exists, it replaces the base type schema with the subtype schema.<br/>
/// If multiple subtypes exist, it cleans up and inlines the main schema and adds `oneOf` referencing each subtype schema.
/// </remarks>
public class SubTypesTransformer : IOpenApiSchemaTransformer
{
    private const string SchemaIdKey = "x-schema-id";

    /// <inheritdoc />
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver = context.ApplicationServices.GetRequiredService<IUmbracoJsonTypeInfoResolver>();
        var subTypes = umbracoJsonTypeInfoResolver.FindSubTypes(context.JsonTypeInfo.Type).ToList();
        if (subTypes.Count == 0 || (subTypes.Count == 1 && subTypes[0] == context.JsonTypeInfo.Type))
        {
            // If there are no subtypes, or the only subtype is the base type itself, do nothing
            return;
        }

        if (subTypes.Count == 1)
        {
            // Only one subtype, just replace the schema with the subtype schema
            var subTypeSchemaId = UmbracoSchemaIdGenerator.Generate(subTypes[0]);
            schema.Metadata ??= new Dictionary<string, object>();
            schema.Metadata[SchemaIdKey] = subTypeSchemaId;
            if (context.Document?.Components?.Schemas?.ContainsKey(subTypeSchemaId) == true)
            {
                // If the schema is already registered, we can just return
                return;
            }

            OpenApiSchema subTypeSchema = await context.GetOrCreateSchemaAsync(subTypes[0], cancellationToken: cancellationToken);

            // By registering the subtype with the same id, we effectively replace the base type schema with the subtype schema
            context.Document?.AddComponent(subTypeSchemaId, subTypeSchema);
            return;
        }

        schema.Metadata?.Remove(SchemaIdKey); // Removing the schema id will lead to an inline schema
        CleanupSchema(schema); // By cleaning up the base schema, we will only have the oneOf with subtypes and the subtypes themselves will have the full schema
        schema.OneOf ??= new List<IOpenApiSchema>();
        foreach (Type subType in subTypes)
        {
            OpenApiSchema subTypeSchema = await context.GetOrCreateSchemaAsync(subType, cancellationToken: cancellationToken);
            var subTypeSchemaId = GetSchemaId(subTypeSchema);
            context.Document?.AddComponent(subTypeSchemaId, subTypeSchema);
            schema.OneOf.Add(new OpenApiSchemaReference(subTypeSchemaId, context.Document));
        }
    }

    private static string GetSchemaId(OpenApiSchema schema)
    {
        if (schema.Metadata?.TryGetValue(SchemaIdKey, out var schemaIdObj) == true && schemaIdObj is string schemaIdStr)
        {
            return schemaIdStr;
        }

        throw new NotSupportedException("Schema id is missing.");
    }

    private static void CleanupSchema(OpenApiSchema schema)
    {
        schema.Properties?.Clear();
        schema.Required?.Clear();
    }
}
