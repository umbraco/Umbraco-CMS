using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Web.Security;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    public class PublishedContentRequestEngineTests : BaseRoutingTest
    {
        [Test]
        public void Ctor_Throws_On_Null_PCR()
        {
            Assert.Throws<ArgumentException>(() => new PublishedContentRequestEngine(                
                Mock.Of<IWebRoutingSection>(),
                null));
        }

        [Test]
        public void ConfigureRequest_Returns_False_Without_HasPublishedContent()
        {
            var routeCtx = GetRoutingContext("/test");

            var pcre = new PublishedContentRequestEngine(
                Mock.Of<IWebRoutingSection>(),
                new PublishedContentRequest(
                    routeCtx.UmbracoContext.CleanedUmbracoUrl, 
                    routeCtx,
                    Mock.Of<IWebRoutingSection>(),
                    s => new string[] { }));

            var result = pcre.ConfigureRequest();
            Assert.IsFalse(result);
        }

        [Test]
        public void ConfigureRequest_Returns_False_When_IsRedirect()
        {
            var routeCtx = GetRoutingContext("/test");

            var pcr = new PublishedContentRequest(routeCtx.UmbracoContext.CleanedUmbracoUrl, routeCtx, Mock.Of<IWebRoutingSection>(), s => new string[] { });
            var pc = GetPublishedContentMock();            
            pcr.PublishedContent = pc.Object;
            pcr.Culture = new CultureInfo("en-AU");
            pcr.SetRedirect("/hello");
            var pcre = new PublishedContentRequestEngine(
                Mock.Of<IWebRoutingSection>(),
                pcr);

            var result = pcre.ConfigureRequest();
            Assert.IsFalse(result);
        }

        [Test]
        public void ConfigureRequest_Adds_HttpContext_Items_When_Published_Content_Assigned()
        {
            var routeCtx = GetRoutingContext("/test");

            var pcr = new PublishedContentRequest(routeCtx.UmbracoContext.CleanedUmbracoUrl, routeCtx, Mock.Of<IWebRoutingSection>(), s => new string[] { });
            var pc = GetPublishedContentMock();
            pcr.PublishedContent = pc.Object;
            pcr.Culture = new CultureInfo("en-AU");
            var pcre = new PublishedContentRequestEngine(
                Mock.Of<IWebRoutingSection>(),
                pcr);

            pcre.ConfigureRequest();
            
            Assert.AreEqual(1, routeCtx.UmbracoContext.HttpContext.Items["pageID"]);
            Assert.AreEqual(pcr.UmbracoPage.Elements.Count, ((Hashtable)routeCtx.UmbracoContext.HttpContext.Items["pageElements"]).Count);
        }

        [Test]
        public void ConfigureRequest_Sets_UmbracoPage_When_Published_Content_Assigned()
        {
            var routeCtx = GetRoutingContext("/test");

            var pcr = new PublishedContentRequest(routeCtx.UmbracoContext.CleanedUmbracoUrl, routeCtx, Mock.Of<IWebRoutingSection>(), s => new string[] { });
            var pc = GetPublishedContentMock();
            pcr.Culture = new CultureInfo("en-AU");
            pcr.PublishedContent = pc.Object;
            var pcre = new PublishedContentRequestEngine(
                Mock.Of<IWebRoutingSection>(),
                pcr);

            pcre.ConfigureRequest();

            Assert.IsNotNull(pcr.UmbracoPage);
        }

        private Mock<IPublishedContent> GetPublishedContentMock()
        {
            var pc = new Mock<IPublishedContent>();
            pc.Setup(content => content.Id).Returns(1);
            pc.Setup(content => content.Name).Returns("test");
            pc.Setup(content => content.DocumentTypeId).Returns(2);
            pc.Setup(content => content.DocumentTypeAlias).Returns("testAlias");
            pc.Setup(content => content.WriterName).Returns("admin");
            pc.Setup(content => content.CreatorName).Returns("admin");
            pc.Setup(content => content.CreateDate).Returns(DateTime.Now);
            pc.Setup(content => content.UpdateDate).Returns(DateTime.Now);
            pc.Setup(content => content.Path).Returns("-1,1");
            pc.Setup(content => content.Version).Returns(Guid.NewGuid);
            pc.Setup(content => content.Parent).Returns(() => null);
            pc.Setup(content => content.Version).Returns(Guid.NewGuid);
            pc.Setup(content => content.Properties).Returns(new Collection<IPublishedProperty>());
            return pc;
        }

    }
}