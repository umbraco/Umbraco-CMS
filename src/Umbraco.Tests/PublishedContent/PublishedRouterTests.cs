using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    public class PublishedRouterTests : BaseWebTest
    {
        [Test]
        public async Task ConfigureRequest_Returns_False_Without_HasPublishedContent()
        {
            var umbracoContext = GetUmbracoContext("/test");
            var publishedRouter = CreatePublishedRouter(GetUmbracoContextAccessor(umbracoContext));
            var request = await publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);
            var result = publishedRouter.BuildRequest(request);

            Assert.IsFalse(result.Success());
        }

        [Test]
        public async Task ConfigureRequest_Returns_False_When_IsRedirect()
        {
            var umbracoContext = GetUmbracoContext("/test");
            var publishedRouter = CreatePublishedRouter(GetUmbracoContextAccessor(umbracoContext));
            var request = await publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);
            var content = GetPublishedContentMock();
            request.SetPublishedContent(content.Object);
            request.SetCulture("en-AU");
            request.SetRedirect("/hello");
            var result = publishedRouter.BuildRequest(request);

            Assert.IsFalse(result.Success());
        }

        private Mock<IPublishedContent> GetPublishedContentMock()
        {
            var pc = new Mock<IPublishedContent>();
            pc.Setup(content => content.Id).Returns(1);
            pc.Setup(content => content.Name).Returns("test");
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
