using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.Configuration;

[TestFixture]
public class ConfigureUmbracoOpenApiOptionsBaseTests
{
    private JsonSerializerOptions _jsonOptions = null!;

    [SetUp]
    public void SetUp()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        };
    }

    [Test]
    public void CreateSchemaReferenceId_Returns_Null_For_Primitive_Types()
    {
        // Arrange - primitives are inlined by default (OpenApiOptions returns null)
        var jsonTypeInfo = _jsonOptions.GetTypeInfo(typeof(int));

        // Act
        var result = ConfigureUmbracoOpenApiOptionsBase.CreateSchemaReferenceId(jsonTypeInfo);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public void CreateSchemaReferenceId_Returns_DefaultSchemaId_For_Non_Umbraco_Types()
    {
        // Arrange - non-Umbraco types should return the default schema ID (unchanged)
        // Using NUnit.Framework.TestAttribute as an example of a non-Umbraco type that gets a schema ID
        var jsonTypeInfo = _jsonOptions.GetTypeInfo(typeof(TestAttribute));

        // Act
        var result = ConfigureUmbracoOpenApiOptionsBase.CreateSchemaReferenceId(jsonTypeInfo);

        // Assert - note: no "Model" suffix added for non-Umbraco types
        Assert.AreEqual("TestAttribute", result);
    }

    [Test]
    public void CreateSchemaReferenceId_Uses_UmbracoSchemaIdGenerator_For_Umbraco_Types()
    {
        // Arrange - Umbraco types should use UmbracoSchemaIdGenerator
        var jsonTypeInfo = _jsonOptions.GetTypeInfo(typeof(UmbracoSchemaIdGenerator));

        // Act
        var result = ConfigureUmbracoOpenApiOptionsBase.CreateSchemaReferenceId(jsonTypeInfo);

        // Assert
        Assert.AreEqual("UmbracoSchemaIdGeneratorModel", result);
    }

    [Test]
    public void CreateSchemaReferenceId_Unwraps_Nullable_Umbraco_Types()
    {
        // Arrange - nullable Umbraco types should be unwrapped and use SchemaIdGenerator
        var jsonTypeInfo = _jsonOptions.GetTypeInfo(typeof(Direction?));

        // Act
        var result = ConfigureUmbracoOpenApiOptionsBase.CreateSchemaReferenceId(jsonTypeInfo);

        // Assert
        Assert.AreEqual("DirectionModel", result);
    }
}
