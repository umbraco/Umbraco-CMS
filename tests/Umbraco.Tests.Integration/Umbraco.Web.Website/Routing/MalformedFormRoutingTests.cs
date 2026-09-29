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
    // Content finders probe the request form for keys during routing (e.g. ContentFinderByPageIdQuery reads
    // "umbPageID"). When a request declares a form content type but carries a body the synchronous
    // HttpRequest.Form getter cannot parse (here a truncated multipart body with no closing boundary), that probe
    // must treat the form as absent and let routing continue, rather than let the parse failure escape as an
    // unhandled exception (which surfaces to the client as an HTTP 500).
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
