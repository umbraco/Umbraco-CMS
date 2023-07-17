using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management;
using Umbraco.Cms.Api.Management.Controllers.Install;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Persistence.EFCore.Composition;
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

        new ManagementApiComposer().Compose(builder);
        new UmbracoEFCoreComposer().Compose(builder);

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
        Console.WriteLine($"\nCurrent culture: {Thread.CurrentThread.CurrentCulture.Name}");
        Console.WriteLine($"Current UI culture: {Thread.CurrentThread.CurrentUICulture.Name}");
        var officePath = GlobalSettings.GetBackOfficePath(HostingEnvironment);

        var urlToContract = $"{officePath}/management/api/openapi.json";
        var swaggerPath = $"{officePath}/swagger/management/swagger.json";
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
        Assert.AreEqual(originalGeneratedContract.ToString(Formatting.Indented), mergedContract.ToString(Formatting.Indented), $"Generated API do not respect the contract:{Environment.NewLine}Expected:{Environment.NewLine}{originalGeneratedContract.ToString(Formatting.Indented)}{Environment.NewLine}{Environment.NewLine}Actual:{Environment.NewLine}{mergedContract.ToString(Formatting.Indented)}");
        Assert.AreEqual(originalGeneratedContract.ToString(Formatting.Indented), mergedContract.ToString(Formatting.Indented), $"Generated API do not respect the contract.");
    }
}
