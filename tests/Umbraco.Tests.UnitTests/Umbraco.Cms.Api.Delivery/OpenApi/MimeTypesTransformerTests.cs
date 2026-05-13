using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.OpenApi;

[TestFixture]
public class MimeTypesTransformerTests
{
    private MimeTypesTransformer _transformer = null!;

    [SetUp]
    public void SetUp() => _transformer = new MimeTypesTransformer();

    [Test]
    public async Task TransformAsync_Removes_Non_Json_MimeTypes_From_Responses()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new(),
                        ["application/xml"] = new(),
                        ["text/plain"] = new(),
                    },
                },
            },
        };

        OpenApiOperationTransformerContext context = CreateContext();

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert
        var response = (OpenApiResponse)operation.Responses!["200"];
        Assert.AreEqual(1, response.Content?.Count);
        Assert.IsTrue(response.Content?.ContainsKey("application/json"));
    }

    [Test]
    public async Task TransformAsync_Removes_Non_Json_MimeTypes_From_RequestBodies()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new(),
                    ["application/x-www-form-urlencoded"] = new(),
                },
            },
        };

        OpenApiOperationTransformerContext context = CreateContext();

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert
        var requestBody = (OpenApiRequestBody)operation.RequestBody!;
        Assert.AreEqual(1, requestBody.Content?.Count);
        Assert.IsTrue(requestBody.Content?.ContainsKey("application/json"));
    }

    [Test]
    public async Task TransformAsync_Preserves_All_MimeTypes_When_Json_Not_Present()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/xml"] = new(),
                        ["text/plain"] = new(),
                    },
                },
            },
        };

        OpenApiOperationTransformerContext context = CreateContext();

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert - All MIME types preserved when JSON is not present
        var response = (OpenApiResponse)operation.Responses!["200"];
        Assert.AreEqual(2, response.Content?.Count);
        Assert.IsTrue(response.Content?.ContainsKey("application/xml"));
        Assert.IsTrue(response.Content?.ContainsKey("text/plain"));
    }

    [Test]
    public async Task TransformAsync_Handles_Null_Content()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse { Content = null },
            },
        };

        OpenApiOperationTransformerContext context = CreateContext();

        // Act & Assert - should not throw
        await _transformer.TransformAsync(operation, context, CancellationToken.None);
    }

    [Test]
    public async Task TransformAsync_Handles_Null_RequestBody()
    {
        // Arrange
        var operation = new OpenApiOperation { RequestBody = null };

        OpenApiOperationTransformerContext context = CreateContext();

        // Act & Assert - should not throw
        await _transformer.TransformAsync(operation, context, CancellationToken.None);
    }

    [Test]
    public async Task TransformAsync_Handles_Null_Responses()
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = null };

        OpenApiOperationTransformerContext context = CreateContext();

        // Act & Assert - should not throw
        await _transformer.TransformAsync(operation, context, CancellationToken.None);
    }

    [Test]
    public async Task TransformAsync_Keeps_Only_Consumes_ContentTypes_For_RequestBody()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new(),
                    ["application/xml"] = new(),
                    ["multipart/form-data"] = new(),
                },
            },
        };

        OpenApiOperationTransformerContext context = CreateContext(new ConsumesAttribute("multipart/form-data"));

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert
        var requestBody = (OpenApiRequestBody)operation.RequestBody!;
        Assert.AreEqual(1, requestBody.Content?.Count);
        Assert.IsTrue(requestBody.Content?.ContainsKey("multipart/form-data"));
    }

    [Test]
    public async Task TransformAsync_Consumes_With_Multiple_ContentTypes_Keeps_All_Declared()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new(),
                    ["application/xml"] = new(),
                    ["text/plain"] = new(),
                },
            },
        };

        OpenApiOperationTransformerContext context = CreateContext(
            new ConsumesAttribute("application/json", "text/plain"));

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert
        var requestBody = (OpenApiRequestBody)operation.RequestBody!;
        Assert.AreEqual(2, requestBody.Content?.Count);
        Assert.IsTrue(requestBody.Content?.ContainsKey("application/json"));
        Assert.IsTrue(requestBody.Content?.ContainsKey("text/plain"));
    }

    [Test]
    public async Task TransformAsync_Consumes_Preserves_Schema_From_Existing_Entry()
    {
        // Arrange
        var existingSchema = new OpenApiSchema();
        var operation = new OpenApiOperation
        {
            RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new() { Schema = existingSchema },
                },
            },
        };

        OpenApiOperationTransformerContext context = CreateContext(
            new ConsumesAttribute("multipart/form-data"));

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert
        var requestBody = (OpenApiRequestBody)operation.RequestBody!;
        Assert.AreEqual(1, requestBody.Content?.Count);
        Assert.IsTrue(requestBody.Content?.ContainsKey("multipart/form-data"));
        Assert.AreSame(existingSchema, requestBody.Content!["multipart/form-data"].Schema);
    }

    private static OpenApiOperationTransformerContext CreateContext(params object[] endpointMetadata) =>
        new()
        {
            Document = new OpenApiDocument(),
            Description = new ApiDescription
            {
                ActionDescriptor = new ActionDescriptor
                {
                    EndpointMetadata = endpointMetadata.ToList(),
                },
            },
            DocumentName = "test",
            ApplicationServices = null!,
        };
}
