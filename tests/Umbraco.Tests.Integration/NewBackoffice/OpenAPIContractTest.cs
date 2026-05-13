using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Install;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Cms.Tests.Integration.NewBackoffice;

/// <summary>
/// Tests the Management API OpenAPI contract for correctness and consistency.
/// </summary>
[TestFixture]
internal sealed class OpenAPIContractTest : UmbracoTestServerTestBase
{
    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddMvcAndRazor(mvcBuilder =>
        {
            // Adds Umbraco.Cms.Api.Management
            mvcBuilder.AddApplicationPart(typeof(InstallControllerBase).Assembly);
        });

        // Currently we cannot do this in tests, as EF Core is not initialized
        builder.Services.PostConfigure<UmbracoPipelineOptions>(options =>
        {
            var backofficePipelineFilter = options.PipelineFilters.FirstOrDefault(x => x.Name.Equals("Backoffice"));
            if (backofficePipelineFilter != null)
            {
                options.PipelineFilters.Remove(backofficePipelineFilter);
            }
        });
    }

    [Test]
    public async Task OpenApiDocument_IsValid()
    {
        var openApiContract = await FetchGeneratedContractAsync();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(openApiContract));
        var result = await OpenApiDocument.LoadAsync(stream, "json");

        Assert.That(result.Document, Is.Not.Null, "Failed to parse OpenAPI document.");

        // Fail on unexpected errors (each error reported individually)
        Assert.Multiple(() =>
        {
            foreach (var error in result.Diagnostic?.Errors ?? [])
            {
                Assert.Fail($"(Error) {error.Message}");
            }
        });
    }

    [Test]
    public async Task OpenApiContract_MatchesExpected()
    {
        var backOfficePath = HostingEnvironment.GetBackOfficePath();

        // Fetch the expected OpenAPI contract
        var expectedContractUrl = $"{backOfficePath}/management/api/openapi.json";
        var expectedContract = JsonNode.Parse(await Client.GetStringAsync(expectedContractUrl))!.AsObject();

        // Fetch the generated OpenAPI contract
        var generatedContractJson = await FetchGeneratedContractAsync();
        var generatedContract = JsonNode.Parse(generatedContractJson)!.AsObject();

        // Merge the expected contract into a copy of generated (to check if generated contains everything from expected)
        var mergedContract = JsonNode.Parse(generatedContractJson)!.AsObject();
        mergedContract.MergeLeft(expectedContract);

        Assert.AreEqual(
            generatedContract.ToJsonString(),
            mergedContract.ToJsonString(),
            "Generated API does not respect the contract.");
    }

    /// <summary>
    /// Fetches the generated OpenAPI contract from the Management API endpoint.
    /// </summary>
    private async Task<string> FetchGeneratedContractAsync()
    {
        var backOfficePath = HostingEnvironment.GetBackOfficePath();
        var openApiPath = $"{backOfficePath}/openapi/management.json";
        return await Client.GetStringAsync(openApiPath);
    }
}
