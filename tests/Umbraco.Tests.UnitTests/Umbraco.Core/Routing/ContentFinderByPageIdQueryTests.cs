using System.Web;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class ContentFinderByPageIdQueryTests
{
    [Test]
    [InlineAutoMoqData("/?umbPageId=1046", 1046)]
    [InlineAutoMoqData("/?UMBPAGEID=1046", 1046)]
    public async Task Lookup_By_Page_Id(
        string urlAsString,
        int nodeMatch,
        [Frozen] IPublishedContent publishedContent,
        [Frozen] IUmbracoContextAccessor umbracoContextAccessor,
        [Frozen] IUmbracoContext umbracoContext,
        [Frozen] IRequestAccessor requestAccessor,
        IFileService fileService)
    {
        var absoluteUrl = "http://localhost" + urlAsString;

        Mock.Get(umbracoContextAccessor).Setup(x => x.TryGetUmbracoContext(out umbracoContext)).Returns(true);
        Mock.Get(umbracoContext).Setup(x => x.Content.GetById(nodeMatch)).Returns(publishedContent);
        Mock.Get(umbracoContext).Setup(x => x.CleanedUmbracoUrl)
            .Returns(new Uri(absoluteUrl, UriKind.Absolute));
        Mock.Get(publishedContent).Setup(x => x.Id).Returns(nodeMatch);

        var queryStrings = HttpUtility.ParseQueryString(umbracoContext.CleanedUmbracoUrl.Query);
        Mock.Get(requestAccessor).Setup(x => x.GetRequestValue("umbPageID"))
            .Returns(queryStrings["umbPageID"]);

        var publishedRequestBuilder = new PublishedRequestBuilder(new Uri(absoluteUrl, UriKind.Absolute), fileService);

        var lookup = new ContentFinderByPageIdQuery(requestAccessor, umbracoContextAccessor);

        var result = await lookup.TryFindContent(publishedRequestBuilder);

        Assert.IsTrue(result);
        Assert.AreEqual(publishedRequestBuilder.PublishedContent!.Id, nodeMatch);
    }
}
