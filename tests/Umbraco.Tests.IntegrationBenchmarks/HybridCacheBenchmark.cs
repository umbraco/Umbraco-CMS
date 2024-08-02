using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Umbraco.Cms.Api.Delivery.Controllers.Content;
using Umbraco.Cms.Api.Management.Controllers.ModelsBuilder;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Tests.IntegrationBenchmarks;

public class HybridCacheBenchmark : UmbracoTestServerTestBase
{
    /// <summary>
    /// we need this method because we are piggy backing off of <see cref="UmbracoTestServerTestBase"/>
    /// That uses NUnit things to figure out configuration information
    /// Since we are not actually running NUnit, we need to match some of its defaults.
    /// </summary>
    [Test]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console, Boot = true)]
    public void AdhocTestMethod()
    {
    }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.ConfigureOptions<ConfigureTypeFinderSettings>();
        builder.Services.ConfigureOptions<ConfigureTestDatabaseSettings>();

        builder.AddMvcAndRazor(mvcBuilding: mvcBuilder =>
        {
            // Adds Umbraco.Web.Common
            mvcBuilder.AddApplicationPart(typeof(RenderController).Assembly);

            // Adds Umbraco.Web.Website
            mvcBuilder.AddApplicationPart(typeof(SurfaceController).Assembly);

            // Adds Umbraco.Cms.Api.ManagementApi
            mvcBuilder.AddApplicationPart(typeof(ModelsBuilderControllerBase).Assembly);

            // Adds Umbraco.Cms.Api.DeliveryApi
            mvcBuilder.AddApplicationPart(typeof(ContentApiItemControllerBase).Assembly);

            // Adds Umbraco.Tests.Integration
            mvcBuilder.AddApplicationPart(typeof(UmbracoTestServerTestBase).Assembly);

            // Add Umbraco.Tests.IntegrationBenchmarks
            mvcBuilder.AddApplicationPart(typeof(HybridCacheBenchmark).Assembly);
        });
    }

    [GlobalSetup]
    public void BenchmarkSetup()
    {
        var globalSetup = new  GlobalSetupTeardown();
        globalSetup.SetUp();

        // See comment of AdhocTestMethod for why this is needed
        TestExecutionContext.CurrentContext.TestObject = this;

        Setup();
    }

    [GlobalCleanup]
    public void BenchmarkCleanup()
    {
        TearDown();
    }

    [Benchmark]
    public void GetCachedCanAssignNoFactory()
    {
        var url = PrepareApiControllerUrl<MyTestController>(x => x.GetWithHybridCache());
        var response = Client.GetAsync(url).GetAwaiter().GetResult();
    }
}

public class MyTestController : UmbracoApiController
{
    [HttpGet]
    public string GetWithHybridCache() => "Hello, World!";
}

public class ConfigureTypeFinderSettings : IConfigureOptions<TypeFinderSettings>
{
    public void Configure(TypeFinderSettings options)
    {
        // this assembly is discovered but not fully available
        options.AdditionalAssemblyExclusionEntries = ["testcentric.engine.metadata"];
    }
}

public class ConfigureTestDatabaseSettings : IConfigureOptions<TestDatabaseSettings>
{
    public void Configure(TestDatabaseSettings options)
    {
        options.DatabaseType = TestDatabaseSettings.TestDatabaseType.Sqlite;
        options.PrepareThreadCount = 1;
        options.SchemaDatabaseCount = 1;
        options.EmptyDatabasesCount = 1;
        options.SQLServerMasterConnectionString = string.Empty;
    }
}
