using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.References;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace Umbraco.POC.JsonSchemaGeneration;

public class JsonSchemaExporterService
{
    private readonly IContentTypeSchemaService _contentTypeSchemaService;
    private readonly ISchemaIdSelector _schemaIdSelector;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public JsonSchemaExporterService(
        IContentTypeSchemaService contentTypeSchemaService,
        ISchemaIdSelector schemaIdSelector,
        IOptionsMonitor<JsonOptions> options)
    {
        _contentTypeSchemaService = contentTypeSchemaService;
        _schemaIdSelector = schemaIdSelector;
        _jsonSerializerOptions = options.Get(Constants.JsonOptionsNames.DeliveryApi).JsonSerializerOptions;
    }

    public JsonSchema GenerateJsonSchema<T>()
    {
        var schema = JsonSchema.FromType<T>(
            new SystemTextJsonSchemaGeneratorSettings
            {
                SerializerOptions = _jsonSerializerOptions,
                SchemaNameGenerator = new MySchemaNameGenerator(_schemaIdSelector),
                SchemaProcessors =
                {
                    new MySchemaProcessor(_contentTypeSchemaService, _schemaIdSelector, _jsonSerializerOptions),
                },
            });
        return schema;
    }

    private class MySchemaNameGenerator : ISchemaNameGenerator
    {
        private readonly ISchemaIdSelector _schemaIdSelector;

        public MySchemaNameGenerator(ISchemaIdSelector schemaIdSelector)
        {
            _schemaIdSelector = schemaIdSelector;
        }

        public string Generate(Type type) => _schemaIdSelector.SchemaId(type);
    }

    private class MySchemaProcessor : ISchemaProcessor
    {
        private readonly IContentTypeSchemaService _contentTypeSchemaService;
        private readonly ISchemaIdSelector _schemaIdSelector;
        private readonly HashSet<Type> _handledSchemas = [];
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public MySchemaProcessor(
            IContentTypeSchemaService contentTypeSchemaService,
            ISchemaIdSelector schemaIdSelector,
            JsonSerializerOptions jsonSerializerOptions)
        {
            _contentTypeSchemaService = contentTypeSchemaService;
            _schemaIdSelector = schemaIdSelector;
            _jsonSerializerOptions = new JsonSerializerOptions(jsonSerializerOptions)
            {
                ReferenceHandler = ReferenceHandler.Preserve,
            };
        }

        public void Process(SchemaProcessorContext context)
        {
            switch (context.ContextualType.Type)
            {
                case var type when type == typeof(IApiContentResponse):
                    ApplyPolymorphicContentType(
                        context.Schema,
                        context,
                        PublishedItemType.Content,
                        _contentTypeSchemaService.GetDocumentTypes().Where(c => !c.IsElement).ToList(),
                        (contentType, derivedTypes) =>
                        {
                            var schemaId = $"{contentType.SchemaId}ContentResponseModel";
                            return GetOrCreateContentTypeSchema(schemaId, contentType, derivedTypes, context);
                        });
                    break;
                case var type when type == typeof(IApiContent):
                    ApplyPolymorphicContentType(
                        context.Schema,
                        context,
                        PublishedItemType.Content,
                        _contentTypeSchemaService.GetDocumentTypes().Where(c => !c.IsElement).ToList(),
                        (contentType, derivedTypes) =>
                        {
                            var schemaId = $"{contentType.SchemaId}ContentModel";
                            return GetOrCreateContentTypeSchema(schemaId, contentType, derivedTypes, context);
                        });
                    break;
                case var type when type == typeof(IApiElement):
                    ApplyPolymorphicContentType(
                        context.Schema,
                        context,
                        PublishedItemType.Content,
                        _contentTypeSchemaService.GetDocumentTypes().Where(c => c.IsElement).ToList(),
                        (contentType, derivedTypes) =>
                        {
                            var schemaId = $"{contentType.SchemaId}ElementModel";
                            return GetOrCreateContentTypeSchema(schemaId, contentType, derivedTypes, context);
                        });
                    break;
                case var type when type == typeof(IApiMediaWithCropsResponse):
                    ApplyPolymorphicContentType(
                        context.Schema,
                        context,
                        PublishedItemType.Media,
                        _contentTypeSchemaService.GetMediaTypes().ToList(),
                        (contentType, derivedTypes) =>
                        {
                            var schemaId = $"{contentType.SchemaId}MediaWithCropsResponseModel";
                            return GetOrCreateContentTypeSchema(schemaId, contentType, derivedTypes, context);
                        });
                    break;
                case var type when type == typeof(IApiMediaWithCrops):
                    ApplyPolymorphicContentType(
                        context.Schema,
                        context,
                        PublishedItemType.Media,
                        _contentTypeSchemaService.GetMediaTypes().ToList(),
                        (contentType, derivedTypes) =>
                        {
                            var schemaId = $"{contentType.SchemaId}MediaWithCropsModel";
                            return GetOrCreateContentTypeSchema(schemaId, contentType, derivedTypes, context);
                        });
                    break;
            }
        }

        private void ApplyPolymorphicContentType(
            JsonSchema schema,
            SchemaProcessorContext context,
            PublishedItemType itemType,
            List<ContentTypeSchemaInfo> contentTypes,
            Func<ContentTypeSchemaInfo, List<JsonSchema>, JsonSchema> getOrCreateContentTypeSchema)
        {
            // Clear the schema
            schema.Properties.Clear();
            schema.RequiredProperties.Clear();
            schema.AnyOf.Clear();
            schema.OneOf.Clear();

            // Set up discriminator (not sure if supported by json schema or only Open API).
            var typePropertyName = GetTypePropertyName(itemType);
            schema.DiscriminatorObject = new OpenApiDiscriminator { PropertyName = typePropertyName };
            schema.RequiredProperties.Add(typePropertyName);

            // Get the current type derived types, in order to make the new types extend from them.
            JsonTypeInfo jsonTypeInfo = GetJsonTypeInfo(context.ContextualType.Type);
            List<JsonSchema> derivedTypeSchemas = jsonTypeInfo.PolymorphismOptions?.DerivedTypes
                .Select(derivedType => (JsonSchema) GetOrCreateSchema(derivedType.DerivedType, context))
                .ToList() ?? [];

            // Generate the schema for each content type, and set it as oneOf of the main schema.
            foreach (ContentTypeSchemaInfo contentType in contentTypes)
            {
                JsonSchema contentTypeSchema = getOrCreateContentTypeSchema(contentType, derivedTypeSchemas);
                schema.DiscriminatorObject.Mapping[contentType.Alias] = new JsonSchema { Reference = contentTypeSchema };
                schema.OneOf.Add(contentTypeSchema);
            }
        }

        private JsonSchemaProperty GetOrCreateSchema(
            Type type,
            SchemaProcessorContext context)
        {
            JsonTypeInfo jsonTypeInfo = GetJsonTypeInfo(type);

            // If the type is an array or enumerable, call this function with the inner type and manually return an
            // array json schema. This is another measure to avoid circular reference issues.
            if (jsonTypeInfo.Type.IsArray || jsonTypeInfo.Kind == JsonTypeInfoKind.Enumerable)
            {
                Type elementType = jsonTypeInfo.ElementType ?? jsonTypeInfo.Type.GetElementType() ?? typeof(object);
                JsonSchema itemSchema = GetOrCreateSchema(elementType, context);
                var arraySchema = new JsonSchemaProperty
                {
                    Type = JsonObjectType.Array,
                    Items = { itemSchema },
                };
                return arraySchema;
            }

            // If this is an object type (not a simple type) and we know we are already handling it, return a reference.
            // This avoids circular reference issues.
            if (jsonTypeInfo.Kind == JsonTypeInfoKind.Object && !_handledSchemas.Add(type))
            {
                var schemaId = GetSchemaId(jsonTypeInfo);
                var reference = new JsonSchemaProperty();
                ((IJsonReferenceBase)reference).ReferencePath = $"#/definitions/{schemaId}";
                return reference;
            }

            JsonSchemaProperty schema = context.Generator.GenerateWithReference<JsonSchemaProperty>(type.ToContextualType(), context.Resolver);
            return schema;
        }

        private static JsonSchemaProperty GetOrCreateSchema(
            string schemaId,
            SchemaProcessorContext context,
            Func<JsonSchema> factory)
        {
            var rootSchema = context.Resolver.RootObject as JsonSchema;
            if (rootSchema is not null && rootSchema.Definitions.TryGetValue(schemaId, out JsonSchema? existingSchema))
            {
                return new JsonSchemaProperty { Reference = existingSchema };
            }

            JsonSchema schema = factory();

            // Check again if a reference has already been added in the meantime, to avoid adding it multiple times
            // while going through the properties recursively.
            if (rootSchema is not null && rootSchema.Definitions.TryGetValue(schemaId, out JsonSchema? existingSchema2))
            {
                return new JsonSchemaProperty { Reference = existingSchema2 };
            }

            context.Resolver.AppendSchema(schema, schemaId);
            return new JsonSchemaProperty { Reference = schema };
        }

        private JsonSchema GetOrCreateContentTypeSchema(
            string schemaId,
            ContentTypeSchemaInfo contentType,
            List<JsonSchema> derivedTypes,
            SchemaProcessorContext context) =>
            GetOrCreateSchema(
                schemaId,
                context,
                () =>
                {
                    var schema = new JsonSchema
                    {
                        Type = JsonObjectType.Object,
                        AllowAdditionalProperties = false,
                        Properties = { ["properties"] = CreatePropertiesSchema(contentType, context) },
                    };

                    foreach (JsonSchema derivedType in derivedTypes)
                    {
                        schema.AllOf.Add(derivedType);
                    }

                    return schema;
                });

        private JsonSchemaProperty CreatePropertiesSchema(
            ContentTypeSchemaInfo contentType,
            SchemaProcessorContext context)
        {
            JsonSchemaProperty schema = GetOrCreateSchema(
                $"{contentType.SchemaId}PropertiesModel",
                context,
                () =>
                {
                    var schema = new JsonSchema
                    {
                        Type = JsonObjectType.Object,
                        AllowAdditionalProperties = false,
                    };

                    // We will be creating schemas for all content types, so we know for sure that the composition will exist.
                    // It might not exist at this point, but it will be added later, so we can use a simple reference.
                    foreach (var compositionSchemaId in contentType.CompositionSchemaIds)
                    {
                        var reference = new JsonSchemaProperty();
                        ((IJsonReferenceBase)reference).ReferencePath = $"#/definitions/{compositionSchemaId}";
                        schema.AllOf.Add(reference);
                    }

                    foreach (KeyValuePair<string, JsonSchemaProperty> propertyKv in ContentTypePropertiesMapper(contentType, context))
                    {
                        schema.Properties.Add(propertyKv);
                    }

                    return schema;
                });
            return schema;
        }

        private Dictionary<string, JsonSchemaProperty> ContentTypePropertiesMapper(
            ContentTypeSchemaInfo contentType,
            SchemaProcessorContext context)
        {
            var properties = new Dictionary<string, JsonSchemaProperty>();
            foreach (ContentTypePropertySchemaInfo propertyInfo in contentType.Properties.Where(x => !x.Inherited))
            {
                JsonSchemaProperty schema = GetOrCreateSchema(propertyInfo.DeliveryApiClrType, context);
                properties[propertyInfo.Alias] = schema;
            }

            return properties;
        }

        private JsonTypeInfo GetJsonTypeInfo(Type type)
        {
            JsonTypeInfo? jsonTypeInfo = _jsonSerializerOptions.TypeInfoResolver?.GetTypeInfo(type, _jsonSerializerOptions);
            return jsonTypeInfo ?? throw new InvalidOperationException("Could not get JsonTypeInfo for type " + type.FullName);
        }

        private string GetSchemaId(JsonTypeInfo type)
            => _schemaIdSelector.SchemaId(type.Type);

        private string GetTypePropertyName(PublishedItemType itemType)
            => itemType switch
            {
                PublishedItemType.Content => "contentType",
                PublishedItemType.Media => "mediaType",
                _ => throw new NotSupportedException($"Unsupported PublishedItemType: {itemType}"),
            };
    }
}
