using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class PublishedRouterTests
{
    private PublishedRouter CreatePublishedRouter(IUmbracoContextAccessor umbracoContextAccessor)
        => new(
            Mock.Of<IOptionsMonitor<WebRoutingSettings>>(x => x.CurrentValue == new WebRoutingSettings()),
            new ContentFinderCollection(() => Enumerable.Empty<IContentFinder>()),
            new TestLastChanceFinder(),
            new TestVariationContextAccessor(),
            Mock.Of<IProfilingLogger>(),
            Mock.Of<ILogger<PublishedRouter>>(),
            Mock.Of<IPublishedUrlProvider>(),
            Mock.Of<IRequestAccessor>(),
            Mock.Of<IPublishedValueFallback>(),
            Mock.Of<IFileService>(),
            Mock.Of<IContentTypeService>(),
            umbracoContextAccessor,
            Mock.Of<IEventAggregator>());

    private IUmbracoContextAccessor GetUmbracoContextAccessor()
    {
        var uri = new Uri("http://example.com");
        var umbracoContext = Mock.Of<IUmbracoContext>(x => x.CleanedUmbracoUrl == uri);
        var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);
        return umbracoContextAccessor;
    }

    [Test]
    public async Task ConfigureRequest_Returns_False_Without_HasPublishedContent()
    {
        var umbracoContextAccessor = GetUmbracoContextAccessor();
        var publishedRouter = CreatePublishedRouter(umbracoContextAccessor);
        var request =
            await publishedRouter.CreateRequestAsync(umbracoContextAccessor.GetRequiredUmbracoContext()
                .CleanedUmbracoUrl);
        var result = publishedRouter.BuildRequest(request);

        Assert.IsFalse(result.Success());
    }

    [Test]
    public async Task ConfigureRequest_Returns_False_When_IsRedirect()
    {
        var umbracoContextAccessor = GetUmbracoContextAccessor();
        var publishedRouter = CreatePublishedRouter(umbracoContextAccessor);
        var request =
            await publishedRouter.CreateRequestAsync(umbracoContextAccessor.GetRequiredUmbracoContext()
                .CleanedUmbracoUrl);
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
        pc.Setup(content => content.ContentType)
            .Returns(new PublishedContentType(
                Guid.NewGuid(),
                22,
                "anything",
                PublishedItemType.Content,
                Enumerable.Empty<string>(),
                Enumerable.Empty<PublishedPropertyType>(),
                ContentVariation.Nothing));
        return pc;
    }
}
