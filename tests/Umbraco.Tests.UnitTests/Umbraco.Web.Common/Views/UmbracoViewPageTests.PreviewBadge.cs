// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.ModelBinders;
using Umbraco.Cms.Web.Common.Views;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Views;

[TestFixture]
public partial class UmbracoViewPageTests
{
    [Test]
    public void Preview_Badge_Contains_Nonce_When_Available()
    {
        PreviewBadgeTestPage page = CreatePreviewBadgePage("s0m3-n0nc3");
        TagHelperOutput output = CreateBodyTagHelperOutput();

        page.WriteUmbracoContent(output);

        Assert.That(output.Content.GetContent(), Does.Contain(@"<script nonce=""s0m3-n0nc3"" src="""));
    }

    [Test]
    public void Preview_Badge_Omits_Nonce_When_Not_Available()
    {
        PreviewBadgeTestPage page = CreatePreviewBadgePage(null);
        TagHelperOutput output = CreateBodyTagHelperOutput();

        page.WriteUmbracoContent(output);

        var content = output.Content.GetContent();
        Assert.Multiple(() =>
        {
            Assert.That(content, Does.Contain(@"<script src="""));
            Assert.That(content, Does.Not.Contain("nonce"));
        });
    }

    [Test]
    public void Preview_Badge_Omits_Nonce_When_Service_Is_Not_Registered()
    {
        PreviewBadgeTestPage page = CreatePreviewBadgePage(null, registerCspNonceService: false);
        TagHelperOutput output = CreateBodyTagHelperOutput();

        page.WriteUmbracoContent(output);

        var content = output.Content.GetContent();
        Assert.Multiple(() =>
        {
            Assert.That(content, Does.Contain(@"<script src="""));
            Assert.That(content, Does.Not.Contain("nonce"));
        });
    }

    private static TagHelperOutput CreateBodyTagHelperOutput()
        => new(
            "body",
            new TagHelperAttributeList(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

    private static PreviewBadgeTestPage CreatePreviewBadgePage(string? nonce, bool registerCspNonceService = true)
    {
        var umbracoContext = new Mock<IUmbracoContext>();
        umbracoContext.Setup(x => x.InPreviewMode).Returns(true);

        var umbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        IUmbracoContext? outContext = umbracoContext.Object;
        umbracoContextAccessor.Setup(x => x.TryGetUmbracoContext(out outContext)).Returns(true);

        var services = new ServiceCollection();
        services.AddSingleton(umbracoContextAccessor.Object);
        services.AddSingleton(Options.Create(new ContentSettings()));
        services.AddSingleton(Mock.Of<IHostingEnvironment>(x => x.ToAbsolute(It.IsAny<string>()) == "/umbraco"));
        services.AddSingleton(new ContentModelBinder(Mock.Of<IEventAggregator>()));

        if (registerCspNonceService)
        {
            services.AddSingleton(Mock.Of<ICspNonceService>(x => x.GetNonce() == nonce));
        }

        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost");
        httpContext.Request.Path = "/";
        httpContext.Response.ContentType = "text/html";

        var viewData = new ViewDataDictionary<IPublishedContent>(
            new EmptyModelMetadataProvider(),
            new ModelStateDictionary());

        return new PreviewBadgeTestPage
        {
            ViewContext = new ViewContext
            {
                HttpContext = httpContext,
                ViewData = viewData,
            },
        };
    }

    private class PreviewBadgeTestPage : UmbracoViewPage<IPublishedContent>
    {
        public override Task ExecuteAsync() => throw new NotImplementedException();
    }
}
