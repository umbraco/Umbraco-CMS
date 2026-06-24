// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Extensions;

[TestFixture]
public class HttpRequestExtensionsTests
{
    [Test]
    public void GetUfprt_DoesNotThrow_WhenFormBodyIsMalformed()
    {
        HttpRequest request = RequestWithUnreadableForm(new IOException(
            "Unexpected end of Stream, the content may have already been read by another component."));

        Assert.DoesNotThrow(() => request.GetUfprt());
        Assert.IsNull(request.GetUfprt());
    }

    [Test]
    public void GetUfprt_DoesNotThrow_WhenFormLimitIsExceeded()
    {
        HttpRequest request = RequestWithUnreadableForm(new InvalidDataException("Form value count limit exceeded."));

        Assert.DoesNotThrow(() => request.GetUfprt());
        Assert.IsNull(request.GetUfprt());
    }

    [Test]
    public void GetUfprt_FallsBackToQueryString_WhenFormBodyIsMalformed()
    {
        HttpRequest request = RequestWithUnreadableForm(new IOException("Unexpected end of Stream."));
        request.QueryString = new QueryString("?ufprt=from-query");

        Assert.AreEqual("from-query", request.GetUfprt());
    }

    [Test]
    public void GetUfprt_ReadsFormValue_WhenFormIsWellFormed()
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Form = new FormCollection(new Dictionary<string, StringValues> { ["ufprt"] = "from-form" });

        Assert.AreEqual("from-form", context.Request.GetUfprt());
    }

    // Exercises the real ASP.NET Core form parser (not a fake) against a truncated multipart body, mirroring
    // the reported repro: a missing closing boundary makes the synchronous Form getter throw a real IOException.
    [Test]
    public void GetUfprt_DoesNotThrow_WhenRealMultipartBodyIsTruncated()
    {
        const string boundary = "----testboundary";
        var body = $"--{boundary}\r\nContent-Disposition: form-data; name=\"test\"\r\n\r\nsomevalue";
        HttpRequest request = RequestWithRawMultipartBody(body, boundary);

        Assert.DoesNotThrow(() => request.GetUfprt());
        Assert.IsNull(request.GetUfprt());
    }

    // BadHttpRequestException (raised for body/size faults) derives from IOException, so it must be swallowed too.
    [Test]
    public void GetUfprt_DoesNotThrow_WhenBodyExceedsSizeLimit()
    {
        HttpRequest request = RequestWithUnreadableForm(new BadHttpRequestException("Request body too large."));

        Assert.DoesNotThrow(() => request.GetUfprt());
        Assert.IsNull(request.GetUfprt());
    }

    // Guards against the filter being loosened to a bare catch: only form-parse failures are swallowed,
    // every other exception must still propagate.
    [Test]
    public void GetUfprt_Rethrows_WhenExceptionIsUnexpected()
    {
        HttpRequest request = RequestWithUnreadableForm(new InvalidOperationException("unexpected"));

        Assert.Throws<InvalidOperationException>(() => request.GetUfprt());
    }

    private static HttpRequest RequestWithUnreadableForm(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = "multipart/form-data; boundary=----test";
        context.Features.Set<IFormFeature>(new ThrowingFormFeature(exception));
        return context.Request;
    }

    private static HttpRequest RequestWithRawMultipartBody(string body, string boundary)
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        var context = new DefaultHttpContext();
        context.Request.ContentType = $"multipart/form-data; boundary={boundary}";
        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentLength = bytes.Length;
        return context.Request;
    }
}
