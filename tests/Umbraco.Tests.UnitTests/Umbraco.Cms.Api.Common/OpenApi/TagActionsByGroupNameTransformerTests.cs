using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.OpenApi;

[TestFixture]
public class TagActionsByGroupNameTransformerTests
{
    private TagActionsByGroupNameTransformer _transformer = null!;
    private IServiceProvider _services = null!;

    [SetUp]
    public void SetUp()
    {
        _transformer = new TagActionsByGroupNameTransformer();
        _services = new ServiceCollection().BuildServiceProvider();
    }

    #region Operation Transformer Tests

    [Test]
    public async Task TransformAsync_Operation_Does_Nothing_When_GroupName_Is_Null()
    {
        // Arrange
        var operation = new OpenApiOperation();
        var apiDescription = new ApiDescription { GroupName = null };
        var document = new OpenApiDocument();
        var context = new OpenApiOperationTransformerContext
        {
            Document = document,
            Description = apiDescription,
            DocumentName = "test",
            ApplicationServices = _services,
        };

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert
        Assert.IsNull(operation.Tags);
    }

    [Test]
    public async Task TransformAsync_Operation_Sets_Tag_From_GroupName()
    {
        // Arrange
        const string groupName = "TestGroup";
        var operation = new OpenApiOperation();
        var apiDescription = new ApiDescription { GroupName = groupName };
        var document = new OpenApiDocument();
        var context = new OpenApiOperationTransformerContext
        {
            Document = document,
            Description = apiDescription,
            DocumentName = "test",
            ApplicationServices = _services,
        };

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert
        Assert.IsNotNull(operation.Tags);
        Assert.AreEqual(1, operation.Tags.Count);
        Assert.AreEqual(groupName, operation.Tags.First().Name);
    }

    [Test]
    public async Task TransformAsync_Operation_Adds_Tag_To_Document_When_Not_Present()
    {
        // Arrange
        const string groupName = "NewGroup";
        var operation = new OpenApiOperation();
        var apiDescription = new ApiDescription { GroupName = groupName };
        var document = new OpenApiDocument();
        var context = new OpenApiOperationTransformerContext
        {
            Document = document,
            Description = apiDescription,
            DocumentName = "test",
            ApplicationServices = _services,
        };

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert
        Assert.IsNotNull(document.Tags);
        Assert.AreEqual(1, document.Tags.Count);
        Assert.AreEqual(groupName, document.Tags.First().Name);
    }

    [Test]
    public async Task TransformAsync_Operation_Does_Not_Duplicate_Tag_In_Document()
    {
        // Arrange
        const string groupName = "ExistingGroup";
        var operation = new OpenApiOperation();
        var apiDescription = new ApiDescription { GroupName = groupName };
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag> { new() { Name = groupName } },
        };
        var context = new OpenApiOperationTransformerContext
        {
            Document = document,
            Description = apiDescription,
            DocumentName = "test",
            ApplicationServices = _services,
        };

        // Act
        await _transformer.TransformAsync(operation, context, CancellationToken.None);

        // Assert
        Assert.AreEqual(1, document.Tags.Count);
    }

    #endregion

    #region Document Transformer Tests

    [Test]
    public async Task TransformAsync_Document_Removes_Unused_Tags()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag>
            {
                new() { Name = "UsedTag" },
                new() { Name = "UnusedTag" },
            },
            Paths = new OpenApiPaths
            {
                ["/test"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new()
                        {
                            Tags = new HashSet<OpenApiTagReference> { new("UsedTag") },
                        },
                    },
                },
            },
        };

        // Act - context is not used by the document transformer implementation
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.AreEqual(1, document.Tags.Count);
        Assert.AreEqual("UsedTag", document.Tags.First().Name);
    }

    [Test]
    public async Task TransformAsync_Document_Keeps_All_Used_Tags()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag>
            {
                new() { Name = "Tag1" },
                new() { Name = "Tag2" },
            },
            Paths = new OpenApiPaths
            {
                ["/test1"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new()
                        {
                            Tags = new HashSet<OpenApiTagReference> { new("Tag1") },
                        },
                    },
                },
                ["/test2"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Post] = new()
                        {
                            Tags = new HashSet<OpenApiTagReference> { new("Tag2") },
                        },
                    },
                },
            },
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.AreEqual(2, document.Tags.Count);
    }

    [Test]
    public async Task TransformAsync_Document_Handles_Null_Tags()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Tags = null,
            Paths = new OpenApiPaths(),
        };

        // Act & Assert - should not throw
        await _transformer.TransformAsync(document, null!, CancellationToken.None);
        Assert.IsNull(document.Tags);
    }

    [Test]
    public async Task TransformAsync_Document_Handles_Empty_Paths()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag> { new() { Name = "OrphanTag" } },
            Paths = new OpenApiPaths(),
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert - orphan tag should be removed
        Assert.AreEqual(0, document.Tags.Count);
    }

    [Test]
    public async Task TransformAsync_Document_Handles_Operations_Without_Tags()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag> { new() { Name = "UnusedTag" } },
            Paths = new OpenApiPaths
            {
                ["/test"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new() { Tags = null },
                    },
                },
            },
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert - unused tag should be removed
        Assert.AreEqual(0, document.Tags.Count);
    }

    [Test]
    public async Task TransformAsync_Document_Handles_Multiple_Tags_Per_Operation()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag>
            {
                new() { Name = "Tag1" },
                new() { Name = "Tag2" },
                new() { Name = "UnusedTag" },
            },
            Paths = new OpenApiPaths
            {
                ["/test"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new()
                        {
                            Tags = new HashSet<OpenApiTagReference>
                            {
                                new("Tag1"),
                                new("Tag2"),
                            },
                        },
                    },
                },
            },
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.AreEqual(2, document.Tags.Count);
        Assert.IsTrue(document.Tags.Any(t => t.Name == "Tag1"));
        Assert.IsTrue(document.Tags.Any(t => t.Name == "Tag2"));
        Assert.IsFalse(document.Tags.Any(t => t.Name == "UnusedTag"));
    }

    [Test]
    public async Task TransformAsync_Document_Handles_Null_Operations()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag> { new() { Name = "UnusedTag" } },
            Paths = new OpenApiPaths
            {
                ["/test"] = new OpenApiPathItem
                {
                    Operations = null,
                },
            },
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert - unused tag should be removed
        Assert.AreEqual(0, document.Tags.Count);
    }

    #endregion
}
