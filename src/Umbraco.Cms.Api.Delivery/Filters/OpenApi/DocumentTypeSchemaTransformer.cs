using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Core;
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
    private readonly IJsonTypeInfoResolver _jsonTypeInfoResolver;
    private readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentTypeSchemaTransformer"/> class.
    /// </summary>
    /// <param name="contentTypeInfoService">The content type info service.</param>
    /// <param name="jsonOptionsMonitor">The JSON options monitor.</param>
    public DocumentTypeSchemaTransformer(
        IContentTypeInfoService contentTypeInfoService,
        IOptionsMonitor<JsonOptions> jsonOptionsMonitor)
    {
        _contentTypeInfoService = contentTypeInfoService;
        _serializerOptions = jsonOptionsMonitor
            .Get(Constants.JsonOptionsNames.DeliveryApi)
            .SerializerOptions;
        _jsonTypeInfoResolver = _serializerOptions.TypeInfoResolver
                                ?? throw new InvalidOperationException("The JSON serializer options must have a TypeInfoResolver configured.");
    }

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
                        return (schemaId, await CreateContentTypeSchema(schemaId, contentType, context));
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
                        return (schemaId, await CreateContentTypeSchema(schemaId, contentType, context));
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
                        return (schemaId, await CreateContentTypeSchema(schemaId, contentType, context));
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
        var schemaId = GetSchemaId(context.JsonTypeInfo) ?? throw new InvalidOperationException("The schema must have an ID to apply polymorphic content types.");
        context.Document!.AddComponent(schemaId, new OpenApiSchema());

        // Ensure that the derived types are generated
        List<IOpenApiSchema> derivedTypeSchemas = [];
        foreach (JsonDerivedType derivedType in context.JsonTypeInfo.PolymorphismOptions?.DerivedTypes ?? [])
        {
            IOpenApiSchema derivedTypeSchema = await CreateSchema(
                GetJsonTypeInfo(derivedType.DerivedType),
                context,
                derivedTypeSchema =>
                {
                    derivedTypeSchema.Properties?.Remove("properties");
                });
            derivedTypeSchemas.Add(derivedTypeSchema);
        }

        // Set up discriminator
        schema.Discriminator = new OpenApiDiscriminator
        {
            PropertyName = "contentType",
            Mapping = new Dictionary<string, OpenApiSchemaReference>(),
        };
        schema.OneOf ??= new List<IOpenApiSchema>();

        foreach (ContentTypeInfo contentType in contentTypes)
        {
            (var contentTypeSchemaId, OpenApiSchema contentTypeSchema) = await contentTypeSchemaMapper(contentType);
            schema.OneOf.Add(new OpenApiSchemaReference(contentTypeSchemaId, context.Document));
            schema.Discriminator.Mapping[contentType.Alias] = new OpenApiSchemaReference(contentTypeSchemaId, context.Document);

            // Only add the schema if it isn't already present
            if (context.Document?.Components?.Schemas?.ContainsKey(contentTypeSchemaId) == true)
            {
                continue;
            }

            contentTypeSchema.OneOf = derivedTypeSchemas;
            context.Document?.AddComponent(contentTypeSchemaId, contentTypeSchema);
        }

        // Remove all schema properties that are now handled by the derived types
        schema.Type = null;
        schema.AnyOf = null;

        // Inline the schema
        schema.Metadata?.Remove("x-schema-id");

        context.Document!.Components!.Schemas!.Remove(schemaId);
    }

    /// <summary>
    /// Creates and adds a schema to the OpenAPI document if it does not already exist.
    /// </summary>
    /// <remarks>A placeholder schema is added first to avoid recursion issues when generating schemas that reference themselves.</remarks>
    private async Task<IOpenApiSchema> CreateSchema(
        JsonTypeInfo jsonTypeInfo,
        OpenApiSchemaTransformerContext context,
        Action<OpenApiSchema>? configureSchema = null)
    {
        var schemaId = GetSchemaId(jsonTypeInfo);

        // If the schema ID is null, that means the schema should be inlined, so we create and return it directly.
        if (schemaId is null)
        {
            // For array or enumerable types, we need to create the item schema for the elements and then return the array schema inline.
            if (jsonTypeInfo.Type.IsArray || jsonTypeInfo.Kind == JsonTypeInfoKind.Enumerable)
            {
                Type elementType = jsonTypeInfo.ElementType ?? jsonTypeInfo.Type.GetElementType() ?? typeof(object);
                JsonTypeInfo elementJsonTypeInfo = GetJsonTypeInfo(elementType);
                IOpenApiSchema itemSchema = await CreateSchema(elementJsonTypeInfo, context, configureSchema);
                var arraySchema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = itemSchema,
                };
                return arraySchema;
            }

            OpenApiSchema inlineSchema = await context.GetOrCreateSchemaAsync(jsonTypeInfo.Type);
            configureSchema?.Invoke(inlineSchema);
            return inlineSchema;
        }

        if (context.Document!.Components?.Schemas?.ContainsKey(schemaId) == true)
        {
            return new OpenApiSchemaReference(schemaId, context.Document);
        }

        // Add an empty placeholder schema to avoid recursion issues
        //context.Document!.AddComponent(schemaId, new OpenApiSchema());
        OpenApiSchema schema = await context.GetOrCreateSchemaAsync(jsonTypeInfo.Type);
        configureSchema?.Invoke(schema);

        // Remove the placeholder and add the real schema
        //context.Document!.Components?.Schemas?.Remove(schemaId);
        context.Document!.AddComponent(schemaId, schema);
        return new OpenApiSchemaReference(schemaId, context.Document);
    }

    private async Task<OpenApiSchema> CreateContentTypeSchema(
        string schemaId,
        ContentTypeInfo contentType,
        OpenApiSchemaTransformerContext context) =>
        new()
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["properties"] = await CreatePropertiesSchema(contentType, context),
            },
            AdditionalPropertiesAllowed = false,
            Metadata = new Dictionary<string, object> { ["x-schema-id"] = schemaId, },
        };

    private async Task<IOpenApiSchema> CreatePropertiesSchema(
        ContentTypeInfo contentType,
        OpenApiSchemaTransformerContext context)
    {
        var schemaId = $"{contentType.SchemaId}PropertiesModel";
        if (context.Document!.Components?.Schemas?.ContainsKey(schemaId) == true)
        {
            return new OpenApiSchemaReference(schemaId, context.Document);
        }

        var propertiesSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = await ContentTypePropertiesMapper(contentType, context),
            Metadata = new Dictionary<string, object> { ["x-schema-id"] = schemaId, },
        };
        context.Document.AddComponent(schemaId, propertiesSchema);
        return new OpenApiSchemaReference(schemaId, context.Document);
    }

    private async Task<Dictionary<string, IOpenApiSchema>> ContentTypePropertiesMapper(
        ContentTypeInfo contentType,
        OpenApiSchemaTransformerContext context)
    {
        var properties = new Dictionary<string, IOpenApiSchema>();
        foreach (ContentTypePropertyInfo propertyInfo in contentType.Properties)
        {
            IOpenApiSchema schema = await CreateSchema(GetJsonTypeInfo(propertyInfo.Type), context);
            properties[propertyInfo.Alias] = schema;
        }

        return properties;
    }

    private JsonTypeInfo GetJsonTypeInfo(Type type)
    {
        JsonTypeInfo? jsonTypeInfo = _jsonTypeInfoResolver.GetTypeInfo(type, _serializerOptions);
        return jsonTypeInfo ?? throw new InvalidOperationException("Could not get JsonTypeInfo for type " + type.FullName);
    }

    private string? GetSchemaId(JsonTypeInfo type)
        => ConfigureUmbracoOpenApiOptionsBase.CreateSchemaReferenceId(type);
}
