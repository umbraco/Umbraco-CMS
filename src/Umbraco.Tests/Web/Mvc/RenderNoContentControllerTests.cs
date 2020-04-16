using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Tests.Common;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Tests.Web.Mvc
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
            var mockGlobalSettings = new Mock<IGlobalSettings>();
            var controller = new RenderNoContentController(new TestUmbracoContextAccessor(mockUmbracoContext.Object), mockIOHelper.Object, mockGlobalSettings.Object);

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
            var mockGlobalSettings = new Mock<IGlobalSettings>();
            mockGlobalSettings.SetupGet(x => x.UmbracoPath).Returns(UmbracoPathSetting);
            mockGlobalSettings.SetupGet(x => x.NoNodesViewPath).Returns(ViewPath);
            var controller = new RenderNoContentController(new TestUmbracoContextAccessor(mockUmbracoContext.Object), mockIOHelper.Object, mockGlobalSettings.Object);

            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewPath, result.ViewName);

            var model = result.Model as NoNodesViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(UmbracoPath, model.UmbracoPath);
        }
    }
}
