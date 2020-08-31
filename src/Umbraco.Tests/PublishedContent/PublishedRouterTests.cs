using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    public class PublishedRouterTests : BaseWebTest
    {
        [Test]
        public void ConfigureRequest_Returns_False_Without_HasPublishedContent()
        {
            var umbracoContext = GetUmbracoContext("/test");
            var publishedRouter = CreatePublishedRouter();
            var request = publishedRouter.CreateRequest(umbracoContext);
            var result = publishedRouter.ConfigureRequest(request);

            Assert.IsFalse(result);
        }

        [Test]
        public void ConfigureRequest_Returns_False_When_IsRedirect()
        {
            var umbracoContext = GetUmbracoContext("/test");
            var publishedRouter = CreatePublishedRouter();
            var request = publishedRouter.CreateRequest(umbracoContext);
            var content = GetPublishedContentMock();
            request.PublishedContent = content.Object;
            request.Culture = new CultureInfo("en-AU");
            request.SetRedirect("/hello");
            var result = publishedRouter.ConfigureRequest(request);

            Assert.IsFalse(result);
        }

        [Test]
        public void ConfigureRequest_Sets_UmbracoPage_When_Published_Content_Assigned()
        {
            var umbracoContext = GetUmbracoContext("/test");
            var publishedRouter = CreatePublishedRouter();
            var request = publishedRouter.CreateRequest(umbracoContext);
            var content = GetPublishedContentMock();
            request.Culture = new CultureInfo("en-AU");
            request.PublishedContent = content.Object;
            publishedRouter.ConfigureRequest(request);

            Assert.IsNotNull(request.LegacyContentHashTable);
        }

        private Mock<IPublishedContent> GetPublishedContentMock()
        {
            var pc = new Mock<IPublishedContent>();
            pc.Setup(content => content.Id).Returns(1);
            pc.Setup(content => content.Name).Returns("test");
            pc.Setup(content => content.WriterName).Returns("admin");
            pc.Setup(content => content.CreatorName).Returns("admin");
            pc.Setup(content => content.CreateDate).Returns(DateTime.Now);
            pc.Setup(content => content.UpdateDate).Returns(DateTime.Now);
            pc.Setup(content => content.Path).Returns("-1,1");
            pc.Setup(content => content.Parent).Returns(() => null);
            pc.Setup(content => content.Properties).Returns(new Collection<IPublishedProperty>());
            pc.Setup(content => content.ContentType).Returns(new PublishedContentType(Guid.NewGuid(), 22, "anything", PublishedItemType.Content, Enumerable.Empty<string>(), Enumerable.Empty<PublishedPropertyType>(), ContentVariation.Nothing));
            return pc;
        }
    }
}
