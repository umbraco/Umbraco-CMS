using AutoFixture.NUnit3;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class ContentFinderByKeyTests
{
    [Test]
    [InlineAutoMoqData("/1598901d-ebbe-4996-b7fb-6a6cbac13a62", "1598901d-ebbe-4996-b7fb-6a6cbac13a62", true)]
    [InlineAutoMoqData("/1598901d-ebbe-4996-b7fb-6a6cbac13a62", "9E966427-25AB-4909-B403-DED1F421D1A7", false)]
    public async Task Lookup_By_Key(
        string urlAsString,
        string nodeKeyString,
        bool shouldSucceed,
        [Frozen] IPublishedContent publishedContent,
        [Frozen] IUmbracoContextAccessor umbracoContextAccessor,
        [Frozen] IUmbracoContext umbracoContext,
        IFileService fileService)
    {
        var absoluteUrl = "http://localhost" + urlAsString;

        Guid nodeKey = Guid.Parse(nodeKeyString);

        Mock.Get(umbracoContextAccessor).Setup(x => x.TryGetUmbracoContext(out umbracoContext)).Returns(true);
        Mock.Get(umbracoContext).Setup(x => x.Content.GetById(nodeKey)).Returns(publishedContent);
        Mock.Get(publishedContent).Setup(x => x.Key).Returns(nodeKey);

        var publishedRequestBuilder = new PublishedRequestBuilder(new Uri(absoluteUrl, UriKind.Absolute), fileService);
        var webRoutingSettings = new WebRoutingSettings();

        var sut = new ContentFinderByKeyPath(
            Mock.Of<IOptionsMonitor<WebRoutingSettings>>(x => x.CurrentValue == webRoutingSettings),
            Mock.Of<ILogger<ContentFinderByKeyPath>>(),
            Mock.Of<IRequestAccessor>(),
            umbracoContextAccessor);

        await sut.TryFindContent(publishedRequestBuilder);

        if (shouldSucceed)
        {
            Assert.AreEqual(publishedRequestBuilder.PublishedContent!.Key, nodeKey);
        }
        else
        {
            Assert.AreNotEqual(publishedRequestBuilder.PublishedContent!.Key, nodeKey);
        }
    }
}
