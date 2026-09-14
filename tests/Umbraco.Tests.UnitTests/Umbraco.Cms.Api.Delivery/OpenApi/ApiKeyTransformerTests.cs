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
        Assert.That(document.Components, Is.Not.Null);
        Assert.That(document.Components.SecuritySchemes, Is.Not.Null);
        Assert.That(document.Components.SecuritySchemes.ContainsKey("ApiKeyAuth"), Is.True);

        var scheme = document.Components.SecuritySchemes["ApiKeyAuth"];
        Assert.That(scheme.Type, Is.EqualTo(SecuritySchemeType.ApiKey));
        Assert.That(scheme.Name, Is.EqualTo(Constants.DeliveryApi.HeaderNames.ApiKey));
        Assert.That(scheme.In, Is.EqualTo(ParameterLocation.Header));
        Assert.That(scheme.Description, Is.Not.Null);
    }

    [Test]
    public async Task TransformAsync_Document_Adds_Security_Requirement()
    {
        // Arrange
        var document = new OpenApiDocument();

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.That(document.Security, Is.Not.Null);
        Assert.That(document.Security, Has.Count.EqualTo(1));
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
        Assert.That(document.Components.Schemas.ContainsKey("ExistingSchema"), Is.True);
        Assert.That(document.Components.SecuritySchemes!.ContainsKey("ApiKeyAuth"), Is.True);
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
        Assert.That(document.Components.SecuritySchemes.ContainsKey("ExistingScheme"), Is.True);
        Assert.That(document.Components.SecuritySchemes.ContainsKey("ApiKeyAuth"), Is.True);
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
        Assert.That(document.Security, Has.Count.EqualTo(2));
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
        Assert.That(operation.Security, Is.Not.Null);
        Assert.That(operation.Security, Has.Count.EqualTo(1));
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
        Assert.That(operation.Security, Has.Count.EqualTo(2));
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
        Assert.That(operation.Security, Is.Not.Null);
        Assert.That(operation.Security, Has.Count.EqualTo(1));
        var requirement = operation.Security.First();
        Assert.That(requirement, Has.Count.EqualTo(1));
    }

    #endregion
}
