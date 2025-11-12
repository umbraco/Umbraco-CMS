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
public class DocumentTypeSchemaTransformer : IOpenApiSchemaTransformer, IOpenApiDocumentTransformer
{
    private const string CustomRecursiveRefKey = "x-recursive-ref";
    private readonly IContentTypeInfoService _contentTypeInfoService;
    private readonly HashSet<Type> _handledTypes = [];
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
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (document.Components?.Schemas is not { Count: > 0 })
        {
            return Task.CompletedTask;
        }

        foreach (IOpenApiSchema componentsSchema in document.Components.Schemas.Values)
        {
            ReplacePlaceholderSchemas(document, componentsSchema);
        }

        return Task.CompletedTask;
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
        _handledTypes.Add(context.JsonTypeInfo.Type);

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

        schema.Discriminator = new OpenApiDiscriminator
        {
            PropertyName = "contentType",
            Mapping = new Dictionary<string, OpenApiSchemaReference>(),
        };
        schema.OneOf ??= new List<IOpenApiSchema>();

        foreach (ContentTypeInfo contentType in contentTypes)
        {
            (var contentTypeSchemaId, OpenApiSchema contentTypeSchema) = await contentTypeSchemaMapper(contentType);
            contentTypeSchema.OneOf = derivedTypeSchemas;
            schema.Discriminator.Mapping[contentType.Alias] = new OpenApiSchemaReference(contentTypeSchemaId, context.Document);
            schema.OneOf.Add(contentTypeSchema);
        }

        // Remove all schema properties that are now handled by the derived types
        schema.Type = null;
        schema.AnyOf = null;

        // Inline the schema
        schema.Metadata?.Remove("x-schema-id");
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

        var schemaId = GetSchemaId(jsonTypeInfo);

        // If this is one of the types we handle, and we already started generating it, return a placeholder
        // to avoid circular reference issues.
        // In the document transformer, these placeholders will be replaced with the actual schemas.
        if (schemaId is not null && _handledTypes.Contains(jsonTypeInfo.Type))
        {
            return GetPlaceholderSchema(schemaId);
        }

        OpenApiSchema schema = await context.GetOrCreateSchemaAsync(jsonTypeInfo.Type);
        configureSchema?.Invoke(schema);
        return schema;
    }

    private static string? GetSchemaId(JsonTypeInfo type)
        => ConfigureUmbracoOpenApiOptionsBase.CreateSchemaReferenceId(type);

    private static OpenApiSchema GetPlaceholderSchema(string schemaId)
        => new()
        {
            Metadata = new Dictionary<string, object>
            {
                [CustomRecursiveRefKey] = schemaId,
            },
        };

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
        if (context.Document!.Components?.Schemas?.TryGetValue(schemaId, out IOpenApiSchema? existingSchema) == true)
        {
            return existingSchema;
        }

        var propertiesSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = await ContentTypePropertiesMapper(contentType, context),
            Metadata = new Dictionary<string, object> { ["x-schema-id"] = schemaId, },
        };
        context.Document.AddComponent(schemaId, propertiesSchema);
        return propertiesSchema;
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

    /// <summary>
    /// Replaces placeholder schemas (that caused circular references) with the actual schemas in the OpenAPI document.
    /// </summary>
    /// <param name="document">The OpenAPI document.</param>
    /// <param name="schema">The schema to process.</param>
    private static void ReplacePlaceholderSchemas(OpenApiDocument document, IOpenApiSchema schema)
    {
        if (document.Components?.Schemas is null || schema.Properties is not { Count: > 0 })
        {
            return;
        }

        foreach (var propertyKey in schema.Properties.Keys)
        {
            IOpenApiSchema propertySchema = schema.Properties[propertyKey];
            if (propertySchema is not OpenApiSchema innerSchema)
            {
                continue;
            }

            // Check if this is a placeholder schema
            if (innerSchema.Metadata?.TryGetValue(CustomRecursiveRefKey, out var recursiveRefIdObj) == true
                && recursiveRefIdObj is string recursiveRefId)
            {
                // Replace the reference with the actual schema
                schema.Properties[propertyKey] = document.Components.Schemas.TryGetValue(recursiveRefId, out IOpenApiSchema? actualItemSchema)
                    ? actualItemSchema
                    : new OpenApiSchema { Type = JsonSchemaType.Object };
                continue;
            }

            // Check if this is an array with a placeholder item schema
            if (innerSchema.Items is OpenApiSchema itemsSchema
                && itemsSchema.Metadata?.TryGetValue(CustomRecursiveRefKey, out var itemsRecursiveRefIdObj) == true
                && itemsRecursiveRefIdObj is string itemsRecursiveRefId)
            {
                // Replace the reference with the actual schema
                innerSchema.Items = document.Components.Schemas.TryGetValue(itemsRecursiveRefId, out IOpenApiSchema? actualItemSchema)
                    ? actualItemSchema
                    : null;
                continue;
            }

            // Recursive call to handle the property schema
            ReplacePlaceholderSchemas(document, innerSchema);
        }
    }
}
