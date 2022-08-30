using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.NewBackoffice;

[TestFixture]
public class OpenAPIContractTest : UmbracoTestServerTestBase
{
    // ensure composers are added

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddComposers();


    private GlobalSettings GlobalSettings => GetRequiredService<IOptions<GlobalSettings>>().Value;
    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    [Test]
    public async Task Validate_OpenApi_Contract_is_implemented()
    {
        string[] keysToIgnore = new[] { "servers" };
        
        var officePath = GlobalSettings.GetBackOfficePath(HostingEnvironment);

        var urlToContract = $"{officePath}/api/openapi.json";
        var swaggerPath = $"{officePath}/swagger/All/swagger.json";
        var apiContract = JObject.Parse(await Client.GetStringAsync(urlToContract));

        var generatedJsonString = await Client.GetStringAsync(swaggerPath);
        var mergedContract = JObject.Parse(generatedJsonString);
        var originalGeneratedContract = JObject.Parse(generatedJsonString);


        mergedContract.Merge(apiContract, new JsonMergeSettings()
        {
            MergeArrayHandling = MergeArrayHandling.Merge
        });

        foreach (var key in keysToIgnore)
        {
            originalGeneratedContract.Remove(key);
            mergedContract.Remove(key);
        }

        Assert.AreEqual(mergedContract, originalGeneratedContract, "Generated API do not respect the contract");
    }
}
