using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using Umbraco.Cms.Web.Common.Controllers;

namespace Umbraco.Tests.IntegrationBenchmarks;

public class HybridCacheBenchmark : UmbracoTestServerTestBase
{
    [GlobalSetup]
    public void BenchmarkSetup()
    {
        Setup();
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
