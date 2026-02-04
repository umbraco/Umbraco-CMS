using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.OpenApi.Transformers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.OpenApi;

[TestFixture]
public class ContentTypeSchemaTransformerTests
{
    private Mock<IContentTypeSchemaService> _contentTypeSchemaServiceMock = null!;
    private Mock<IOptionsMonitor<JsonOptions>> _jsonOptionsMonitorMock = null!;
    private Mock<ILogger<ContentTypeSchemaTransformer>> _loggerMock = null!;
    private JsonSerializerOptions _jsonSerializerOptions = null!;
    private IServiceProvider _services = null!;

    [SetUp]
    public void SetUp()
    {
        _contentTypeSchemaServiceMock = new Mock<IContentTypeSchemaService>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<ContentTypeSchemaTransformer>>(MockBehavior.Strict);
        _jsonOptionsMonitorMock = new Mock<IOptionsMonitor<JsonOptions>>(MockBehavior.Strict);

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        var jsonOptions = new JsonOptions
        {
            SerializerOptions =
            {
                TypeInfoResolver = _jsonSerializerOptions.TypeInfoResolver,
                PropertyNamingPolicy = _jsonSerializerOptions.PropertyNamingPolicy,
            },
        };

        _jsonOptionsMonitorMock
            .Setup(x => x.Get(Constants.JsonOptionsNames.DeliveryApi))
            .Returns(jsonOptions);

        _services = new ServiceCollection().BuildServiceProvider();
    }

    [Test]
    public void Constructor_Throws_When_TypeInfoResolver_Is_Null()
    {
        // Arrange
        var jsonOptions = new JsonOptions { SerializerOptions = { TypeInfoResolver = null } };

        _jsonOptionsMonitorMock
            .Setup(x => x.Get(Constants.JsonOptionsNames.DeliveryApi))
            .Returns(jsonOptions);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => CreateTransformer());
    }

    [Test]
    public async Task DocumentTransformAsync_Does_Not_Throw_When_Components_Is_Null()
    {
        // Arrange
        var transformer = CreateTransformer();
        var document = new OpenApiDocument { Components = null };

        // Act
        await transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.IsNull(document.Components);
    }

    [Test]
    public async Task DocumentTransformAsync_Does_Not_Throw_When_Schemas_Is_Null()
    {
        // Arrange
        var transformer = CreateTransformer();
        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents { Schemas = null },
        };

        // Act
        await transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.IsNull(document.Components.Schemas);
    }

    [Test]
    public async Task DocumentTransformAsync_Does_Not_Throw_When_Schemas_Is_Empty()
    {
        // Arrange
        var transformer = CreateTransformer();
        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>(),
            },
        };

        // Act
        await transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.AreEqual(0, document.Components.Schemas.Count);
    }

    [Test]
    public async Task DocumentTransformAsync_Preserves_Schemas_Without_Placeholders()
    {
        // Arrange
        var transformer = CreateTransformer();
        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["TestSchema"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        Properties = new Dictionary<string, IOpenApiSchema>
                        {
                            ["TestProperty"] = new OpenApiSchema { Type = JsonSchemaType.String },
                        },
                    },
                },
            },
        };

        // Act
        await transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.AreEqual(1, document.Components.Schemas.Count);
        Assert.That(document.Components.Schemas.ContainsKey("TestSchema"));

        var schema = document.Components.Schemas["TestSchema"] as OpenApiSchema;
        Assert.IsNotNull(schema);
        Assert.AreEqual(JsonSchemaType.Object, schema.Type);
        Assert.IsNotNull(schema.Properties);
        Assert.AreEqual(1, schema.Properties.Count);
        Assert.That(schema.Properties.ContainsKey("TestProperty"));

        var propertySchema = schema.Properties["TestProperty"] as OpenApiSchema;
        Assert.IsNotNull(propertySchema);
        Assert.AreEqual(JsonSchemaType.String, propertySchema.Type);
    }

    [Test]
    public async Task DocumentTransformAsync_Replaces_Placeholder_Schema_In_Properties_With_Reference()
    {
        // Arrange
        var transformer = CreateTransformer();
        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["ParentSchema"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        Properties = new Dictionary<string, IOpenApiSchema>
                        {
                            ["ChildProperty"] = new OpenApiSchema
                            {
                                Metadata = new Dictionary<string, object>
                                {
                                    ["x-recursive-ref"] = "ChildSchema",
                                },
                            },
                        },
                    },
                    ["ChildSchema"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                    },
                },
            },
        };

        // Act
        await transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        var parentSchema = document.Components.Schemas["ParentSchema"] as OpenApiSchema;
        Assert.IsNotNull(parentSchema?.Properties);
        var childProperty = parentSchema.Properties["ChildProperty"];
        Assert.IsInstanceOf<OpenApiSchemaReference>(childProperty);
        Assert.That(((OpenApiSchemaReference)childProperty).Reference.Id, Is.EqualTo("ChildSchema"));
    }

    [Test]
    public async Task DocumentTransformAsync_Replaces_Placeholder_Schema_In_AllOf_With_Reference()
    {
        // Arrange
        var transformer = CreateTransformer();
        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["ComposedSchema"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        AllOf = new List<IOpenApiSchema>
                        {
                            new OpenApiSchema
                            {
                                Metadata = new Dictionary<string, object>
                                {
                                    ["x-recursive-ref"] = "BaseSchema",
                                },
                            },
                        },
                    },
                    ["BaseSchema"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                    },
                },
            },
        };

        // Act
        await transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        var composedSchema = document.Components.Schemas["ComposedSchema"] as OpenApiSchema;
        Assert.IsNotNull(composedSchema);
        Assert.IsNotNull(composedSchema.AllOf);
        Assert.AreEqual(1, composedSchema.AllOf.Count);
        Assert.IsInstanceOf<OpenApiSchemaReference>(composedSchema.AllOf[0]);
        var baseReference = (OpenApiSchemaReference)composedSchema.AllOf[0];
        Assert.That(baseReference.Reference?.Id, Is.EqualTo("BaseSchema"));
    }

    [Test]
    public async Task DocumentTransformAsync_Replaces_Placeholder_Schema_In_Array_Items_With_Reference()
    {
        // Arrange
        var transformer = CreateTransformer();
        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["ArraySchema"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        Properties = new Dictionary<string, IOpenApiSchema>
                        {
                            ["Items"] = new OpenApiSchema
                            {
                                Type = JsonSchemaType.Array,
                                Items = new OpenApiSchema
                                {
                                    Metadata = new Dictionary<string, object>
                                    {
                                        ["x-recursive-ref"] = "ItemSchema",
                                    },
                                },
                            },
                        },
                    },
                    ["ItemSchema"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                    },
                },
            },
        };

        // Act
        await transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        var arraySchema = document.Components.Schemas["ArraySchema"] as OpenApiSchema;
        Assert.IsNotNull(arraySchema);
        Assert.IsNotNull(arraySchema.Properties);
        var itemsProperty = arraySchema.Properties["Items"] as OpenApiSchema;
        Assert.IsNotNull(itemsProperty);
        Assert.IsNotNull(itemsProperty.Items);
        Assert.IsInstanceOf<OpenApiSchemaReference>(itemsProperty.Items);
        var itemReference = (OpenApiSchemaReference)itemsProperty.Items;
        Assert.That(itemReference.Reference?.Id, Is.EqualTo("ItemSchema"));
    }

    [Test]
    public async Task SchemaTransformAsync_Does_Nothing_For_NonMatching_Types()
    {
        // Arrange
        var transformer = CreateTransformer();
        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["Name"] = new OpenApiSchema { Type = JsonSchemaType.String },
            },
        };

        var jsonTypeInfo = _jsonSerializerOptions.GetTypeInfo(typeof(object));
        var context = CreateSchemaContext(jsonTypeInfo);

        // Act
        await transformer.TransformAsync(schema, context, CancellationToken.None);

        // Assert - Schema should remain unchanged (no discriminator added)
        Assert.IsNull(schema.Discriminator);
        Assert.IsNull(schema.OneOf);
    }

    [Test]
    public async Task SchemaTransformAsync_Adds_ElementTypes_For_IApiElement()
    {
        // Arrange
        var documentTypes = new List<ContentTypeSchemaInfo>
        {
            CreateContentTypeSchemaInfo("article", "Article", isElement: false),
            CreateContentTypeSchemaInfo("textBlock", "TextBlock", isElement: true),
        };

        _contentTypeSchemaServiceMock
            .Setup(x => x.GetDocumentTypes())
            .Returns(documentTypes);

        var transformer = CreateTransformer();
        var schema = new OpenApiSchema();

        var jsonTypeInfo = CreatePolymorphicJsonTypeInfo<IApiElement>();
        var context = CreateSchemaContext(jsonTypeInfo);

        // Act
        await transformer.TransformAsync(schema, context, CancellationToken.None);

        // Assert - Only element types should be included
        _contentTypeSchemaServiceMock.Verify(x => x.GetDocumentTypes(), Times.Once);

        Assert.IsNotNull(schema.Discriminator);
        Assert.IsNotNull(schema.Discriminator.Mapping);
        Assert.IsNotNull(schema.OneOf);

        Assert.AreEqual("contentType", schema.Discriminator.PropertyName);
        Assert.AreEqual(1, schema.OneOf.Count);
        Assert.IsTrue(schema.Discriminator.Mapping.ContainsKey("textBlock"));
        Assert.IsFalse(schema.Discriminator.Mapping.ContainsKey("article"));
    }

    [Test]
    public async Task SchemaTransformAsync_Adds_NonElementTypes_For_IApiContent()
    {
        // Arrange
        var documentTypes = new List<ContentTypeSchemaInfo>
        {
            CreateContentTypeSchemaInfo("article", "Article", isElement: false),
            CreateContentTypeSchemaInfo("textBlock", "TextBlock", isElement: true),
        };

        _contentTypeSchemaServiceMock
            .Setup(x => x.GetDocumentTypes())
            .Returns(documentTypes);

        var transformer = CreateTransformer();
        var schema = new OpenApiSchema();

        var jsonTypeInfo = CreatePolymorphicJsonTypeInfo<IApiContent>();
        var context = CreateSchemaContext(jsonTypeInfo);

        // Act
        await transformer.TransformAsync(schema, context, CancellationToken.None);

        // Assert - Only non-element types should be included
        _contentTypeSchemaServiceMock.Verify(x => x.GetDocumentTypes(), Times.Once);

        Assert.IsNotNull(schema.Discriminator);
        Assert.IsNotNull(schema.Discriminator.Mapping);
        Assert.IsNotNull(schema.OneOf);

        Assert.AreEqual("contentType", schema.Discriminator.PropertyName);
        Assert.AreEqual(1, schema.OneOf.Count);
        Assert.IsTrue(schema.Discriminator.Mapping.ContainsKey("article"));
        Assert.IsFalse(schema.Discriminator.Mapping.ContainsKey("textBlock"));
    }

    [Test]
    public async Task SchemaTransformAsync_Adds_NonElementTypes_For_IApiContentResponse()
    {
        // Arrange
        var documentTypes = new List<ContentTypeSchemaInfo>
        {
            CreateContentTypeSchemaInfo("article", "Article", isElement: false),
            CreateContentTypeSchemaInfo("textBlock", "TextBlock", isElement: true),
        };

        _contentTypeSchemaServiceMock
            .Setup(x => x.GetDocumentTypes())
            .Returns(documentTypes);

        var transformer = CreateTransformer();
        var schema = new OpenApiSchema();

        var jsonTypeInfo = CreatePolymorphicJsonTypeInfo<IApiContentResponse>();
        var context = CreateSchemaContext(jsonTypeInfo);

        // Act
        await transformer.TransformAsync(schema, context, CancellationToken.None);

        // Assert - Only non-element types should be included for IApiContentResponse
        _contentTypeSchemaServiceMock.Verify(x => x.GetDocumentTypes(), Times.Once);

        Assert.IsNotNull(schema.Discriminator);
        Assert.IsNotNull(schema.Discriminator.Mapping);
        Assert.IsNotNull(schema.OneOf);

        Assert.AreEqual("contentType", schema.Discriminator.PropertyName);
        Assert.AreEqual(1, schema.OneOf.Count);
        Assert.IsTrue(schema.Discriminator.Mapping.ContainsKey("article"));
        Assert.IsFalse(schema.Discriminator.Mapping.ContainsKey("textBlock"));
    }

    [Test]
    public async Task SchemaTransformAsync_Adds_MediaTypes_For_IApiMediaWithCropsResponse()
    {
        // Arrange
        var mediaTypes = new List<ContentTypeSchemaInfo>
        {
            CreateContentTypeSchemaInfo("image", "Image", isElement: false),
        };

        _contentTypeSchemaServiceMock
            .Setup(x => x.GetMediaTypes())
            .Returns(mediaTypes);

        var transformer = CreateTransformer();
        var schema = new OpenApiSchema();

        var jsonTypeInfo = CreatePolymorphicJsonTypeInfo<IApiMediaWithCropsResponse>();
        var context = CreateSchemaContext(jsonTypeInfo);

        // Act
        await transformer.TransformAsync(schema, context, CancellationToken.None);

        // Assert
        _contentTypeSchemaServiceMock.Verify(x => x.GetMediaTypes(), Times.Once);

        Assert.IsNotNull(schema.Discriminator);
        Assert.IsNotNull(schema.Discriminator.Mapping);
        Assert.IsNotNull(schema.OneOf);

        Assert.AreEqual("mediaType", schema.Discriminator.PropertyName);
        Assert.AreEqual(1, schema.OneOf.Count);
        Assert.IsTrue(schema.Discriminator.Mapping.ContainsKey("image"));
    }

    [Test]
    public async Task SchemaTransformAsync_Adds_MediaTypes_For_IApiMediaWithCrops()
    {
        // Arrange
        var mediaTypes = new List<ContentTypeSchemaInfo>
        {
            CreateContentTypeSchemaInfo("image", "Image", isElement: false),
        };

        _contentTypeSchemaServiceMock
            .Setup(x => x.GetMediaTypes())
            .Returns(mediaTypes);

        var transformer = CreateTransformer();
        var schema = new OpenApiSchema();

        var jsonTypeInfo = CreatePolymorphicJsonTypeInfo<IApiMediaWithCrops>();
        var context = CreateSchemaContext(jsonTypeInfo);

        // Act
        await transformer.TransformAsync(schema, context, CancellationToken.None);

        // Assert
        _contentTypeSchemaServiceMock.Verify(x => x.GetMediaTypes(), Times.Once);

        Assert.IsNotNull(schema.Discriminator);
        Assert.IsNotNull(schema.Discriminator.Mapping);
        Assert.IsNotNull(schema.OneOf);

        Assert.AreEqual("mediaType", schema.Discriminator.PropertyName);
        Assert.AreEqual(1, schema.OneOf.Count);
        Assert.IsTrue(schema.Discriminator.Mapping.ContainsKey("image"));
    }

    private ContentTypeSchemaTransformer CreateTransformer() =>
        new(
            _contentTypeSchemaServiceMock.Object,
            _jsonOptionsMonitorMock.Object,
            _loggerMock.Object);

    private OpenApiSchemaTransformerContext CreateSchemaContext(JsonTypeInfo jsonTypeInfo) =>
        new()
        {
            JsonTypeInfo = jsonTypeInfo,
            JsonPropertyInfo = null,
            ParameterDescription = null,
            DocumentName = "test",
            ApplicationServices = _services,
            Document = new OpenApiDocument(),
        };

    private static JsonTypeInfo<T> CreatePolymorphicJsonTypeInfo<T>() =>
        JsonTypeInfo.CreateJsonTypeInfo<T>(
            new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() });

    private static ContentTypeSchemaInfo CreateContentTypeSchemaInfo(
        string alias,
        string schemaId,
        bool isElement,
        List<ContentTypePropertySchemaInfo>? properties = null) =>
        new()
        {
            Alias = alias,
            SchemaId = schemaId,
            CompositionSchemaIds = [],
            Properties = properties ?? [],
            IsElement = isElement,
        };
}
