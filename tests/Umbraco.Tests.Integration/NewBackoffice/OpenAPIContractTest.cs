using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.NewBackoffice;

// We only run this test in release because the schema looks different depending if it's built against release or debug.
// XML summaries is included in the description of a response model in release, but not debug mode.
#if DEBUG
[Ignore("This test runs only in release")]
#endif
[TestFixture]
public class OpenAPIContractTest : UmbracoTestServerTestBase
{

    private GlobalSettings GlobalSettings => GetRequiredService<IOptions<GlobalSettings>>().Value;

    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    [Test]
    public async Task Validate_OpenApi_Contract_is_implemented()
    {
        string[] keysToIgnore = { "servers" };

        var officePath = GlobalSettings.GetBackOfficePath(HostingEnvironment);

        var urlToContract = $"{officePath}/api/openapi.json";
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

        Assert.AreEqual(originalGeneratedContract, mergedContract, $"Generated API do not respect the contract:{Environment.NewLine}Expected:{Environment.NewLine}{originalGeneratedContract.ToString(Formatting.Indented)}{Environment.NewLine}{Environment.NewLine}Actual:{Environment.NewLine}{mergedContract.ToString(Formatting.Indented)}");
    }
}
