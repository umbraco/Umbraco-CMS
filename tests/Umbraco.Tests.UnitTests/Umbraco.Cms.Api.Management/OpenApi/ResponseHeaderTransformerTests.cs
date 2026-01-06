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
        Assert.IsNotNull(response.Headers);
        Assert.IsTrue(response.Headers.ContainsKey(Constants.Headers.GeneratedResource));

        var header = (OpenApiHeader)response.Headers[Constants.Headers.GeneratedResource];
        Assert.AreEqual(JsonSchemaType.String, header.Schema?.Type);
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
        Assert.IsNotNull(response.Headers);
        Assert.IsTrue(response.Headers.ContainsKey(Constants.Headers.Location));

        var header = (OpenApiHeader)response.Headers[Constants.Headers.Location];
        Assert.AreEqual(JsonSchemaType.String, header.Schema?.Type);
        Assert.AreEqual("uri", header.Schema?.Format);
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
            Assert.IsNull(response.Headers);
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
        Assert.AreEqual(3, response.Headers?.Count); // Custom + GeneratedResource + Location
        Assert.IsTrue(response.Headers?.ContainsKey("X-Custom-Header"));
        Assert.IsTrue(response.Headers?.ContainsKey(Constants.Headers.GeneratedResource));
        Assert.IsTrue(response.Headers?.ContainsKey(Constants.Headers.Location));
    }
}
