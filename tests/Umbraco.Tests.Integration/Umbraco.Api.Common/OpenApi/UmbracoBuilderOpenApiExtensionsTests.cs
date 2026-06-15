using System.Text;
using System.Text.Json.Nodes;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Api.Common.OpenApi;

/// <summary>
/// Integration tests for <see cref="UmbracoBuilderOpenApiExtensions.AddBackOfficeOpenApiDocument"/>. Registers a
/// pair of test controllers (one mapped to the test document via <see cref="MapToApiAttribute"/>, one not) and
/// asserts that the generated OpenAPI document at <c>/umbraco/openapi/{name}.json</c> reflects each of the
/// defaults the builder is supposed to apply (filtering by <c>[MapToApi]</c>, Umbraco operation-id and schema-id
/// conventions, default tagging, and title flowing from <c>WithTitle</c>).
/// </summary>
[TestFixture]
internal sealed class UmbracoBuilderOpenApiExtensionsTests : UmbracoTestServerTestBase
{
    internal const string ApiName = "test-back-office-api";

    private const string ApiTitle = "Test Back Office API";

    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddMvcAndRazor(mvc => mvc.AddApplicationPart(typeof(UmbracoBuilderOpenApiExtensionsTests).Assembly));

        builder.AddBackOfficeOpenApiDocument(
            ApiName,
            document => document
                .WithTitle(ApiTitle));
    }

    [Test]
    public async Task Generated_Document_Is_Valid_OpenApi()
    {
        var spec = await FetchDocumentAsync();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(spec));
        var result = await OpenApiDocument.LoadAsync(stream, "json");

        Assert.That(result.Document, Is.Not.Null, "Failed to parse OpenAPI document.");
        Assert.Multiple(() =>
        {
            foreach (var error in result.Diagnostic?.Errors ?? [])
            {
                Assert.Fail($"(Error) {error.Message}");
            }
        });
    }

    [Test]
    public async Task WithTitle_Flows_Through_To_Info_Title()
    {
        JsonObject doc = await ParseDocumentAsync();

        Assert.That((string?)doc["info"]?["title"], Is.EqualTo(ApiTitle));
    }

    [Test]
    public async Task Only_Endpoints_With_Matching_MapToApi_Are_Included()
    {
        JsonObject doc = await ParseDocumentAsync();
        JsonObject paths = doc["paths"]!.AsObject();

        Assert.That(paths.ContainsKey($"/{ApiName}/apple/ping"), Is.True, "Mapped endpoint missing from generated document.");
        Assert.That(paths.ContainsKey($"/{ApiName}/apple/things"), Is.True, "Mapped endpoint missing from generated document.");
        Assert.That(paths.ContainsKey($"/{ApiName}/zebra/ping"), Is.True, "Mapped endpoint missing from generated document.");

        foreach (var path in paths)
        {
            Assert.That(
                path.Key.StartsWith($"/{ApiName}/apple") || path.Key.StartsWith($"/{ApiName}/zebra"), Is.True,
                $"Unexpected path '{path.Key}' in document — only endpoints matching MapToApi(\"{ApiName}\") should be included.");
        }
    }

    [Test]
    public async Task Operation_Ids_Follow_Umbraco_Convention()
    {
        JsonObject doc = await ParseDocumentAsync();

        Assert.That(
            (string?)doc["paths"]?[$"/{ApiName}/apple/ping"]?["get"]?["operationId"], Is.EqualTo("GetTestBackOfficeApiApplePing"));
        Assert.That(
            (string?)doc["paths"]?[$"/{ApiName}/apple/things"]?["post"]?["operationId"], Is.EqualTo("PostTestBackOfficeApiAppleThings"));
    }

    [Test]
    public async Task Operation_Id_Appends_Version_Suffix_When_Action_Targets_A_Non_Default_Version()
    {
        JsonObject doc = await ParseDocumentAsync();

        // [MapToApiVersion("2.0")] on the action — default API version is 1.0, so the transformer
        // appends "2.0" to the operation ID. Default-version actions (above) get no suffix.
        Assert.That(
            (string?)doc["paths"]?[$"/{ApiName}/apple/special"]?["get"]?["operationId"], Is.EqualTo("GetTestBackOfficeApiAppleSpecial2.0"));
    }

    [Test]
    public async Task Schema_Ids_Follow_Umbraco_Naming_Convention()
    {
        JsonObject doc = await ParseDocumentAsync();
        JsonObject? schemas = doc["components"]?["schemas"]?.AsObject();

        Assert.That(schemas, Is.Not.Null, "Generated document has no component schemas.");

        // TestPayload has no "Model" suffix in source. UmbracoSchemaIdGenerator should add one because the type
        // lives under the Umbraco.Cms namespace.
        Assert.That(
            schemas!.ContainsKey("TestPayloadModel"), Is.True,
            $"Expected schema 'TestPayloadModel'. Schemas in document: {string.Join(", ", schemas!.Select(s => s.Key))}");
    }

    [Test]
    public async Task Operations_Are_Tagged_By_Their_Controller_Group_Name()
    {
        JsonObject doc = await ParseDocumentAsync();

        IReadOnlyList<string?> fruitTags = doc["paths"]![$"/{ApiName}/apple/ping"]!["get"]!["tags"]!
            .AsArray().Select(t => (string?)t).ToList();
        IReadOnlyList<string?> animalTags = doc["paths"]![$"/{ApiName}/zebra/ping"]!["get"]!["tags"]!
            .AsArray().Select(t => (string?)t).ToList();

        Assert.That(fruitTags, Is.EqualTo(new[] { "Fruit" }));
        Assert.That(animalTags, Is.EqualTo(new[] { "Animal" }));
    }

    [Test]
    public async Task Document_Level_Tags_Are_Sorted_Alphabetically()
    {
        JsonObject doc = await ParseDocumentAsync();

        IReadOnlyList<string?> tags = doc["tags"]!.AsArray()
            .Select(t => (string?)t!["name"])
            .ToList();

        Assert.That(tags, Is.EqualTo(new[] { "Animal", "Fruit" }));
    }

    [Test]
    public async Task Paths_Are_Grouped_By_Tag_Then_Sorted_Within_Each_Group()
    {
        JsonObject doc = await ParseDocumentAsync();
        IReadOnlyList<string> paths = doc["paths"]!.AsObject().Select(p => p.Key).ToList();

        // "/zebra/ping" sorts AFTER "/apple/*" alphabetically by path, but its operation is tagged "Animal"
        // — which sorts BEFORE "Fruit" — so a tag-first sort places it before the apple paths. A naive
        // alphabetical-by-path sort would put it last.
        Assert.That(
            paths,
            Is.EqualTo(
                new[]
                {
                    $"/{ApiName}/zebra/ping",
                    $"/{ApiName}/apple/ping",
                    $"/{ApiName}/apple/special",
                    $"/{ApiName}/apple/things",
                }));
    }

    private async Task<string> FetchDocumentAsync()
    {
        var backOfficePath = HostingEnvironment.GetBackOfficePath();
        return await Client.GetStringAsync($"{backOfficePath}/openapi/{ApiName}.json");
    }

    private async Task<JsonObject> ParseDocumentAsync()
    {
        var spec = await FetchDocumentAsync();
        JsonObject? doc = JsonNode.Parse(spec)?.AsObject();
        Assert.That(doc, Is.Not.Null, "Failed to parse OpenAPI document as JSON.");
        return doc!;
    }
}

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route(UmbracoBuilderOpenApiExtensionsTests.ApiName + "/apple")]
[ApiExplorerSettings(GroupName = "Fruit")]
[MapToApi(UmbracoBuilderOpenApiExtensionsTests.ApiName)]
public class TestBackOfficeApiAppleController : ControllerBase
{
    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ping() => Ok();

    [HttpPost("things")]
    [ProducesResponseType(typeof(TestPayload), StatusCodes.Status200OK)]
    public IActionResult Create([FromBody] TestPayload payload) => Ok(payload);

    [HttpGet("special")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Special() => Ok();
}

// Route deliberately starts with "zebra" so it sorts AFTER "/apple/*" alphabetically — combined with the
// "Animal" group name (which sorts BEFORE "Fruit"), this lets the path-sort test distinguish tag-grouping
// from a naive alphabetical path sort.
[ApiController]
[Route(UmbracoBuilderOpenApiExtensionsTests.ApiName + "/zebra")]
[ApiExplorerSettings(GroupName = "Animal")]
[MapToApi(UmbracoBuilderOpenApiExtensionsTests.ApiName)]
public class TestBackOfficeApiZebraController : ControllerBase
{
    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ping() => Ok();
}

[ApiController]
[Route(UmbracoBuilderOpenApiExtensionsTests.ApiName + "/unmapped")]
public class TestBackOfficeApiUnmappedController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok();
}

public record TestPayload(string Name, int Count);
