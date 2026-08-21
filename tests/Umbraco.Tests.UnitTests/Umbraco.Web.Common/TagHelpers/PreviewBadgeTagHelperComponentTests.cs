// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.TagHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.TagHelpers;

[TestFixture]
public class PreviewBadgeTagHelperComponentTests
{
    [Test]
    public void Injects_Badge_Into_Body_When_In_Preview()
    {
        PreviewBadgeTagHelperComponent component = CreateComponent(nonce: null);
        TagHelperOutput output = CreateOutput("body");

        component.Process(CreateContext(), output);

        Assert.That(output.PostContent.GetContent(), Does.Contain("umb-website-preview"));
    }

    [Test]
    public void Does_Not_Inject_Badge_Into_Other_Elements()
    {
        PreviewBadgeTagHelperComponent component = CreateComponent(nonce: null);
        TagHelperOutput output = CreateOutput("head");

        component.Process(CreateContext(), output);

        Assert.That(output.PostContent.GetContent(), Is.Empty);
    }

    [Test]
    public void Does_Not_Inject_Badge_When_Not_In_Preview()
    {
        PreviewBadgeTagHelperComponent component = CreateComponent(nonce: null, inPreviewMode: false);
        TagHelperOutput output = CreateOutput("body");

        component.Process(CreateContext(), output);

        Assert.That(output.PostContent.GetContent(), Is.Empty);
    }

    [Test]
    public void Does_Not_Inject_Badge_When_Configured_Badge_Is_Empty()
    {
        PreviewBadgeTagHelperComponent component = CreateComponent(nonce: null, previewBadge: string.Empty);
        TagHelperOutput output = CreateOutput("body");

        component.Process(CreateContext(), output);

        Assert.That(output.PostContent.GetContent(), Is.Empty);
    }

    [Test]
    public void Badge_Contains_Nonce_When_Available()
    {
        PreviewBadgeTagHelperComponent component = CreateComponent("s0m3-n0nc3");
        TagHelperOutput output = CreateOutput("body");

        component.Process(CreateContext(), output);

        Assert.That(output.PostContent.GetContent(), Does.Contain(@"<script nonce=""s0m3-n0nc3"" src="""));
    }

    [Test]
    public void Badge_Omits_Nonce_When_Not_Available()
    {
        PreviewBadgeTagHelperComponent component = CreateComponent(nonce: null);
        TagHelperOutput output = CreateOutput("body");

        component.Process(CreateContext(), output);

        var content = output.PostContent.GetContent();
        Assert.Multiple(() =>
        {
            Assert.That(content, Does.Contain(@"<script src="""));
            Assert.That(content, Does.Not.Contain("nonce"));
        });
    }

    [Test]
    public void Badge_Omits_Nonce_When_Service_Is_Not_Registered()
    {
        PreviewBadgeTagHelperComponent component = CreateComponent(nonce: null, registerCspNonceService: false);
        TagHelperOutput output = CreateOutput("body");

        component.Process(CreateContext(), output);

        var content = output.PostContent.GetContent();
        Assert.Multiple(() =>
        {
            Assert.That(content, Does.Contain(@"<script src="""));
            Assert.That(content, Does.Not.Contain("nonce"));
        });
    }

    private static TagHelperContext CreateContext()
        => new(new TagHelperAttributeList(), new Dictionary<object, object>(), Guid.NewGuid().ToString());

    private static TagHelperOutput CreateOutput(string tagName)
        => new(
            tagName,
            new TagHelperAttributeList(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

    private static PreviewBadgeTagHelperComponent CreateComponent(
        string? nonce,
        bool registerCspNonceService = true,
        bool inPreviewMode = true,
        string? previewBadge = null)
    {
        var umbracoContext = new Mock<IUmbracoContext>();
        umbracoContext.Setup(x => x.InPreviewMode).Returns(inPreviewMode);
        umbracoContext.Setup(x => x.PublishedRequest).Returns(Mock.Of<IPublishedRequest>());

        IUmbracoContext? outContext = umbracoContext.Object;
        var umbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        umbracoContextAccessor.Setup(x => x.TryGetUmbracoContext(out outContext)).Returns(true);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost");
        httpContext.Request.Path = "/";
        httpContext.Response.ContentType = "text/html";

        var contentSettings = new ContentSettings();
        if (previewBadge is not null)
        {
            contentSettings.PreviewBadge = previewBadge;
        }

        return new PreviewBadgeTagHelperComponent(
            umbracoContextAccessor.Object,
            Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext),
            Mock.Of<IHostingEnvironment>(x => x.ToAbsolute(It.IsAny<string>()) == "/umbraco"),
            Mock.Of<IOptionsMonitor<ContentSettings>>(x => x.CurrentValue == contentSettings),
            registerCspNonceService ? Mock.Of<ICspNonceService>(x => x.GetNonce() == nonce) : null);
    }
}
