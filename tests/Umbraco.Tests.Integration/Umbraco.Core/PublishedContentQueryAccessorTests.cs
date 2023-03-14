using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core;

[TestFixture]
public class PublishedContentQueryAccessorTests : UmbracoTestServerTestBase
{
    [Test]
    public async Task PublishedContentQueryAccessor_WithRequestScope_WillProvideQuery()
    {
        var result = await Client.GetAsync("/demo-published-content-query-accessor");
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }
}

public class PublishedContentQueryAccessorTestController : Controller
{
    private readonly IPublishedContentQueryAccessor _accessor;

    public PublishedContentQueryAccessorTestController(IPublishedContentQueryAccessor accessor) => _accessor = accessor;

    [HttpGet("demo-published-content-query-accessor")]
    public IActionResult Test()
    {
        var success = _accessor.TryGetValue(out var query);

        if (!success || query == null)
        {
            throw new ApplicationException("It doesn't work");
        }

        return Ok();
    }
}
