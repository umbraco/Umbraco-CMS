using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Delivery.OpenApi.Extensions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Delivery.OpenApi.Transformers;

/// <summary>
/// Transforms the OpenAPI document to add schemas for the instance's document types.
/// </summary>
/// <remarks>
/// <para>
/// This transformer implements both <see cref="IOpenApiSchemaTransformer"/> and <see cref="IOpenApiDocumentTransformer"/>
/// to handle schema generation in two phases:
/// </para>
/// <para>
/// <b>Phase 1 - Schema Transformation:</b> When the schema transformer encounters types like
/// <see cref="IApiContentResponse"/> or <see cref="IApiMediaWithCrops"/>, it generates content-type-specific
/// schemas (e.g., "ArticleContentResponseModel") and registers them as components in the OpenAPI document.
/// </para>
/// <para>
/// <b>Circular Reference Handling:</b> Content type schemas can reference each other (e.g., a "Page"
/// might have a property of type "Article", which might reference "Page" again). To prevent infinite recursion
/// during schema generation, we use a placeholder pattern:
/// <list type="bullet">
///   <item>When generating a schema, we track its ID in <c>_handledSchemas</c></item>
///   <item>If we encounter the same schema ID again (circular reference), we return a temporary placeholder
///   schema with metadata marking it for later replacement</item>
///   <item>The placeholder contains a <c>x-recursive-ref</c> metadata key with the target schema ID</item>
/// </list>
/// </para>
/// <para>
/// <b>Phase 2 - Document Transformation:</b> After all schemas are generated, the document transformer
/// runs and replaces all placeholder schemas with proper <c>$ref</c> references to the actual schemas.
/// This is done by <see cref="ReplacePlaceholderSchemas(OpenApiDocument, IOpenApiSchema)"/> which recursively walks through all schemas
/// and substitutes placeholders with <see cref="OpenApiSchemaReference"/> instances.
/// </para>
/// </remarks>
public sealed class ContentTypeSchemaTransformer : IOpenApiSchemaTransformer, IOpenApiDocumentTransformer
{
    // Metadata keys
    private const string RecursiveRefMetadataKey = "x-recursive-ref";
    private const string SchemaIdMetadataKey = "x-schema-id";

    // Schema ID suffixes
    private const string ContentResponseModelSuffix = "ContentResponseModel";
    private const string ContentModelSuffix = "ContentModel";
    private const string ElementModelSuffix = "ElementModel";
    private const string MediaWithCropsResponseModelSuffix = "MediaWithCropsResponseModel";
    private const string MediaWithCropsModelSuffix = "MediaWithCropsModel";
    private const string PropertiesModelSuffix = "PropertiesModel";

    private readonly IContentTypeSchemaService _contentTypeSchemaService;
    private readonly ILogger<ContentTypeSchemaTransformer> _logger;
    private readonly IJsonTypeInfoResolver _jsonTypeInfoResolver;

    /// <summary>
    /// Tracks schema IDs that have been or are being generated to detect circular references.
    /// When a schema ID is encountered a second time, a placeholder is returned instead of recursing infinitely.
    /// </summary>
    private readonly HashSet<string> _handledSchemas = [];
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

    private IReadOnlyCollection<ContentTypeSchemaInfo> DocumentTypes
        => field ??= _contentTypeSchemaService.GetDocumentTypes();

    private IReadOnlyCollection<ContentTypeSchemaInfo> MediaTypes
        => field ??= _contentTypeSchemaService.GetMediaTypes();

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
                    DocumentTypes.Where(c => !c.IsElement),
                    async (contentType, derivedTypeSchemas) =>
                    {
                        var schemaId = $"{contentType.SchemaId}{ContentResponseModelSuffix}";
                        return await CreateContentTypeSchema(
                            schemaId,
                            PublishedItemType.Content,
                            contentType,
                            derivedTypeSchemas,
                            context,
                            cancellationToken);
                    },
                    cancellationToken);
                return;
            case var type when type == typeof(IApiContent):
                await ApplyPolymorphicContentType(
                    schema,
                    context,
                    PublishedItemType.Content,
                    DocumentTypes.Where(c => !c.IsElement),
                    async (contentType, derivedTypeSchemas) =>
                    {
                        var schemaId = $"{contentType.SchemaId}{ContentModelSuffix}";
                        return await CreateContentTypeSchema(
                            schemaId,
                            PublishedItemType.Content,
                            contentType,
                            derivedTypeSchemas,
                            context,
                            cancellationToken);
                    },
                    cancellationToken);
                return;
            case var type when type == typeof(IApiElement):
                await ApplyPolymorphicContentType(
                    schema,
                    context,
                    PublishedItemType.Content,
                    DocumentTypes.Where(c => c.IsElement),
                    async (contentType, derivedTypeSchemas) =>
                    {
                        var schemaId = $"{contentType.SchemaId}{ElementModelSuffix}";
                        return await CreateContentTypeSchema(
                            schemaId,
                            PublishedItemType.Content,
                            contentType,
                            derivedTypeSchemas,
                            context,
                            cancellationToken);
                    },
                    cancellationToken);
                return;
            case var type when type == typeof(IApiMediaWithCropsResponse):
                await ApplyPolymorphicContentType(
                    schema,
                    context,
                    PublishedItemType.Media,
                    MediaTypes,
                    async (contentType, derivedTypeSchemas) =>
                    {
                        var schemaId = $"{contentType.SchemaId}{MediaWithCropsResponseModelSuffix}";
                        return await CreateContentTypeSchema(
                            schemaId,
                            PublishedItemType.Media,
                            contentType,
                            derivedTypeSchemas,
                            context,
                            cancellationToken);
                    },
                    cancellationToken);
                return;
            case var type when type == typeof(IApiMediaWithCrops):
                await ApplyPolymorphicContentType(
                    schema,
                    context,
                    PublishedItemType.Media,
                    MediaTypes,
                    async (contentType, derivedTypeSchemas) =>
                    {
                        var schemaId = $"{contentType.SchemaId}{MediaWithCropsModelSuffix}";
                        return await CreateContentTypeSchema(
                            schemaId,
                            PublishedItemType.Media,
                            contentType,
                            derivedTypeSchemas,
                            context,
                            cancellationToken);
                    },
                    cancellationToken);
                return;
        }
    }

    private async Task ApplyPolymorphicContentType(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        PublishedItemType itemType,
        IEnumerable<ContentTypeSchemaInfo> contentTypes,
        Func<ContentTypeSchemaInfo, List<IOpenApiSchema>, Task<OpenApiSchemaReference>> contentTypeSchemaMapper,
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
            OpenApiSchemaReference contentTypeSchema = await contentTypeSchemaMapper(contentType, derivedTypeSchemas);
            schema.Discriminator.Mapping[contentType.Alias] = contentTypeSchema;
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
            // Log the error but continue with a fallback schema to avoid failing the entire document generation.
            // The fallback schema includes a description indicating the failure, making it visible to API consumers.
            _logger.LogError(ex, "Failed to create OpenAPI schema for type {TypeName}", jsonTypeInfo.Type.FullName);
            schema = new OpenApiSchema
            {
                Description = $"[Schema generation failed for type '{jsonTypeInfo.Type.FullName}'. See server logs for details.]",
            };
        }

        configureSchema?.Invoke(schema);

        if (schemaId is null)
        {
            return schema;
        }

        OpenApiDocument document = context.GetRequiredDocument();
        document.AddComponent(schemaId, schema);
        return new OpenApiSchemaReference(schemaId, document);
    }

    private async Task<OpenApiSchemaReference> CreateContentTypeSchema(
        string schemaId,
        PublishedItemType itemType,
        ContentTypeSchemaInfo contentType,
        List<IOpenApiSchema> derivedTypeSchemas,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var typePropertyName = GetTypePropertyName(itemType);
        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                [typePropertyName] = new OpenApiSchema { Const = contentType.Alias },
                ["properties"] = await CreatePropertiesSchema(contentType, context, cancellationToken),
            },
            Required = new HashSet<string> { typePropertyName },
            AdditionalPropertiesAllowed = false,
            AllOf = derivedTypeSchemas.Count > 0 ? derivedTypeSchemas : null,
            Metadata = new Dictionary<string, object> { [SchemaIdMetadataKey] = schemaId, },
        };

        OpenApiDocument document = context.GetRequiredDocument();
        document.AddComponent(schemaId, schema);
        return new OpenApiSchemaReference(schemaId, document);
    }

    private async Task<OpenApiSchemaReference> CreatePropertiesSchema(
        ContentTypeSchemaInfo contentType,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var schemaId = $"{contentType.SchemaId}{PropertiesModelSuffix}";

        var propertiesSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            AllOf =
            [
                ..contentType.CompositionSchemaIds.Select(compositionSchemaId
                    => GetPlaceholderSchema($"{compositionSchemaId}{PropertiesModelSuffix}"))
            ],
            Properties = await ContentTypePropertiesMapper(contentType, context, cancellationToken),
            Metadata = new Dictionary<string, object> { [SchemaIdMetadataKey] = schemaId },
            AdditionalPropertiesAllowed = false,
        };

        OpenApiDocument document = context.GetRequiredDocument();
        document.AddComponent(schemaId, propertiesSchema);
        return new OpenApiSchemaReference(schemaId, document);
    }

    private async Task<Dictionary<string, IOpenApiSchema>> ContentTypePropertiesMapper(
        ContentTypeSchemaInfo contentType,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var properties = new Dictionary<string, IOpenApiSchema>();
        foreach (ContentTypePropertySchemaInfo propertyInfo in contentType.Properties.Where(p => !p.Inherited))
        {
            IOpenApiSchema schema = await CreateSchema(
                GetJsonTypeInfo(propertyInfo.DeliveryApiClrType),
                context,
                cancellationToken,
                // All properties can be null (e.g., when a property is added but content has not been republished)
                schema => schema.Type |= JsonSchemaType.Null);

            properties[propertyInfo.Alias] = schema;
        }

        return properties;
    }

    private JsonTypeInfo GetJsonTypeInfo(Type type)
    {
        JsonTypeInfo? jsonTypeInfo = _jsonTypeInfoResolver.GetTypeInfo(type, _serializerOptions);
        return jsonTypeInfo ?? throw new InvalidOperationException("Could not get JsonTypeInfo for type " + type.FullName);
    }

    private string GetTypePropertyName(PublishedItemType itemType)
    {
        var propertyName = itemType switch
        {
            PublishedItemType.Content => nameof(IApiElement.ContentType),
            PublishedItemType.Media => nameof(IApiMedia.MediaType),
            _ => throw new NotSupportedException($"Unsupported PublishedItemType: {itemType}"),
        };

        return _serializerOptions.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName;
    }

    private static string? GetSchemaId(JsonTypeInfo type)
        => ConfigureUmbracoOpenApiOptionsBase.CreateSchemaReferenceId(type);

    /// <summary>
    /// Creates a temporary placeholder schema to break circular reference chains during schema generation.
    /// </summary>
    /// <remarks>
    /// The placeholder contains metadata with the target schema ID. During the document transformation phase,
    /// <see cref="ReplacePlaceholderSchemas(OpenApiDocument, IOpenApiSchema)"/> will replace these placeholders with actual schema references.
    /// </remarks>
    /// <param name="schemaId">The ID of the schema this placeholder represents.</param>
    /// <returns>A placeholder schema with metadata indicating the target schema reference.</returns>
    private static OpenApiSchema GetPlaceholderSchema(string schemaId)
        => new()
        {
            Metadata = new Dictionary<string, object>
            {
                [RecursiveRefMetadataKey] = schemaId,
            },
        };

    /// <summary>
    /// Recursively replaces placeholder schemas with proper <c>$ref</c> references to the actual schemas.
    /// </summary>
    /// <remarks>
    /// This method is called during the document transformation phase (after all schemas have been generated).
    /// It walks through all schema properties, allOf, oneOf, and anyOf collections, looking for placeholders
    /// created by <see cref="GetPlaceholderSchema"/>. Each placeholder is replaced with an
    /// <see cref="OpenApiSchemaReference"/> pointing to the actual schema in the document's components.
    /// </remarks>
    /// <param name="document">The OpenAPI document containing the registered schema components.</param>
    /// <param name="schema">The schema to process (will be modified in place).</param>
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
    private static IOpenApiSchema? GetActualSchemaOrReference(
        OpenApiDocument document,
        IOpenApiSchema? schema,
        out bool replaced)
    {
        if (schema is not OpenApiSchema openApiSchema)
        {
            replaced = false;
            return schema;
        }

        // Check if this is a placeholder schema
        if (openApiSchema.Metadata?.TryGetValue(RecursiveRefMetadataKey, out var recursiveRefIdObj) != true
            || recursiveRefIdObj is not string recursiveRefId)
        {
            replaced = false;
            return schema;
        }

        // Return the actual schema
        replaced = true;
        return new OpenApiSchemaReference(recursiveRefId, document);
    }
}
