using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Tests.Integration.TestServerTest;

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
            // Adds Umbraco.Cms.ManagementApi
            mvcBuilder.AddApplicationPart(typeof(ManagementApi.Controllers.Install.InstallControllerBase).Assembly);
        });

        new ManagementApi.ManagementApiComposer().Compose(builder);
    }

    [Test]
    public async Task Validate_OpenApi_Contract_is_implemented()
    {
        string[] keysToIgnore = { "servers", "x-generator" };

        var officePath = GlobalSettings.GetBackOfficePath(HostingEnvironment);

        var urlToContract = $"{officePath}/management/api/openapi.json";
        var swaggerPath = $"{officePath}/swagger/All/swagger.json";
        var apiContract = JObject.Parse(await Client.GetStringAsync(urlToContract));

        var generatedJsonString = await Client.GetStringAsync(swaggerPath);
        var mergedContract = JObject.Parse(generatedJsonString);
        var originalGeneratedContract = JObject.Parse(generatedJsonString);

        mergedContract.Merge(apiContract, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Merge
        });

        foreach (var key in keysToIgnore)
        {
            originalGeneratedContract.Remove(key);
            mergedContract.Remove(key);
        }

        Assert.AreEqual(originalGeneratedContract.ToString(Formatting.Indented), mergedContract.ToString(Formatting.Indented), $"Generated API do not respect the contract.");
    }
}
