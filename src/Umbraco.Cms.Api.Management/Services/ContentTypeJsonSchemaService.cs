using System.Text.Json.Nodes;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Provides services for generating JSON Schema for content types.
/// </summary>
internal sealed class ContentTypeJsonSchemaService : IContentTypeJsonSchemaService
{
    private const string JsonSchemaVersion = "https://json-schema.org/draft/2020-12/schema";

    private readonly IContentTypeService _contentTypeService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly IPropertyEditorSchemaService _propertyEditorSchemaService;
    private readonly IManagementApiRouteBuilder _routeBuilder;
    private readonly IDataTypeService _dataTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeJsonSchemaService"/> class.
    /// </summary>
    public ContentTypeJsonSchemaService(
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IMemberTypeService memberTypeService,
        IPropertyEditorSchemaService propertyEditorSchemaService,
        IManagementApiRouteBuilder routeBuilder,
        IDataTypeService dataTypeService)
    {
        _contentTypeService = contentTypeService;
        _mediaTypeService = mediaTypeService;
        _memberTypeService = memberTypeService;
        _propertyEditorSchemaService = propertyEditorSchemaService;
        _routeBuilder = routeBuilder;
        _dataTypeService = dataTypeService;
    }

    /// <inheritdoc />
    public async Task<JsonObject?> GetDocumentTypeSchemaAsync(Guid key)
    {
        IContentType? contentType = await _contentTypeService.GetAsync(key);
        return contentType is null ? null : await BuildSchemaAsync(contentType, "document");
    }

    /// <inheritdoc />
    public async Task<JsonObject?> GetMediaTypeSchemaAsync(Guid key)
    {
        IMediaType? mediaType = await _mediaTypeService.GetAsync(key);
        return mediaType is null ? null : await BuildSchemaAsync(mediaType, "media");
    }

    /// <inheritdoc />
    public async Task<JsonObject?> GetMemberTypeSchemaAsync(Guid key)
    {
        IMemberType? memberType = await _memberTypeService.GetAsync(key);
        return memberType is null ? null : await BuildSchemaAsync(memberType, "member");
    }

    private async Task<JsonObject> BuildSchemaAsync(IContentTypeComposition contentType, string contentKind)
    {
        var schema = new JsonObject
        {
            ["$schema"] = JsonSchemaVersion,
            ["$id"] = $"urn:umbraco:{contentKind}-type:{contentType.Key}",
            ["title"] = $"Create {contentType.Name}",
            ["description"] = $"JSON Schema for creating/updating {contentKind} content of type '{contentType.Alias}'",
            ["type"] = "object",
            ["required"] = new JsonArray($"{contentKind}Type", "values", "variants"),
        };

        // Build properties
        var properties = new JsonObject
        {
            [$"{contentKind}Type"] = BuildContentTypeReference(contentType.Key),
            ["id"] = new JsonObject
            {
                ["type"] = new JsonArray("string", "null"),
                ["format"] = "uuid",
                ["description"] = "Optional key for the new content item",
            },
            ["values"] = await BuildValuesSchemaAsync(contentType),
            ["variants"] = new JsonObject { ["$ref"] = "#/$defs/variants" },
        };

        // Add parent reference for documents and media (not members)
        if (contentKind is "document" or "media")
        {
            properties["parent"] = new JsonObject { ["$ref"] = "#/$defs/referenceById" };
        }

        // Add template reference for documents only
        if (contentKind == "document")
        {
            properties["template"] = new JsonObject { ["$ref"] = "#/$defs/referenceById" };
        }

        schema["properties"] = properties;

        // Build $defs
        schema["$defs"] = BuildDefs();

        // Build x-umbraco-content-type metadata
        schema["x-umbraco-content-type"] = new JsonObject
        {
            ["key"] = contentType.Key.ToString(),
            ["alias"] = contentType.Alias,
            ["isElement"] = contentType.IsElement,
            ["variations"] = contentType.Variations.ToString(),
        };

        return schema;
    }

    private static JsonObject BuildContentTypeReference(Guid contentTypeKey)
        => new()
        {
            ["type"] = "object",
            ["required"] = new JsonArray("id"),
            ["properties"] = new JsonObject
            {
                ["id"] = new JsonObject
                {
                    ["const"] = contentTypeKey.ToString(),
                    ["description"] = "Content type identifier",
                },
            },
        };

    private async Task<JsonObject> BuildValuesSchemaAsync(IContentTypeComposition contentType)
    {
        IPropertyType[] propertyTypes = contentType.CompositionPropertyTypes.ToArray();

        // Build the items schema with if/then clauses for each property
        var itemsAllOf = new JsonArray
        {
            new JsonObject { ["$ref"] = "#/$defs/valueBase" },
        };

        // Add if/then clause for each property that supports schema
        foreach (IPropertyType propertyType in propertyTypes)
        {
            JsonObject ifThen = BuildPropertyIfThen(propertyType);
            itemsAllOf.Add(ifThen);
        }

        // get all relevant datatypes
        IDataType[] dataTypes = (await _dataTypeService.GetAllAsync(propertyTypes.Select(propertyType => propertyType.DataTypeKey).Distinct()
            .ToArray())).ToArray();

        // Index by key for O(1) lookup
        Dictionary<Guid, IDataType> dataTypesByKey = dataTypes.ToDictionary(dt => dt.Key);

        // Build x-umbraco-properties metadata
        var propertiesMetadata = new JsonObject();
        foreach (IPropertyType propertyType in propertyTypes)
        {
            propertiesMetadata[propertyType.Alias] = new JsonObject
            {
                ["dataTypeId"] = propertyType.DataTypeKey.ToString(),
                ["editorAlias"] = propertyType.PropertyEditorAlias,
                ["editorUiAlias"] = dataTypesByKey.GetValueOrDefault(propertyType.DataTypeKey)?.EditorUiAlias,
                ["mandatory"] = propertyType.Mandatory,
                ["variations"] = propertyType.Variations.ToString(),
            };
        }

        return new JsonObject
        {
            ["type"] = "array",
            ["description"] = "Property values for the content item",
            ["items"] = new JsonObject { ["allOf"] = itemsAllOf },
            ["x-umbraco-properties"] = propertiesMetadata,
        };
    }

    private JsonObject BuildPropertyIfThen(IPropertyType propertyType)
    {
        var thenProperties = new JsonObject();

        // Check if the property editor supports schema
        if (_propertyEditorSchemaService.SupportsSchema(propertyType.PropertyEditorAlias))
        {
            // Add $ref to the data type schema endpoint
            var schemaPath = _routeBuilder.GetPathByAction<SchemaDataTypeController>(
                c => nameof(c.Schema),
                new { id = propertyType.DataTypeKey });

            thenProperties["value"] = new JsonObject
            {
                ["$ref"] = schemaPath,
            };
        }

        // If no schema support, value can be anything (already defined in valueBase)

        return new JsonObject
        {
            ["if"] = new JsonObject
            {
                ["properties"] = new JsonObject
                {
                    ["alias"] = new JsonObject { ["const"] = propertyType.Alias },
                },
            },
            ["then"] = new JsonObject { ["properties"] = thenProperties },
        };
    }

    private static JsonObject BuildDefs()
        => new()
        {
            ["referenceById"] = new JsonObject
            {
                ["oneOf"] = new JsonArray
                {
                    new JsonObject { ["type"] = "null" },
                    new JsonObject
                    {
                        ["type"] = "object",
                        ["required"] = new JsonArray("id"),
                        ["properties"] = new JsonObject
                        {
                            ["id"] = new JsonObject
                            {
                                ["type"] = "string",
                                ["format"] = "uuid",
                            },
                        },
                    },
                },
            },
            ["valueBase"] = new JsonObject
            {
                ["type"] = "object",
                ["required"] = new JsonArray("alias"),
                ["properties"] = new JsonObject
                {
                    ["alias"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "Property type alias",
                    },
                    ["culture"] = new JsonObject
                    {
                        ["type"] = new JsonArray("string", "null"),
                        ["description"] = "Culture code for variant properties, or null for invariant",
                    },
                    ["segment"] = new JsonObject
                    {
                        ["type"] = new JsonArray("string", "null"),
                        ["description"] = "Segment identifier, or null for non-segmented",
                    },
                    ["value"] = new JsonObject
                    {
                        ["description"] = "Property value (type depends on property editor)",
                    },
                },
            },
            ["variants"] = new JsonObject
            {
                ["type"] = "array",
                ["minItems"] = 1,
                ["description"] = "Content variants (at minimum one variant with the name is required)",
                ["items"] = new JsonObject
                {
                    ["type"] = "object",
                    ["required"] = new JsonArray("name"),
                    ["properties"] = new JsonObject
                    {
                        ["name"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["minLength"] = 1,
                            ["description"] = "Content name for this variant",
                        },
                        ["culture"] = new JsonObject
                        {
                            ["type"] = new JsonArray("string", "null"),
                            ["description"] = "Culture code for this variant, or null for invariant",
                        },
                        ["segment"] = new JsonObject
                        {
                            ["type"] = new JsonArray("string", "null"),
                            ["description"] = "Segment identifier, or null for non-segmented",
                        },
                    },
                },
            },
        };
}
