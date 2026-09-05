// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Web.Common.AspNetCore;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.AspNetCore;

[TestFixture]
public class AspNetCoreRequestAccessorTests
{
    [Test]
    public void GetRequestValue_DoesNotThrow_WhenFormBodyIsMalformed()
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = "multipart/form-data; boundary=----test";
        context.Features.Set<IFormFeature>(new ThrowingFormFeature(new IOException(
            "Unexpected end of Stream, the content may have already been read by another component.")));

        var accessor = CreateAccessor(context);

        Assert.DoesNotThrow(() => accessor.GetRequestValue("umbPageID"));
        Assert.IsNull(accessor.GetRequestValue("umbPageID"));
    }

    [Test]
    public void GetRequestValue_FallsBackToQueryString_WhenFormBodyIsMalformed()
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = "multipart/form-data; boundary=----test";
        context.Request.QueryString = new QueryString("?umbPageID=1046");
        context.Features.Set<IFormFeature>(new ThrowingFormFeature(new InvalidDataException("Form value count limit exceeded.")));

        var accessor = CreateAccessor(context);

        Assert.AreEqual("1046", accessor.GetRequestValue("umbPageID"));
    }

    [Test]
    public void GetRequestValue_ReadsFormValue_WhenFormIsWellFormed()
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Form = new FormCollection(new Dictionary<string, StringValues> { ["umbPageID"] = "1046" });

        var accessor = CreateAccessor(context);

        Assert.AreEqual("1046", accessor.GetRequestValue("umbPageID"));
    }

    private static AspNetCoreRequestAccessor CreateAccessor(HttpContext context)
    {
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(context);

        var webRoutingSettings = new Mock<IOptionsMonitor<WebRoutingSettings>>();
        webRoutingSettings.Setup(x => x.CurrentValue).Returns(new WebRoutingSettings());

        return new AspNetCoreRequestAccessor(httpContextAccessor.Object, webRoutingSettings.Object);
    }
}
