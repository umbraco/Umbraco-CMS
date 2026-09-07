// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Extensions;

[TestFixture]
public class HtmlHelperRenderExtensionsTests
{
    [Test]
    public void PreviewBadge_Contains_Nonce_When_Available()
    {
        var badge = RenderPreviewBadge("s0m3-n0nc3");

        Assert.That(badge, Does.Contain(@"<script nonce=""s0m3-n0nc3"" src="""));
    }

    [Test]
    public void PreviewBadge_Omits_Nonce_When_Not_Available()
    {
        var badge = RenderPreviewBadge(null);

        Assert.Multiple(() =>
        {
            Assert.That(badge, Does.Contain(@"<script src="""));
            Assert.That(badge, Does.Not.Contain("nonce"));
        });
    }

    [Test]
    public void PreviewBadge_Omits_Nonce_When_Service_Is_Not_Registered()
    {
        var badge = RenderPreviewBadge(null, registerCspNonceService: false);

        Assert.Multiple(() =>
        {
            Assert.That(badge, Does.Contain(@"<script src="""));
            Assert.That(badge, Does.Not.Contain("nonce"));
        });
    }

    private static string RenderPreviewBadge(string? nonce, bool registerCspNonceService = true)
    {
        var umbracoContext = new Mock<IUmbracoContext>();
        umbracoContext.Setup(x => x.InPreviewMode).Returns(true);

        var umbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        IUmbracoContext? outContext = umbracoContext.Object;
        umbracoContextAccessor.Setup(x => x.TryGetUmbracoContext(out outContext)).Returns(true);

        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IHostingEnvironment>(x => x.ToAbsolute(It.IsAny<string>()) == "/umbraco"));

        if (registerCspNonceService)
        {
            services.AddSingleton(Mock.Of<ICspNonceService>(x => x.GetNonce() == nonce));
        }

        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        httpContext.Request.Path = "/";

        var htmlHelper = Mock.Of<IHtmlHelper>(x => x.ViewContext == new ViewContext { HttpContext = httpContext });

        return htmlHelper.PreviewBadge(
            umbracoContextAccessor.Object,
            Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext),
            new GlobalSettings(),
            Mock.Of<IIOHelper>(),
            new ContentSettings()).ToString()!;
    }
}
