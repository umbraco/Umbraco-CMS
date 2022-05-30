// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Web.Website.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Controllers;

[TestFixture]
public class RenderNoContentControllerTests
{
    [Test]
    public void Redirects_To_Root_When_Content_Published()
    {
        var mockUmbracoContext = new Mock<IUmbracoContext>();
        mockUmbracoContext.Setup(x => x.Content.HasContent()).Returns(true);
        var mockHostingEnvironment = new Mock<IHostingEnvironment>();
        var controller = new RenderNoContentController(new TestUmbracoContextAccessor(mockUmbracoContext.Object),
            new TestOptionsSnapshot<GlobalSettings>(new GlobalSettings()), mockHostingEnvironment.Object);

        var result = controller.Index() as RedirectResult;

        Assert.IsNotNull(result);
        Assert.AreEqual("~/", result.Url);
    }

    [Test]
    public void Renders_View_When_No_Content_Published()
    {
        const string UmbracoPathSetting = "~/umbraco";
        const string UmbracoPath = "/umbraco";
        const string ViewPath = "~/config/splashes/NoNodes.cshtml";
        var mockUmbracoContext = new Mock<IUmbracoContext>();
        mockUmbracoContext.Setup(x => x.Content.HasContent()).Returns(false);
        var mockIOHelper = new Mock<IIOHelper>();
        mockIOHelper.Setup(x => x.ResolveUrl(It.Is<string>(y => y == UmbracoPathSetting))).Returns(UmbracoPath);
        var mockHostingEnvironment = new Mock<IHostingEnvironment>();
        mockHostingEnvironment.Setup(x => x.ToAbsolute(It.Is<string>(y => y == UmbracoPathSetting)))
            .Returns(UmbracoPath);


        var globalSettings = new TestOptionsSnapshot<GlobalSettings>(new GlobalSettings
        {
            UmbracoPath = UmbracoPathSetting,
            NoNodesViewPath = ViewPath
        });
        var controller = new RenderNoContentController(new TestUmbracoContextAccessor(mockUmbracoContext.Object),
            globalSettings, mockHostingEnvironment.Object);

        var result = controller.Index() as ViewResult;
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewPath, result.ViewName);

        var model = result.Model as NoNodesViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(UmbracoPath, model.UmbracoPath);
    }
}
