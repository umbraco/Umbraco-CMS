using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Delivery.OpenApi.Transformers;

/// <summary>
/// Transforms the OpenAPI document to add schemas for the instance's document types.
/// </summary>
public class ContentTypeSchemaTransformer : IOpenApiSchemaTransformer, IOpenApiDocumentTransformer
{
    private const string CustomRecursiveRefKey = "x-recursive-ref";
    private readonly IContentTypeSchemaService _contentTypeSchemaService;
    private readonly ILogger<ContentTypeSchemaTransformer> _logger;
    private readonly HashSet<string> _handledSchemas = [];
    private readonly IJsonTypeInfoResolver _jsonTypeInfoResolver;
    private readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeSchemaTransformer"/> class.
    /// </summary>
    /// <param name="contentTypeSchemaService">The content type info service.</param>
    /// <param name="jsonOptionsMonitor">The JSON options monitor.</param>
    /// <param name="logger">The logger.</param>
    public ContentTypeSchemaTransformer(
        IContentTypeSchemaService contentTypeSchemaService,
        IOptionsMonitor<JsonOptions> jsonOptionsMonitor,
        ILogger<ContentTypeSchemaTransformer> logger)
    {
        _contentTypeSchemaService = contentTypeSchemaService;
        _logger = logger;
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
                    PublishedItemType.Content,
                    _contentTypeSchemaService.GetDocumentTypes().Where(c => !c.IsElement).ToList(),
                    async contentType =>
                    {
                        var schemaId = $"{contentType.SchemaId}ContentResponseModel";
                        return (schemaId, await CreateContentTypeSchema(schemaId, PublishedItemType.Content, contentType, context, cancellationToken));
                    },
                    cancellationToken);
                return;
            case var type when type == typeof(IApiContent):
                await ApplyPolymorphicContentType(
                    schema,
                    context,
                    PublishedItemType.Content,
                    _contentTypeSchemaService.GetDocumentTypes().Where(c => !c.IsElement).ToList(),
                    async contentType =>
                    {
                        var schemaId = $"{contentType.SchemaId}ContentModel";
                        return (schemaId, await CreateContentTypeSchema(schemaId, PublishedItemType.Content, contentType, context, cancellationToken));
                    },
                    cancellationToken);
                return;
            case var type when type == typeof(IApiElement):
                await ApplyPolymorphicContentType(
                    schema,
                    context,
                    PublishedItemType.Content,
                    _contentTypeSchemaService.GetDocumentTypes().Where(c => c.IsElement).ToList(),
                    async contentType =>
                    {
                        var schemaId = $"{contentType.SchemaId}ElementModel";
                        return (schemaId, await CreateContentTypeSchema(schemaId, PublishedItemType.Content, contentType, context, cancellationToken));
                    },
                    cancellationToken);
                return;
            case var type when type == typeof(IApiMediaWithCropsResponse):
                await ApplyPolymorphicContentType(
                    schema,
                    context,
                    PublishedItemType.Media,
                    _contentTypeSchemaService.GetMediaTypes().ToList(),
                    async contentType =>
                    {
                        var schemaId = $"{contentType.SchemaId}MediaWithCropsResponseModel";
                        return (schemaId, await CreateContentTypeSchema(schemaId, PublishedItemType.Media, contentType, context, cancellationToken));
                    },
                    cancellationToken);
                return;
            case var type when type == typeof(IApiMediaWithCrops):
                await ApplyPolymorphicContentType(
                    schema,
                    context,
                    PublishedItemType.Media,
                    _contentTypeSchemaService.GetMediaTypes().ToList(),
                    async contentType =>
                    {
                        var schemaId = $"{contentType.SchemaId}MediaWithCropsModel";
                        return (schemaId, await CreateContentTypeSchema(schemaId, PublishedItemType.Media, contentType, context, cancellationToken));
                    },
                    cancellationToken);
                return;
        }
    }

    private async Task ApplyPolymorphicContentType(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        PublishedItemType itemType,
        List<ContentTypeSchemaInfo> contentTypes,
        Func<ContentTypeSchemaInfo, Task<(string SchemaId, OpenApiSchema Schema)>> contentTypeSchemaMapper,
        CancellationToken cancellationToken)
    {
        List<IOpenApiSchema> derivedTypeSchemas = [];
        foreach (JsonDerivedType derivedType in context.JsonTypeInfo.PolymorphismOptions?.DerivedTypes ?? [])
        {
            IOpenApiSchema derivedTypeSchema = await CreateSchema(
                GetJsonTypeInfo(derivedType.DerivedType),
                context,
                cancellationToken);
            derivedTypeSchemas.Add(derivedTypeSchema);
        }

        schema.Discriminator = new OpenApiDiscriminator
        {
            PropertyName = GetTypePropertyName(itemType),
            Mapping = new Dictionary<string, OpenApiSchemaReference>(),
        };
        schema.OneOf ??= new List<IOpenApiSchema>();

        foreach (ContentTypeSchemaInfo contentType in contentTypes)
        {
            (var contentTypeSchemaId, OpenApiSchema contentTypeSchema) = await contentTypeSchemaMapper(contentType);
            contentTypeSchema.AllOf = derivedTypeSchemas;
            schema.Discriminator.Mapping[contentType.Alias] = new OpenApiSchemaReference(contentTypeSchemaId, context.Document);
            schema.OneOf.Add(contentTypeSchema);
        }

        // Remove all schema properties that are now handled by the derived types
        schema.Type = null;
        schema.AnyOf = null;
    }

    /// <summary>
    /// Creates and adds a schema to the OpenAPI document if it does not already exist.
    /// </summary>
    /// <remarks>A placeholder schema is added first to avoid recursion issues when generating schemas that reference themselves.</remarks>
    private async Task<IOpenApiSchema> CreateSchema(
        JsonTypeInfo jsonTypeInfo,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken,
        Action<OpenApiSchema>? configureSchema = null)
    {
        if (jsonTypeInfo.Type.IsArray || jsonTypeInfo.Kind == JsonTypeInfoKind.Enumerable)
        {
            Type elementType = jsonTypeInfo.ElementType ?? jsonTypeInfo.Type.GetElementType() ?? typeof(object);
            JsonTypeInfo elementJsonTypeInfo = GetJsonTypeInfo(elementType);
            IOpenApiSchema itemSchema = await CreateSchema(elementJsonTypeInfo, context, cancellationToken, configureSchema);
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
        if (schemaId is not null && !_handledSchemas.Add(schemaId))
        {
            return GetPlaceholderSchema(schemaId);
        }

        OpenApiSchema schema;
        try
        {
            schema = await context.GetOrCreateSchemaAsync(
                jsonTypeInfo.Type,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create OpenAPI schema for type {TypeName}", jsonTypeInfo.Type.FullName);
            schema = new OpenApiSchema();
        }

        configureSchema?.Invoke(schema);
        return schema;
    }

    private string? GetSchemaId(JsonTypeInfo type)
    {
        var defaultSchemaReferenceId = OpenApiOptions.CreateDefaultSchemaReferenceId(type);
        return defaultSchemaReferenceId is null ? null : UmbracoSchemaIdGenerator.Generate(type.Type);
    }

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
        PublishedItemType itemType,
        ContentTypeSchemaInfo contentType,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var typePropertyName = GetTypePropertyName(itemType);
        return new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                [typePropertyName] = new OpenApiSchema { Const = contentType.Alias },
                ["properties"] = await CreatePropertiesSchema(contentType, context, cancellationToken),
            },
            Required = new HashSet<string> { typePropertyName },
            AdditionalPropertiesAllowed = false,
            Metadata = new Dictionary<string, object> { ["x-schema-id"] = schemaId, },
        };
    }

    private async Task<IOpenApiSchema> CreatePropertiesSchema(
        ContentTypeSchemaInfo contentType,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var schemaId = $"{contentType.SchemaId}PropertiesModel";

        var propertiesSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            AllOf = [..contentType.CompositionSchemaIds.Select(compositionSchemaId => GetPlaceholderSchema($"{compositionSchemaId}PropertiesModel"))],
            Properties = await ContentTypePropertiesMapper(contentType, context, cancellationToken),
            Metadata = new Dictionary<string, object> { ["x-schema-id"] = schemaId, },
            AdditionalPropertiesAllowed = false,
        };
        return propertiesSchema;
    }

    private async Task<Dictionary<string, IOpenApiSchema>> ContentTypePropertiesMapper(
        ContentTypeSchemaInfo contentType,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var properties = new Dictionary<string, IOpenApiSchema>();
        foreach (ContentTypePropertySchemaInfo propertyInfo in contentType.Properties.Where(p => !p.Inherited))
        {
            IOpenApiSchema schema = await CreateSchema(GetJsonTypeInfo(propertyInfo.DeliveryApiClrType), context, cancellationToken);
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
        // Replace in allOf, oneOf, anyOf
        ReplacePlaceholderSchemas(document, schema.AllOf);
        ReplacePlaceholderSchemas(document, schema.OneOf);
        ReplacePlaceholderSchemas(document, schema.AnyOf);

        if (schema.Properties is not { Count: > 0 })
        {
            return;
        }

        // Process properties
        foreach (var propertyKey in schema.Properties.Keys)
        {
            IOpenApiSchema propertySchema = schema.Properties[propertyKey];
            if (propertySchema is not OpenApiSchema innerSchema)
            {
                continue;
            }

            schema.Properties[propertyKey] = GetActualSchemaOrReference(document, innerSchema, out var replaced);
            if (replaced)
            {
                // If we replaced the schema, we don't need to recurse into it
                continue;
            }

            innerSchema.Items = GetActualSchemaOrReference(document, innerSchema.Items, out replaced);
            if (replaced)
            {
                // If we replaced the schema, we don't need to recurse into it
                continue;
            }

            // Recursive call to handle the property schema
            ReplacePlaceholderSchemas(document, innerSchema);
        }
    }

    private static void ReplacePlaceholderSchemas(OpenApiDocument document, IList<IOpenApiSchema>? schemas)
    {
        if (schemas is null || schemas.Count == 0)
        {
            return;
        }

        for (var i = 0; i < schemas.Count; i++)
        {
            IOpenApiSchema allOfSchema = schemas[i];
            schemas[i] = GetActualSchemaOrReference(document, allOfSchema, out var replaced);
            if (!replaced)
            {
                ReplacePlaceholderSchemas(document, schemas[i]);
            }
        }
    }

    [return: NotNullIfNotNull(nameof(schema))]
    private static IOpenApiSchema? GetActualSchemaOrReference(OpenApiDocument document, IOpenApiSchema? schema, out bool replaced)
    {
        if (schema is not OpenApiSchema openApiSchema)
        {
            replaced = false;
            return schema;
        }

        // Check if this is a placeholder schema
        if (openApiSchema.Metadata?.TryGetValue(CustomRecursiveRefKey, out var recursiveRefIdObj) != true
            || recursiveRefIdObj is not string recursiveRefId)
        {
            replaced = false;
            return schema;
        }

        // Return the actual schema
        replaced = true;
        return new OpenApiSchemaReference(recursiveRefId, document);
    }

    private string GetTypePropertyName(PublishedItemType itemType)
        => itemType switch
        {
            PublishedItemType.Content => "contentType",
            PublishedItemType.Media => "mediaType",
            _ => throw new NotSupportedException($"Unsupported PublishedItemType: {itemType}"),
        };
}
