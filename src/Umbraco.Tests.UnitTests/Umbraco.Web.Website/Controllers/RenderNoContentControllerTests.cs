// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Web;
using Umbraco.Tests.Common;
using Umbraco.Web;
using Umbraco.Web.Website.Controllers;
using Umbraco.Web.Website.Models;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Website.Controllers
{
    [TestFixture]
    public class RenderNoContentControllerTests
    {
        [Test]
        public void Redirects_To_Root_When_Content_Published()
        {
            var mockUmbracoContext = new Mock<IUmbracoContext>();
            mockUmbracoContext.Setup(x => x.Content.HasContent()).Returns(true);
            var mockIOHelper = new Mock<IIOHelper>();
            var controller = new RenderNoContentController(new TestUmbracoContextAccessor(mockUmbracoContext.Object), mockIOHelper.Object, Options.Create(new GlobalSettings()));

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

            IOptions<GlobalSettings> globalSettings = Options.Create(new GlobalSettings()
            {
                UmbracoPath = UmbracoPathSetting,
                NoNodesViewPath = ViewPath,
            });
            var controller = new RenderNoContentController(new TestUmbracoContextAccessor(mockUmbracoContext.Object), mockIOHelper.Object, globalSettings);

            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewPath, result.ViewName);

            var model = result.Model as NoNodesViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(UmbracoPath, model.UmbracoPath);
        }
    }
}
