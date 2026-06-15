using System.Net;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.Website.Routing;

/// <summary>
///     Demonstrates the supported replacement for the removed front-end <c>UmbracoApiController</c>:
///     a plain <see cref="ControllerBase"/> with <c>[ApiController]</c> and an explicit
///     <c>[Route("umbraco/api/...")]</c> is reachable on the front-end pipeline and returns JSON.
/// </summary>
[TestFixture]
internal sealed class FrontEndApiControllerTests : UmbracoTestServerTestBase
{
    [Test]
    [LongRunning]
    public async Task Can_Reach_Plain_ApiController_Via_Umbraco_Api_Route()
    {
        var url = PrepareUrl("/umbraco/api/test-front-end/ping");

        var response = await Client.GetAsync(url);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.Not.Null);
        Assert.That(response.Content.Headers.ContentType!.MediaType, Does.StartWith("application/json"));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("\"pong\""));
    }
}

[ApiController]
[Route("umbraco/api/test-front-end")]
public class TestFrontEndApiController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { reply = "pong" });
}
