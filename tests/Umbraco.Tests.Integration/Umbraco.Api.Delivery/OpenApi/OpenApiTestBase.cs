using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Controllers;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Api.Delivery.OpenApi;

/// <summary>
/// Base class for Delivery API OpenAPI tests with shared test logic.
/// </summary>
internal abstract class OpenApiTestBase : UmbracoTestServerTestBase
{
    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    /// <summary>
    /// Known OpenAPI validation issues that are expected and should be ignored.
    /// These are tracked issues that don't prevent the API from functioning correctly.
    /// </summary>
    private static readonly string[] _knownValidationIssues =
    [
        // These path signature warnings occur because the Delivery API has multiple endpoints
        // that resolve to the same path pattern (e.g., get by id vs get by path)
        "The path signature '/umbraco/delivery/api/v2/content/item/{}' MUST be unique.",
        "The path signature '/umbraco/delivery/api/v2/media/item/{}' MUST be unique.",
    ];

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.AddMvcAndRazor(mvcBuilder =>
        {
            // Adds Umbraco.Cms.Api.Delivery
            mvcBuilder.AddApplicationPart(typeof(DeliveryApiControllerBase).Assembly);
        });

    /// <summary>
    /// Fetches the generated OpenAPI spec from the Delivery API endpoint.
    /// </summary>
    protected async Task<string> FetchOpenApiSpecAsync()
    {
        var backOfficePath = HostingEnvironment.GetBackOfficePath();
        var openApiPath = $"{backOfficePath}/openapi/delivery.json";
        return await Client.GetStringAsync(openApiPath);
    }

    /// <summary>
    /// Validates that the OpenAPI document is well-formed and compliant with the OpenAPI specification.
    /// Known issues are logged as warnings but don't fail the test. Unexpected issues will fail the test.
    /// </summary>
    protected static async Task ValidateOpenApiSpecAsync(string openApiJson)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(openApiJson));
        var result = await OpenApiDocument.LoadAsync(stream, "json");

        Assert.That(result.Document, Is.Not.Null, "Failed to parse OpenAPI document.");

        var allErrors = result.Diagnostic?.Errors ?? [];
        var knownIssues = allErrors.Where(e => _knownValidationIssues.Contains(e.Message)).ToList();

        // Log known issues as warnings (test still passes but warnings are visible in reports)
        foreach (var issue in knownIssues)
        {
            Assert.Warn($"(Known) {issue.Message}");
        }

        // Fail on unexpected errors (each error reported individually)
        Assert.Multiple(() =>
        {
            foreach (var error in allErrors.Except(knownIssues))
            {
                Assert.Fail($"(Error) {error.Message}");
            }
        });
    }

    /// <summary>
    /// Parses the OpenAPI spec JSON into a JsonNode for assertions.
    /// </summary>
    protected static JsonNode ParseOpenApiSpec(string openApiJson)
    {
        var document = JsonNode.Parse(openApiJson);
        Assert.That(document, Is.Not.Null, "Failed to parse OpenAPI spec JSON.");
        return document!;
    }

    /// <summary>
    /// Validates that the generated OpenAPI spec matches the expected contract file.
    /// If the expected contract file doesn't exist, saves the generated spec to that location.
    /// </summary>
    protected static async Task ValidateContractAsync(string generatedSpec, string expectedContractFileName)
    {
        var expectedContractPath = GetExpectedContractPath(expectedContractFileName);

        if (!File.Exists(expectedContractPath))
        {
            // Expected contract doesn't exist - save the generated one
            Directory.CreateDirectory(Path.GetDirectoryName(expectedContractPath)!);
            await File.WriteAllTextAsync(expectedContractPath, generatedSpec);
            Assert.Inconclusive($"Expected contract file did not exist. Generated contract saved to: {expectedContractPath}");
            return;
        }

        // Compare against existing expected contract
        var expectedContract = await File.ReadAllTextAsync(expectedContractPath);
        var generatedOpenApiJson = JsonNode.Parse(generatedSpec);
        var expectedOpenApiJson = JsonNode.Parse(expectedContract);

        Assert.NotNull(generatedOpenApiJson);
        Assert.NotNull(expectedOpenApiJson);

        Assert.AreEqual(
            expectedOpenApiJson!.ToJsonString(),
            generatedOpenApiJson!.ToJsonString(),
            "Generated API does not respect the contract.");
    }

    private static string GetExpectedContractPath(string fileName, [CallerFilePath] string callerFilePath = "")
    {
        // Use the source directory (where this file lives) so that saved files go to source control
        var sourceDirectory = Path.GetDirectoryName(callerFilePath)!;
        return Path.Combine(sourceDirectory, "ExpectedContracts", fileName);
    }
}
