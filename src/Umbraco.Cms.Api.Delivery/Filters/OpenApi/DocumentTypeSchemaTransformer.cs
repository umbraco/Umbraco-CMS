using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Delivery.Filters.OpenApi;

/// <summary>
/// Transforms the OpenAPI document to add schemas for the instance's document types.
/// </summary>
public class DocumentTypeSchemaTransformer : IOpenApiSchemaTransformer
{
    private readonly IContentTypeInfoService _contentTypeInfoService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentTypeSchemaTransformer"/> class.
    /// </summary>
    /// <param name="contentTypeInfoService">The content type info service.</param>
    public DocumentTypeSchemaTransformer(IContentTypeInfoService contentTypeInfoService)
        => _contentTypeInfoService = contentTypeInfoService;

    /// <inheritdoc />
    public async Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        switch (context.JsonTypeInfo.Type)
        {
            case var type when type == typeof(IApiContentResponse):
                await ApplyPolymorphicContentType(
                    schema,
                    context,
                    _contentTypeInfoService.GetContentTypes().Where(c => !c.IsElement).ToList(),
                    async contentType =>
                    {
                        var schemaId = $"{contentType.SchemaId}ContentResponseModel";
                        return (
                            schemaId,
                            new OpenApiSchema
                            {
                                Type = JsonSchemaType.Object,
                                Properties = new Dictionary<string, IOpenApiSchema>
                                {
                                    ["properties"] = new OpenApiSchema
                                    {
                                        Type = JsonSchemaType.Object,
                                        Properties = await ContentTypePropertiesMapper(contentType, context),
                                    },
                                },
                                AdditionalPropertiesAllowed = false,
                                Metadata = new Dictionary<string, object>
                                {
                                    ["x-schema-id"] = schemaId,
                                },
                            });
                    });
                return;
            case var type when type == typeof(IApiContent):
                await ApplyPolymorphicContentType(
                    schema,
                    context,
                    _contentTypeInfoService.GetContentTypes().Where(c => !c.IsElement).ToList(),
                    async contentType =>
                    {
                        var schemaId = $"{contentType.SchemaId}ContentModel";
                        return (
                            schemaId,
                            new OpenApiSchema
                            {
                                Type = JsonSchemaType.Object,
                                Properties = new Dictionary<string, IOpenApiSchema>
                                {
                                    ["properties"] = new OpenApiSchema
                                    {
                                        Type = JsonSchemaType.Object,
                                        Properties = await ContentTypePropertiesMapper(contentType, context),
                                    },
                                },
                                AdditionalPropertiesAllowed = false,
                                Metadata = new Dictionary<string, object>
                                {
                                    ["x-schema-id"] = schemaId,
                                },
                            });
                    });
                return;
            case var type when type == typeof(IApiElement):
                await ApplyPolymorphicContentType(
                    schema,
                    context,
                    _contentTypeInfoService.GetContentTypes().Where(c => c.IsElement).ToList(),
                    async contentType =>
                    {
                        var schemaId = $"{contentType.SchemaId}ElementModel";
                        return (
                            schemaId,
                            new OpenApiSchema
                            {
                                Type = JsonSchemaType.Object,
                                Properties = new Dictionary<string, IOpenApiSchema>
                                {
                                    ["properties"] = new OpenApiSchema
                                    {
                                        Type = JsonSchemaType.Object,
                                        Properties = await ContentTypePropertiesMapper(contentType, context),
                                    },
                                },
                                AdditionalPropertiesAllowed = false,
                                Metadata = new Dictionary<string, object>
                                {
                                    ["x-schema-id"] = schemaId,
                                },
                            });
                    });
                return;
        }
    }

    private async Task ApplyPolymorphicContentType(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        List<ContentTypeInfo> contentTypes,
        Func<ContentTypeInfo, Task<(string SchemaId, OpenApiSchema Schema)>> contentTypeSchemaMapper)
    {
        // Ensure that the derived types are generated
        List<string> derivedTypeSchemaIds = [];
        foreach (JsonDerivedType derivedType in context.JsonTypeInfo.PolymorphismOptions?.DerivedTypes ?? [])
        {
            OpenApiSchema derivedTypeSchema = await context.GetOrCreateSchemaAsync(derivedType.DerivedType);
            derivedTypeSchema.Properties?.Remove("properties");
            var derivedTypeSchemaId = derivedTypeSchema.Metadata?["x-schema-id"] as string ?? throw new InvalidOperationException("Missing derived type schema id.");
            context.Document?.AddComponent(derivedTypeSchemaId, derivedTypeSchema);
            derivedTypeSchemaIds.Add(derivedTypeSchemaId);
        }

        // Inline the schema
        schema.Metadata?.Remove("x-schema-id");

        // Set up discriminator
        schema.Discriminator = new OpenApiDiscriminator
        {
            PropertyName = "contentType",
            Mapping = new Dictionary<string, OpenApiSchemaReference>(),
        };
        schema.OneOf ??= new List<IOpenApiSchema>();

        foreach (ContentTypeInfo contentType in contentTypes)
        {
            (var schemaId, OpenApiSchema openApiSchema) = await contentTypeSchemaMapper(contentType);
            openApiSchema.OneOf = derivedTypeSchemaIds?
                .Select(derivedTypeSchemaId => new OpenApiSchemaReference(derivedTypeSchemaId, context.Document))
                .OfType<IOpenApiSchema>()
                .ToList();
            context.Document?.AddComponent(schemaId, openApiSchema);
            schema.OneOf.Add(new OpenApiSchemaReference(schemaId, context.Document));
            schema.Discriminator.Mapping[contentType.Alias] = new OpenApiSchemaReference(schemaId, context.Document);
        }

        // Remove all schema properties that are now handled by the derived types
        schema.Type = null;
        schema.AnyOf = null;
    }

    private static async Task<Dictionary<string, IOpenApiSchema>> ContentTypePropertiesMapper(ContentTypeInfo contentType, OpenApiSchemaTransformerContext context)
    {
        var properties = new Dictionary<string, IOpenApiSchema>();
        foreach (ContentTypePropertyInfo propertyInfo in contentType.Properties)
        {
            OpenApiSchema propertySchema = await context.GetOrCreateSchemaAsync(propertyInfo.Type);
            propertySchema.Type |= JsonSchemaType.Null;
            var schemaId = UmbracoSchemaIdGenerator.Generate(propertyInfo.Type);
            context.Document?.AddComponent(schemaId, propertySchema);
            properties[propertyInfo.Alias] = new OpenApiSchemaReference(schemaId, context.Document);
        }

        return properties;
    }
}
