using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.OpenApi;

[TestFixture]
public class RequireNonNullablePropertiesSchemaTransformerTests
{
    private RequireNonNullablePropertiesSchemaTransformer _transformer = null!;
    private JsonSerializerOptions _jsonOptions = null!;
    private IServiceProvider _services = null!;

    [SetUp]
    public void SetUp()
    {
        _transformer = new RequireNonNullablePropertiesSchemaTransformer();
        _jsonOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        _services = new ServiceCollection().BuildServiceProvider();
    }

    private OpenApiSchemaTransformerContext CreateContext(JsonTypeInfo jsonTypeInfo) =>
        new()
        {
            JsonTypeInfo = jsonTypeInfo,
            JsonPropertyInfo = null,
            ParameterDescription = null,
            DocumentName = "test",
            ApplicationServices = _services,
        };

    [Test]
    public async Task TransformAsync_Adds_NonNullable_Properties_To_Required()
    {
        // Arrange
        var schema = new OpenApiSchema
        {
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["name"] = new OpenApiSchema { Type = JsonSchemaType.String },
                ["age"] = new OpenApiSchema { Type = JsonSchemaType.Integer },
            },
        };

        var jsonTypeInfo = _jsonOptions.GetTypeInfo(typeof(TestModelWithNonNullableProperties));
        var context = CreateContext(jsonTypeInfo);

        // Act
        await _transformer.TransformAsync(schema, context, CancellationToken.None);

        // Assert
        Assert.That(schema.Required, Is.Not.Null);
        Assert.That(schema.Required.Contains("name"), Is.True);
        Assert.That(schema.Required.Contains("age"), Is.True);
    }

    [Test]
    public async Task TransformAsync_Does_Not_Duplicate_Already_Required_Properties()
    {
        // Arrange
        var schema = new OpenApiSchema
        {
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["name"] = new OpenApiSchema { Type = JsonSchemaType.String },
            },
            Required = new HashSet<string> { "name" },
        };

        var jsonTypeInfo = _jsonOptions.GetTypeInfo(typeof(TestModelWithNonNullableProperties));
        var context = CreateContext(jsonTypeInfo);

        // Act
        await _transformer.TransformAsync(schema, context, CancellationToken.None);

        // Assert - Should still have only one "name" entry
        Assert.That(schema.Required.Count(r => r == "name"), Is.EqualTo(1));
    }

    [Test]
    public async Task TransformAsync_Skips_Nullable_Properties()
    {
        // Arrange
        var schema = new OpenApiSchema
        {
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["optionalName"] = new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null },
            },
        };

        var jsonTypeInfo = _jsonOptions.GetTypeInfo(typeof(TestModelWithNullableProperties));
        var context = CreateContext(jsonTypeInfo);

        // Act
        await _transformer.TransformAsync(schema, context, CancellationToken.None);

        // Assert - Nullable property should not be required
        Assert.That(schema.Required?.Contains("optionalName") ?? false, Is.False);
    }

    [Test]
    public async Task TransformAsync_Handles_Schema_Without_Properties()
    {
        // Arrange
        var schema = new OpenApiSchema
        {
            Properties = null,
        };

        var jsonTypeInfo = _jsonOptions.GetTypeInfo(typeof(TestModelWithNonNullableProperties));
        var context = CreateContext(jsonTypeInfo);

        // Act & Assert - Should not throw
        await _transformer.TransformAsync(schema, context, CancellationToken.None);
        Assert.That(schema.Required, Is.Not.Null); // Required is initialized even with no properties
    }

    [Test]
    public async Task TransformAsync_Handles_Empty_Properties()
    {
        // Arrange
        var schema = new OpenApiSchema
        {
            Properties = new Dictionary<string, IOpenApiSchema>(),
        };

        var jsonTypeInfo = _jsonOptions.GetTypeInfo(typeof(TestModelWithNonNullableProperties));
        var context = CreateContext(jsonTypeInfo);

        // Act & Assert - Should not throw
        await _transformer.TransformAsync(schema, context, CancellationToken.None);
    }

    [Test]
    public async Task TransformAsync_Uses_Schema_Type_For_Unknown_Properties()
    {
        // Arrange - Property not in JsonTypeInfo (like discriminator $type)
        var schema = new OpenApiSchema
        {
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["$type"] = new OpenApiSchema { Type = JsonSchemaType.String }, // Non-nullable schema type
            },
        };

        var jsonTypeInfo = _jsonOptions.GetTypeInfo(typeof(TestModelWithNonNullableProperties));
        var context = CreateContext(jsonTypeInfo);

        // Act
        await _transformer.TransformAsync(schema, context, CancellationToken.None);

        // Assert - Should be required based on schema type (not nullable)
        Assert.That(schema.Required?.Contains("$type") ?? false, Is.True);
    }

    [Test]
    public async Task TransformAsync_Skips_Nullable_Schema_Type_For_Unknown_Properties()
    {
        // Arrange - Unknown property with nullable schema type
        var schema = new OpenApiSchema
        {
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["unknownProp"] = new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null },
            },
        };

        var jsonTypeInfo = _jsonOptions.GetTypeInfo(typeof(TestModelWithNonNullableProperties));
        var context = CreateContext(jsonTypeInfo);

        // Act
        await _transformer.TransformAsync(schema, context, CancellationToken.None);

        // Assert - Should not be required (nullable schema type)
        Assert.That(schema.Required?.Contains("unknownProp") ?? false, Is.False);
    }

    #region Test Helper Classes

    private class TestModelWithNonNullableProperties
    {
        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }
    }

    private class TestModelWithNullableProperties
    {
        public string? OptionalName { get; set; }

        public int? OptionalAge { get; set; }
    }

    #endregion
}
