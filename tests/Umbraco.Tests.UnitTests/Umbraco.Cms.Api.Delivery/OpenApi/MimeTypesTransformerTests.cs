using Microsoft.OpenApi;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.OpenApi.Transformers;

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
        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                ["/test"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new()
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
                        },
                    },
                },
            },
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        var response = (OpenApiResponse)document.Paths["/test"].Operations![HttpMethod.Get].Responses!["200"];
        Assert.AreEqual(1, response.Content?.Count);
        Assert.IsTrue(response.Content?.ContainsKey("application/json"));
    }

    [Test]
    public async Task TransformAsync_Removes_Non_Json_MimeTypes_From_RequestBodies()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                ["/test"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Post] = new()
                        {
                            RequestBody = new OpenApiRequestBody
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new(),
                                    ["application/x-www-form-urlencoded"] = new(),
                                },
                            },
                        },
                    },
                },
            },
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        var requestBody = (OpenApiRequestBody)document.Paths["/test"].Operations![HttpMethod.Post].RequestBody!;
        Assert.AreEqual(1, requestBody.Content?.Count);
        Assert.IsTrue(requestBody.Content?.ContainsKey("application/json"));
    }

    [Test]
    public async Task TransformAsync_Preserves_All_MimeTypes_When_Json_Not_Present()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                ["/test"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new()
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
                        },
                    },
                },
            },
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert - All MIME types preserved when JSON is not present
        var response = (OpenApiResponse)document.Paths["/test"].Operations![HttpMethod.Get].Responses!["200"];
        Assert.AreEqual(2, response.Content?.Count);
        Assert.IsTrue(response.Content?.ContainsKey("application/xml"));
        Assert.IsTrue(response.Content?.ContainsKey("text/plain"));
    }

    [Test]
    public async Task TransformAsync_Handles_Null_Content()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                ["/test"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new()
                        {
                            Responses = new OpenApiResponses
                            {
                                ["200"] = new OpenApiResponse { Content = null },
                            },
                        },
                    },
                },
            },
        };

        // Act & Assert - should not throw
        await _transformer.TransformAsync(document, null!, CancellationToken.None);
    }

    [Test]
    public async Task TransformAsync_Handles_Empty_Paths()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths(),
        };

        // Act & Assert - should not throw
        await _transformer.TransformAsync(document, null!, CancellationToken.None);
    }

    [Test]
    public async Task TransformAsync_Handles_Null_Operations()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                ["/test"] = new OpenApiPathItem { Operations = null },
            },
        };

        // Act & Assert - should not throw
        await _transformer.TransformAsync(document, null!, CancellationToken.None);
    }

    [Test]
    public async Task TransformAsync_Handles_Null_RequestBody()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                ["/test"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Post] = new() { RequestBody = null },
                    },
                },
            },
        };

        // Act & Assert - should not throw
        await _transformer.TransformAsync(document, null!, CancellationToken.None);
    }

    [Test]
    public async Task TransformAsync_Handles_Null_Responses()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Paths = new OpenApiPaths
            {
                ["/test"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new() { Responses = null },
                    },
                },
            },
        };

        // Act & Assert - should not throw
        await _transformer.TransformAsync(document, null!, CancellationToken.None);
    }
}
