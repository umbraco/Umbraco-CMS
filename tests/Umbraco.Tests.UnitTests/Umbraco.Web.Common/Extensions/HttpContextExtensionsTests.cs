// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Extensions;

[TestFixture]
public class HttpContextExtensionsTests
{
    [Test]
    public void GetRequestValue_DoesNotThrow_WhenFormBodyIsMalformed()
    {
        HttpContext context = ContextWithUnreadableForm(new IOException(
            "Unexpected end of Stream, the content may have already been read by another component."));

        Assert.DoesNotThrow(() => context.GetRequestValue("umbdebugshowtrace"));
        Assert.IsNull(context.GetRequestValue("umbdebugshowtrace"));
    }

    [Test]
    public void GetRequestValue_DoesNotThrow_WhenFormLimitIsExceeded()
    {
        HttpContext context = ContextWithUnreadableForm(new InvalidDataException("Form value count limit exceeded."));

        Assert.DoesNotThrow(() => context.GetRequestValue("umbdebugshowtrace"));
        Assert.IsNull(context.GetRequestValue("umbdebugshowtrace"));
    }

    [Test]
    public void GetRequestValue_FallsBackToQueryString_WhenFormBodyIsMalformed()
    {
        HttpContext context = ContextWithUnreadableForm(new IOException("Unexpected end of Stream."));
        context.Request.QueryString = new QueryString("?umbdebugshowtrace=true");

        Assert.AreEqual("true", context.GetRequestValue("umbdebugshowtrace"));
    }

    [Test]
    public void GetRequestValue_ReadsFormValue_WhenFormIsWellFormed()
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Form = new FormCollection(new Dictionary<string, StringValues> { ["key"] = "from-form" });

        Assert.AreEqual("from-form", context.GetRequestValue("key"));
    }

    private static HttpContext ContextWithUnreadableForm(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = "multipart/form-data; boundary=----test";
        context.Features.Set<IFormFeature>(new ThrowingFormFeature(exception));
        return context;
    }
}
