using Microsoft.OpenApi;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.OpenApi;

[TestFixture]
public class SortTagsAndPathsTransformerTests
{
    private SortTagsAndPathsTransformer _transformer = null!;

    [SetUp]
    public void SetUp() => _transformer = new SortTagsAndPathsTransformer();

    [Test]
    public async Task TransformAsync_Sorts_Tags_Alphabetically()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag>
            {
                new() { Name = "Pineapple" },
                new() { Name = "Apple" },
                new() { Name = "Mango" },
            },
            Paths = new OpenApiPaths(),
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        var tagNames = document.Tags.Select(t => t.Name).ToList();
        Assert.AreEqual(new[] { "Apple", "Mango", "Pineapple" }, tagNames);
    }

    [Test]
    public async Task TransformAsync_Sorts_Tags_Case_Sensitive()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag>
            {
                new() { Name = "apple" },
                new() { Name = "Apple" },
                new() { Name = "APPLE" },
            },
            Paths = new OpenApiPaths(),
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert - Ordinal comparison: uppercase letters come before lowercase
        var tagNames = document.Tags.Select(t => t.Name).ToList();
        Assert.AreEqual(new[] { "APPLE", "Apple", "apple" }, tagNames);
    }

    [Test]
    public async Task TransformAsync_Sorts_Paths_By_Tag_Then_By_Key()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag>(),
            Paths = new OpenApiPaths
            {
                ["/pineapple"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new() { Tags = new HashSet<OpenApiTagReference> { new("PineappleTag") } },
                    },
                },
                ["/apple"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new() { Tags = new HashSet<OpenApiTagReference> { new("AppleTag") } },
                    },
                },
                ["/banana"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new() { Tags = new HashSet<OpenApiTagReference> { new("AppleTag") } },
                    },
                },
            },
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert - Sorted by tag name first, then by path key
        var pathKeys = document.Paths.Keys.ToList();
        Assert.AreEqual(new[] { "/apple", "/banana", "/pineapple" }, pathKeys);
    }

    [Test]
    public async Task TransformAsync_Sorts_Paths_By_Alphabetically_First_Tag_On_Operation_With_Multiple_Tags()
    {
        // Arrange - /pineapple's operation has two tags; the alphabetically-first ("AppleTag") must drive
        // sorting, not insertion order. If the transformer picked tags[0] instead, /pineapple would sort
        // under "PineappleTag" and come last.
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag>(),
            Paths = new OpenApiPaths
            {
                ["/pineapple"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new()
                        {
                            Tags = new HashSet<OpenApiTagReference>
                            {
                                new("PineappleTag"),
                                new("AppleTag"),
                            },
                        },
                    },
                },
                ["/banana"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new() { Tags = new HashSet<OpenApiTagReference> { new("BananaTag") } },
                    },
                },
            },
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert - /pineapple wins on "AppleTag" < "BananaTag" despite also carrying "PineappleTag".
        var pathKeys = document.Paths.Keys.ToList();
        Assert.AreEqual(new[] { "/pineapple", "/banana" }, pathKeys);
    }

    [Test]
    public async Task TransformAsync_Sorts_Paths_By_Tags_Flattened_Across_Operations()
    {
        // Arrange - /pineapple has two operations (GET tagged "PineappleTag", POST tagged "AppleTag"). The
        // sort key must flatten tags across all operations on the path and pick the alphabetically-first;
        // without SelectMany this test would sort by whichever operation is enumerated first (non-deterministic).
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag>(),
            Paths = new OpenApiPaths
            {
                ["/pineapple"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new() { Tags = new HashSet<OpenApiTagReference> { new("PineappleTag") } },
                        [HttpMethod.Post] = new() { Tags = new HashSet<OpenApiTagReference> { new("AppleTag") } },
                    },
                },
                ["/banana"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new() { Tags = new HashSet<OpenApiTagReference> { new("BananaTag") } },
                    },
                },
            },
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert - /pineapple wins on "AppleTag" (from its POST) < "BananaTag" (from /banana's only operation).
        var pathKeys = document.Paths.Keys.ToList();
        Assert.AreEqual(new[] { "/pineapple", "/banana" }, pathKeys);
    }

    [Test]
    public async Task TransformAsync_Handles_Empty_Tags()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Tags = null,
            Paths = new OpenApiPaths(),
        };

        // Act & Assert - should not throw
        await _transformer.TransformAsync(document, null!, CancellationToken.None);
        Assert.IsNotNull(document.Tags);
        Assert.AreEqual(0, document.Tags.Count);
    }

    [Test]
    public async Task TransformAsync_Handles_Empty_Paths()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag> { new() { Name = "TestTag" } },
            Paths = new OpenApiPaths(),
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert
        Assert.AreEqual(0, document.Paths.Count);
    }

    [Test]
    public async Task TransformAsync_Handles_Paths_Without_Operations()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag>(),
            Paths = new OpenApiPaths
            {
                ["/path-b"] = new OpenApiPathItem { Operations = null },
                ["/path-a"] = new OpenApiPathItem { Operations = null },
            },
        };

        // Act
        await _transformer.TransformAsync(document, null!, CancellationToken.None);

        // Assert - Falls back to path key sorting
        var pathKeys = document.Paths.Keys.ToList();
        Assert.AreEqual(new[] { "/path-a", "/path-b" }, pathKeys);
    }

    [Test]
    public async Task TransformAsync_Handles_Operations_Without_Tags()
    {
        // Arrange
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag>(),
            Paths = new OpenApiPaths
            {
                ["/path-b"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<HttpMethod, OpenApiOperation>
                    {
                        [HttpMethod.Get] = new() { Tags = null },
                    },
                },
                ["/path-a"] = new OpenApiPathItem
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

        // Assert - Falls back to path key sorting
        var pathKeys = document.Paths.Keys.ToList();
        Assert.AreEqual(new[] { "/path-a", "/path-b" }, pathKeys);
    }
}
