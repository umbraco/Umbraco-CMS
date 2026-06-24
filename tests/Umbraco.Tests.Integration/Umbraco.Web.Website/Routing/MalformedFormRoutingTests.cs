// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.Website.Routing;

[TestFixture]
internal sealed class MalformedFormRoutingTests : UmbracoTestServerTestBase
{
    // Reproduces the reported repro at the routing layer: a request that declares a multipart form content type
    // but carries a truncated body (no closing boundary). The first content finder, ContentFinderByPageIdQuery,
    // reads the form looking for "umbPageID"; before the fix the synchronous HttpRequest.Form getter threw an
    // IOException that propagated out of RouteRequestAsync as an unhandled 500. It must now route gracefully.
    [Test]
    public void RouteRequestAsync_DoesNotThrow_WhenRequestHasMalformedMultipartBody()
    {
        const string boundary = "----testboundary";
        var body = $"--{boundary}\r\nContent-Disposition: form-data; name=\"test\"\r\n\r\nsomevalue";
        var bodyBytes = Encoding.UTF8.GetBytes(body);

        var httpContextAccessor = GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            Request =
            {
                Scheme = "https",
                Host = new HostString("localhost"),
                Path = "/",
                Method = HttpMethods.Post,
                ContentType = $"multipart/form-data; boundary={boundary}",
                ContentLength = bodyBytes.Length,
                Body = new MemoryStream(bodyBytes),
            },
        };

        GetRequiredService<IUmbracoContextFactory>().EnsureUmbracoContext();

        var router = GetRequiredService<IPublishedRouter>();

        Assert.DoesNotThrowAsync(async () =>
        {
            IPublishedRequestBuilder request = await router.CreateRequestAsync(new Uri("https://localhost/"));
            await router.RouteRequestAsync(request, new RouteRequestOptions(RouteDirection.Inbound));
        });
    }
}
