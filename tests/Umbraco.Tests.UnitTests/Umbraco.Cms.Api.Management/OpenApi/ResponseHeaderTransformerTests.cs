using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi.Transformers;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.OpenApi;

[TestFixture]
public class ResponseHeaderTransformerTests
{
    private ResponseHeaderTransformer _transformer = null!;

    [SetUp]
    public void SetUp() => _transformer = new ResponseHeaderTransformer();

    [Test]
    public async Task TransformAsync_Adds_GeneratedResource_Header_For_201_Response()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["201"] = new OpenApiResponse(),
            },
        };

        // Act
        await _transformer.TransformAsync(operation, null!, CancellationToken.None);

        // Assert
        var response = (OpenApiResponse)operation.Responses["201"];
        Assert.That(response.Headers, Is.Not.Null);
        Assert.That(response.Headers.ContainsKey(Constants.Headers.GeneratedResource), Is.True);

        var header = (OpenApiHeader)response.Headers[Constants.Headers.GeneratedResource];
        Assert.That(header.Schema?.Type, Is.EqualTo(JsonSchemaType.String));
    }

    [Test]
    public async Task TransformAsync_Adds_Location_Header_For_201_Response()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["201"] = new OpenApiResponse(),
            },
        };

        // Act
        await _transformer.TransformAsync(operation, null!, CancellationToken.None);

        // Assert
        var response = (OpenApiResponse)operation.Responses["201"];
        Assert.That(response.Headers, Is.Not.Null);
        Assert.That(response.Headers.ContainsKey(Constants.Headers.Location), Is.True);

        var header = (OpenApiHeader)response.Headers[Constants.Headers.Location];
        Assert.That(header.Schema?.Type, Is.EqualTo(JsonSchemaType.String));
        Assert.That(header.Schema?.Format, Is.EqualTo("uri"));
    }

    [Test]
    public async Task TransformAsync_Does_Not_Add_Headers_For_Other_Status_Codes()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse(),
                ["400"] = new OpenApiResponse(),
                ["404"] = new OpenApiResponse(),
                ["500"] = new OpenApiResponse(),
            },
        };

        // Act
        await _transformer.TransformAsync(operation, null!, CancellationToken.None);

        // Assert
        foreach (var (_, value) in operation.Responses)
        {
            var response = (OpenApiResponse)value;
            Assert.That(response.Headers, Is.Null);
        }
    }

    [Test]
    public async Task TransformAsync_Handles_Empty_Responses()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses(),
        };

        // Act & Assert - should not throw
        await _transformer.TransformAsync(operation, null!, CancellationToken.None);
    }

    [Test]
    public async Task TransformAsync_Handles_Null_Responses()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = null,
        };

        // Act & Assert - should not throw
        await _transformer.TransformAsync(operation, null!, CancellationToken.None);
    }

    [Test]
    public async Task TransformAsync_Skips_Non_Numeric_Response_Keys()
    {
        // Arrange - OpenAPI permits "default" and status-class keys (e.g. "2XX") alongside numeric codes.
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["default"] = new OpenApiResponse(),
                ["2XX"] = new OpenApiResponse(),
                ["201"] = new OpenApiResponse(),
            },
        };

        // Act & Assert - should not throw, and 201 should still be transformed.
        await _transformer.TransformAsync(operation, null!, CancellationToken.None);

        var createdResponse = (OpenApiResponse)operation.Responses["201"];
        Assert.That(createdResponse.Headers, Is.Not.Null);
        Assert.That(createdResponse.Headers.ContainsKey(Constants.Headers.GeneratedResource), Is.True);

        var defaultResponse = (OpenApiResponse)operation.Responses["default"];
        Assert.That(defaultResponse.Headers, Is.Null);
    }

    [Test]
    public async Task TransformAsync_Preserves_Existing_Headers()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["201"] = new OpenApiResponse
                {
                    Headers = new Dictionary<string, IOpenApiHeader>
                    {
                        ["X-Custom-Header"] = new OpenApiHeader { Description = "Custom header" },
                    },
                },
            },
        };

        // Act
        await _transformer.TransformAsync(operation, null!, CancellationToken.None);

        // Assert
        var response = (OpenApiResponse)operation.Responses["201"];
        Assert.That(response.Headers?.Count, Is.EqualTo(3)); // Custom + GeneratedResource + Location
        Assert.That(response.Headers?.ContainsKey("X-Custom-Header"), Is.True);
        Assert.That(response.Headers?.ContainsKey(Constants.Headers.GeneratedResource), Is.True);
        Assert.That(response.Headers?.ContainsKey(Constants.Headers.Location), Is.True);
    }
}
