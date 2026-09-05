using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.OpenApi.Transformers;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.OpenApi;

[TestFixture]
public class ApiKeyTransformerTests
{
    private ApiKeyTransformer _transformer = null!;
    private IServiceProvider _services = null!;

    [SetUp]
    public void SetUp()
    {
        _transformer = new ApiKeyTransformer();
        _services = new ServiceCollection().BuildServiceProvider();
    }

    #region Document Transformer Tests

    [Test]
    public async Task TransformAsync_Document_Creates_ApiKey_Security_Scheme()
    {
        // Arrange
        var document = new OpenApiDocument();

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.IsNotNull(document.Components);
        Assert.IsNotNull(document.Components.SecuritySchemes);
        Assert.IsTrue(document.Components.SecuritySchemes.ContainsKey("ApiKeyAuth"));

        var scheme = document.Components.SecuritySchemes["ApiKeyAuth"];
        Assert.AreEqual(SecuritySchemeType.ApiKey, scheme.Type);
        Assert.AreEqual(Constants.DeliveryApi.HeaderNames.ApiKey, scheme.Name);
        Assert.AreEqual(ParameterLocation.Header, scheme.In);
        Assert.IsNotNull(scheme.Description);
    }

    [Test]
    public async Task TransformAsync_Document_Adds_Security_Requirement()
    {
        // Arrange
        var document = new OpenApiDocument();

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.IsNotNull(document.Security);
        Assert.AreEqual(1, document.Security.Count);
    }

    [Test]
    public async Task TransformAsync_Document_Preserves_Existing_Components()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["ExistingSchema"] = new OpenApiSchema(),
                },
            },
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.IsTrue(document.Components.Schemas.ContainsKey("ExistingSchema"));
        Assert.IsTrue(document.Components.SecuritySchemes!.ContainsKey("ApiKeyAuth"));
    }

    [Test]
    public async Task TransformAsync_Document_Preserves_Existing_Security_Schemes()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
                {
                    ["ExistingScheme"] = new OpenApiSecurityScheme { Type = SecuritySchemeType.Http },
                },
            },
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.IsTrue(document.Components.SecuritySchemes.ContainsKey("ExistingScheme"));
        Assert.IsTrue(document.Components.SecuritySchemes.ContainsKey("ApiKeyAuth"));
    }

    [Test]
    public async Task TransformAsync_Document_Adds_To_Existing_Security_Requirements()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Security = new List<OpenApiSecurityRequirement>
            {
                new(),
            },
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.AreEqual(2, document.Security.Count);
    }

    #endregion

    #region Operation Transformer Tests

    [Test]
    public async Task TransformAsync_Operation_Adds_Security_Requirement()
    {
        // Arrange
        var operation = new OpenApiOperation();
        var document = new OpenApiDocument();
        var context = new OpenApiOperationTransformerContext
        {
            Document = document,
            Description = new ApiDescription(),
            DocumentName = "test",
            ApplicationServices = _services,
        };

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert
        Assert.IsNotNull(operation.Security);
        Assert.AreEqual(1, operation.Security.Count);
    }

    [Test]
    public async Task TransformAsync_Operation_Preserves_Existing_Security()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Security = new List<OpenApiSecurityRequirement>
            {
                new(),
            },
        };
        var document = new OpenApiDocument();
        var context = new OpenApiOperationTransformerContext
        {
            Document = document,
            Description = new ApiDescription(),
            DocumentName = "test",
            ApplicationServices = _services,
        };

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert
        Assert.AreEqual(2, operation.Security.Count);
    }

    [Test]
    public async Task TransformAsync_Operation_Security_References_Document()
    {
        // Arrange
        var operation = new OpenApiOperation();
        var document = new OpenApiDocument();
        var context = new OpenApiOperationTransformerContext
        {
            Document = document,
            Description = new ApiDescription(),
            DocumentName = "test",
            ApplicationServices = _services,
        };

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert - The security requirement should have one key (the ApiKeyAuth scheme reference)
        Assert.IsNotNull(operation.Security);
        Assert.AreEqual(1, operation.Security.Count);
        var requirement = operation.Security.First();
        Assert.AreEqual(1, requirement.Count);
    }

    #endregion
}
