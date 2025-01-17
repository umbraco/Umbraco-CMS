using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Install;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Cms.Tests.Integration.NewBackoffice;

[TestFixture]
internal sealed class OpenAPIContractTest : UmbracoTestServerTestBase
{
    private GlobalSettings GlobalSettings => GetRequiredService<IOptions<GlobalSettings>>().Value;

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
    public async Task Validate_OpenApi_Contract_is_implemented()
    {
        string[] keysToIgnore = { "servers", "x-generator" };
        var officePath = GlobalSettings.GetBackOfficePath(HostingEnvironment);

        var urlToContract = $"{officePath}/management/api/openapi.json";
        var swaggerPath = $"{officePath}/swagger/management/swagger.json";
        var apiContract = JsonNode.Parse(await Client.GetStringAsync(urlToContract)).AsObject();

        var generatedJsonString = await Client.GetStringAsync(swaggerPath);
        var mergedContract = JsonNode.Parse(generatedJsonString).AsObject();
        var originalGeneratedContract = JsonNode.Parse(generatedJsonString).AsObject();

        mergedContract.MergeLeft(apiContract);

        foreach (var key in keysToIgnore)
        {
            originalGeneratedContract.Remove(key);
            mergedContract.Remove(key);
        }

        Assert.AreEqual(originalGeneratedContract.ToJsonString(), mergedContract.ToJsonString(), $"Generated API do not respect the contract.");
    }
}
