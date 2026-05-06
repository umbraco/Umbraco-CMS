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
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

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
/// resolves inline schemas into proper <c>$ref</c> references. This handles two cases:
/// <list type="bullet">
///   <item>Circular reference placeholders (marked with <c>x-recursive-ref</c>) created during Phase 1</item>
///   <item>Componentized schemas (marked with <c>x-schema-id</c>) that the framework did not automatically
///   resolve to <c>$ref</c> — this can happen for schemas reached through properties or composition
///   rather than as direct API response types</item>
/// </list>
/// This is done by <see cref="ResolveSchemaReferences(OpenApiDocument, IOpenApiSchema)"/> which recursively walks
/// through all schemas and substitutes matching entries with <see cref="OpenApiSchemaReference"/> instances.
/// </para>
/// </remarks>
public sealed class ContentTypeSchemaTransformer : IOpenApiSchemaTransformer, IOpenApiDocumentTransformer
{
    // Metadata keys
    private const string RecursiveRefMetadataKey = "x-recursive-ref";
    private const string SchemaIdMetadataKey = "x-schema-id";

    // Schema ID suffixes
    private const string ResponseModelSuffix = "ResponseModel";
    private const string ModelSuffix = "Model";
    private const string ContentSuffix = "Content";
    private const string ElementSuffix = "Element";
    private const string MediaSuffix = "Media";
    private const string MediaWithCropsSuffix = "MediaWithCrops";
    private const string PropertiesModelSuffix = "PropertiesModel";

    private readonly IContentTypeSchemaService _contentTypeSchemaService;
    private readonly IOptionsMonitor<DeliveryApiSettings> _deliveryApiSettings;
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
    /// <param name="deliveryApiSettings">The Delivery API settings, used to honour the allow/deny content type list.</param>
    /// <param name="logger">The logger.</param>
    public ContentTypeSchemaTransformer(
        IContentTypeSchemaService contentTypeSchemaService,
        IOptionsMonitor<JsonOptions> jsonOptionsMonitor,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings,
        ILogger<ContentTypeSchemaTransformer> logger)
    {
        _contentTypeSchemaService = contentTypeSchemaService;
        _deliveryApiSettings = deliveryApiSettings;
        _logger = logger;
        _serializerOptions = jsonOptionsMonitor
            .Get(Constants.JsonOptionsNames.DeliveryApi)
            .SerializerOptions;
        _jsonTypeInfoResolver = _serializerOptions.TypeInfoResolver
                                ?? throw new InvalidOperationException("The JSON serializer options must have a TypeInfoResolver configured.");
    }

    private IReadOnlyCollection<ContentTypeSchemaInfo> DocumentTypes
        => field ??= FilterAllowedDocumentTypes(_contentTypeSchemaService.GetDocumentTypes());

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

        foreach ((var schemaId, IOpenApiSchema componentsSchema) in document.Components.Schemas)
        {
            ResolveSchemaReferences(document, componentsSchema);
            FixAutoBuiltDiscriminatorMapping(document, schemaId, componentsSchema);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Repairs broken discriminator mapping refs auto-built by the framework for polymorphic types.
    /// </summary>
    /// <remarks>
    /// The framework prefixes each ref with the base schema id, but the derived schemas are
    /// registered without that prefix. Stripping the prefix recovers the correct ref.
    /// </remarks>
    private static void FixAutoBuiltDiscriminatorMapping(OpenApiDocument document, string parentSchemaId, IOpenApiSchema schema)
    {
        if (schema is not OpenApiSchema concrete
            || concrete.Discriminator?.Mapping is not { } mapping
            || document.Components?.Schemas is not { } schemas)
        {
            return;
        }

        foreach ((var key, OpenApiSchemaReference currentRef) in mapping.ToList())
        {
            var targetId = currentRef.Reference.Id;
            if (string.IsNullOrEmpty(targetId) || schemas.ContainsKey(targetId))
            {
                continue;
            }

            if (targetId.StartsWith(parentSchemaId, StringComparison.Ordinal) is false)
            {
                continue;
            }

            var stripped = targetId[parentSchemaId.Length..];
            if (schemas.ContainsKey(stripped))
            {
                mapping[key] = new OpenApiSchemaReference(stripped, document);
            }
        }
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
                        var schemaIdPrefix = $"{contentType.SchemaId}{ContentSuffix}";
                        return await CreateContentTypeResponseSchema(
                            schemaIdPrefix,
                            derivedTypeSchemas,
                            context);
                    },
                    cancellationToken);
                await CreateSchema(GetJsonTypeInfo(typeof(IApiContent)), context, cancellationToken);
                return;
            case var type when type == typeof(IApiContent):
                await ApplyPolymorphicContentType(
                    schema,
                    context,
                    PublishedItemType.Content,
                    DocumentTypes.Where(c => !c.IsElement),
                    async (contentType, derivedTypeSchemas) =>
                    {
                        var schemaId = $"{contentType.SchemaId}{ContentSuffix}{ModelSuffix}";
                        return await CreateContentTypeSchema(
                            schemaId,
                            PublishedItemType.Content,
                            contentType,
                            derivedTypeSchemas,
                            context,
                            cancellationToken);
                    },
                    cancellationToken);
                await CreateSchema(GetJsonTypeInfo(typeof(IApiElement)), context, cancellationToken);
                return;
            case var type when type == typeof(IApiElement):
                await ApplyPolymorphicContentType(
                    schema,
                    context,
                    PublishedItemType.Content,
                    DocumentTypes.Where(c => c.IsElement),
                    async (contentType, derivedTypeSchemas) =>
                    {
                        var schemaId = $"{contentType.SchemaId}{ElementSuffix}{ModelSuffix}";
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
                        var schemaId = $"{contentType.SchemaId}{MediaWithCropsSuffix}";
                        return await CreateContentTypeResponseSchema(
                            schemaId,
                            derivedTypeSchemas,
                            context);
                    },
                    cancellationToken);
                await CreateSchema(GetJsonTypeInfo(typeof(IApiMediaWithCrops)), context, cancellationToken);
                return;
            case var type when type == typeof(IApiMediaWithCrops):
                await ApplyPolymorphicContentType(
                    schema,
                    context,
                    PublishedItemType.Media,
                    MediaTypes,
                    async (contentType, derivedTypeSchemas) =>
                    {
                        var schemaId = $"{contentType.SchemaId}{MediaWithCropsSuffix}{ModelSuffix}";
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
            default:
                // HACK: Some types with circular references (e.g. ApiBlockGridItem) get left
                // inlined by the framework, breaking $ref resolution. Register them explicitly.
                if (GetSchemaId(context.JsonTypeInfo) is not { } schemaId || !_handledSchemas.Add(schemaId))
                {
                    return;
                }

                OpenApiDocument document = context.GetRequiredDocument();
                document.AddComponent(schemaId, schema);
                return;
        }
    }

    private async Task ApplyPolymorphicContentType(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        PublishedItemType itemType,
        IEnumerable<ContentTypeSchemaInfo> contentTypes,
        Func<ContentTypeSchemaInfo, List<IOpenApiSchema>, Task<OpenApiSchema>> contentTypeSchemaFactory,
        CancellationToken cancellationToken)
    {
        List<IOpenApiSchema> derivedTypeSchemas = await ResolveDerivedTypeSchemas(
            schema,
            context,
            cancellationToken);

        OpenApiDocument document = context.GetRequiredDocument();
        var typePropertyName = GetTypePropertyName(itemType);
        schema.Discriminator = new OpenApiDiscriminator
        {
            PropertyName = typePropertyName,
            Mapping = new Dictionary<string, OpenApiSchemaReference>(),
        };
        schema.OneOf ??= new List<IOpenApiSchema>();

        foreach (ContentTypeSchemaInfo contentType in contentTypes)
        {
            OpenApiSchema contentTypeSchema = await contentTypeSchemaFactory(contentType, derivedTypeSchemas);
            var schemaId = (string)contentTypeSchema.Metadata![SchemaIdMetadataKey];
            schema.Discriminator.Mapping[contentType.Alias] = new OpenApiSchemaReference(schemaId, document);
            schema.OneOf.Add(contentTypeSchema);
        }

        // Remove all schema properties that are now handled by the derived types
        schema.AnyOf = null;
        schema.Properties = null;
        schema.Required = new HashSet<string> { typePropertyName };
    }

    /// <summary>
    /// Creates and adds a schema to the OpenAPI document if it does not already exist.
    /// </summary>
    /// <remarks>A placeholder schema is added first to avoid recursion issues when generating schemas that reference themselves.</remarks>
    private async Task<IOpenApiSchema> CreateSchema(
        JsonTypeInfo jsonTypeInfo,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (jsonTypeInfo.Type.IsArray || jsonTypeInfo.Kind == JsonTypeInfoKind.Enumerable)
        {
            Type elementType = jsonTypeInfo.ElementType ?? jsonTypeInfo.Type.GetElementType() ?? typeof(object);
            JsonTypeInfo elementJsonTypeInfo = GetJsonTypeInfo(elementType);
            IOpenApiSchema itemSchema = await CreateSchema(elementJsonTypeInfo, context, cancellationToken);
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Array,
                Items = itemSchema,
            };
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

        if (schemaId is null)
        {
            return schema;
        }

        OpenApiDocument document = context.GetRequiredDocument();
        document.AddComponent(schemaId, schema);
        return new OpenApiSchemaReference(schemaId, document);
    }

    /// <summary>
    /// Allows null at a property reference site without mutating any shared component schema.
    /// Inline schemas have <c>null</c> OR-ed into their <c>type</c> flags; schema references and
    /// recursive-ref placeholders are wrapped in a <c>oneOf</c> with an explicit null branch so the
    /// shared component is left unchanged.
    /// </summary>
    private static IOpenApiSchema AsNullable(IOpenApiSchema schema)
    {
        if (schema is OpenApiSchema inline
            && inline.Metadata?.ContainsKey(RecursiveRefMetadataKey) is not true)
        {
            inline.Type |= JsonSchemaType.Null;
            return inline;
        }

        return new OpenApiSchema
        {
            OneOf =
            [
                schema,
                new OpenApiSchema { Type = JsonSchemaType.Null },
            ],
        };
    }

    private static Task<OpenApiSchema> CreateContentTypeResponseSchema(
        string schemaIdPrefix,
        List<IOpenApiSchema> derivedTypeSchemas,
        OpenApiSchemaTransformerContext context)
    {
        var schemaId = $"{schemaIdPrefix}{ResponseModelSuffix}";
        OpenApiDocument document = context.GetRequiredDocument();
        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            AllOf = [..derivedTypeSchemas, new OpenApiSchemaReference($"{schemaIdPrefix}{ModelSuffix}", document)],
            Metadata = new Dictionary<string, object> { [SchemaIdMetadataKey] = schemaId },
        };

        document.AddComponent(schemaId, schema);
        return Task.FromResult(schema);
    }

    private async Task<OpenApiSchema> CreateContentTypeSchema(
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
                ["properties"] = await CreatePropertiesSchema(contentType, itemType, context, cancellationToken),
            },
            Required = new HashSet<string> { typePropertyName },
            AllOf = derivedTypeSchemas.Count > 0 ? derivedTypeSchemas : null,
            Metadata = new Dictionary<string, object> { [SchemaIdMetadataKey] = schemaId, },
        };

        OpenApiDocument document = context.GetRequiredDocument();
        document.AddComponent(schemaId, schema);
        return schema;
    }

    private async Task<OpenApiSchemaReference> CreatePropertiesSchema(
        ContentTypeSchemaInfo contentType,
        PublishedItemType itemType,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var schemaId = GetPropertiesModelSchemaId(contentType, itemType);

        var propertiesSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            AllOf =
            [
                ..contentType.CompositionSchemaIds.Select(compositionSchemaId
                    => GetPlaceholderSchema(GetCompositionPropertiesModelSchemaId(compositionSchemaId, itemType)))
            ],
            Properties = await CreateContentTypeProperties(contentType, context, cancellationToken),
            Metadata = new Dictionary<string, object> { [SchemaIdMetadataKey] = schemaId },
        };

        OpenApiDocument document = context.GetRequiredDocument();
        document.AddComponent(schemaId, propertiesSchema);
        return new OpenApiSchemaReference(schemaId, document);
    }

    private static string GetPropertiesModelSchemaId(ContentTypeSchemaInfo contentType, PublishedItemType itemType) =>
        $"{contentType.SchemaId}{GetItemTypeSuffix(itemType, contentType.IsElement)}{PropertiesModelSuffix}";

    private string GetCompositionPropertiesModelSchemaId(string compositionSchemaId, PublishedItemType itemType)
    {
        // Look up the composition's own IsElement so its reference points at the right
        // generated schema (element-type compositions live under the Element suffix).
        IReadOnlyCollection<ContentTypeSchemaInfo> candidates = itemType == PublishedItemType.Media ? MediaTypes : DocumentTypes;
        ContentTypeSchemaInfo? composition = candidates.FirstOrDefault(c => c.SchemaId == compositionSchemaId);
        var suffix = GetItemTypeSuffix(itemType, composition?.IsElement ?? false);
        return $"{compositionSchemaId}{suffix}{PropertiesModelSuffix}";
    }

    private static string GetItemTypeSuffix(PublishedItemType itemType, bool isElement) =>
        itemType switch
        {
            PublishedItemType.Media => MediaSuffix,
            PublishedItemType.Content => isElement ? ElementSuffix : ContentSuffix,
            _ => throw new NotSupportedException($"Unsupported PublishedItemType: {itemType}"),
        };

    private async Task<Dictionary<string, IOpenApiSchema>> CreateContentTypeProperties(
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
                cancellationToken);

            // Properties may be null (e.g. property added after content was last published).
            // Nullability is applied at the reference site, never on a shared component schema.
            properties[propertyInfo.Alias] = AsNullable(schema);
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
    /// <see cref="ResolveSchemaReferences(OpenApiDocument, IOpenApiSchema)"/> will replace these placeholders with actual schema references.
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
    /// Recursively resolves inline schemas into proper <c>$ref</c> references.
    /// </summary>
    /// <remarks>
    /// This method is called during the document transformation phase (after all schemas have been generated).
    /// It walks through all schema properties, allOf, oneOf, and anyOf collections, resolving two types of
    /// inline schemas:
    /// <list type="bullet">
    ///   <item>Circular reference placeholders created by <see cref="GetPlaceholderSchema"/> (marked with <c>x-recursive-ref</c>)</item>
    ///   <item>Componentized schemas that should be references (marked with <c>x-schema-id</c>)</item>
    /// </list>
    /// Each match is replaced with an <see cref="OpenApiSchemaReference"/> pointing to the actual schema in the document's components.
    /// </remarks>
    /// <param name="document">The OpenAPI document containing the registered schema components.</param>
    /// <param name="schema">The schema to process (will be modified in place).</param>
    private static void ResolveSchemaReferences(OpenApiDocument document, IOpenApiSchema schema)
    {
        // Replace in allOf, oneOf, anyOf
        ResolveSchemaReferences(document, schema.AllOf);
        ResolveSchemaReferences(document, schema.OneOf);
        ResolveSchemaReferences(document, schema.AnyOf);

        // Process array items
        if (schema is OpenApiSchema { Items: OpenApiSchema itemsSchema } parentSchema)
        {
            parentSchema.Items = GetActualSchemaOrReference(document, itemsSchema, out var itemsReplaced);
            if (!itemsReplaced)
            {
                ResolveSchemaReferences(document, itemsSchema);
            }
        }

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
                continue;
            }

            // Recursive call to handle the property schema
            ResolveSchemaReferences(document, innerSchema);
        }
    }

    private static void ResolveSchemaReferences(OpenApiDocument document, IList<IOpenApiSchema>? schemas)
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
                ResolveSchemaReferences(document, schemas[i]);
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

        // Check if this is a placeholder schema (circular reference)
        if (openApiSchema.Metadata?.TryGetValue(RecursiveRefMetadataKey, out var recursiveRefIdObj) == true
            && recursiveRefIdObj is string recursiveRefId)
        {
            replaced = true;
            return new OpenApiSchemaReference(recursiveRefId, document);
        }

        // Check if this is a componentized schema that should be a $ref
        // Only resolve if the component actually exists — the framework also sets x-schema-id on
        // schemas that may not end up as components.
        if (openApiSchema.Metadata?.TryGetValue(SchemaIdMetadataKey, out var schemaIdObj) == true
            && schemaIdObj is string schemaId
            && !string.IsNullOrEmpty(schemaId)
            && document.Components?.Schemas?.ContainsKey(schemaId) == true)
        {
            replaced = true;
            return new OpenApiSchemaReference(schemaId, document);
        }

        replaced = false;
        return schema;
    }

    private IReadOnlyCollection<ContentTypeSchemaInfo> FilterAllowedDocumentTypes(IReadOnlyCollection<ContentTypeSchemaInfo> documentTypes)
    {
        DeliveryApiSettings settings = _deliveryApiSettings.CurrentValue;
        return documentTypes
            .Where(c => settings.IsAllowedContentType(c.Alias))
            .ToList();
    }

    /// <summary>
    /// Returns the schemas to use as the <c>allOf</c> bases for each typed content type
    /// schema in a polymorphic union. Prefers concrete derived types declared on the
    /// interface via <c>[JsonDerivedType]</c>; when none are advertised, falls back to a
    /// schema built from the interface's own properties.
    /// </summary>
    /// <remarks>
    /// The fallback exists for media interfaces, whose concrete classes are internal in
    /// Umbraco.Infrastructure and therefore cannot be referenced via <c>[JsonDerivedType]</c>
    /// from Umbraco.Core.
    /// </remarks>
    private async Task<List<IOpenApiSchema>> ResolveDerivedTypeSchemas(
        OpenApiSchema interfaceSchema,
        OpenApiSchemaTransformerContext context,
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

        if (derivedTypeSchemas.Count == 0)
        {
            derivedTypeSchemas.Add(CreateBaseSchemaFromInterface(interfaceSchema, context));
        }

        return derivedTypeSchemas;
    }

    private static IOpenApiSchema CreateBaseSchemaFromInterface(
        OpenApiSchema interfaceSchema,
        OpenApiSchemaTransformerContext context)
    {
        // Append a "Base" marker so this schema stays distinct from the polymorphic union
        // schema for the same interface (e.g. IApiMediaWithCropsResponseBaseModel vs.
        // IApiMediaWithCropsResponseModel).
        var baseSchemaId = $"{context.JsonTypeInfo.Type.Name}Base{ModelSuffix}";
        OpenApiDocument document = context.GetRequiredDocument();
        var baseSchema = new OpenApiSchema
        {
            Type = interfaceSchema.Type,
            Properties = interfaceSchema.Properties,
            Required = interfaceSchema.Required,
            Metadata = new Dictionary<string, object> { [SchemaIdMetadataKey] = baseSchemaId },
        };

        document.AddComponent(baseSchemaId, baseSchema);
        return new OpenApiSchemaReference(baseSchemaId, document);
    }
}
