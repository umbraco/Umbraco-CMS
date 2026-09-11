using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Web.Routing;

[TestFixture]
public class PublishedRequestBuilderTests
{
    private readonly Uri _baseUri = new("https://example.com");

    private IPublishedRequestBuilder GetBuilder() => new PublishedRequestBuilder(
        _baseUri,
        Mock.Of<ITemplateService>());

    [Test]
    public void Setting_Published_Content_Clears_Template_And_Redirect()
    {
        var sut = GetBuilder();
        sut.SetTemplate(Mock.Of<ITemplate>());

        Assert.That(sut.Template, Is.Not.Null);

        sut.SetInternalRedirect(Mock.Of<IPublishedContent>());

        Assert.That(sut.Template, Is.Null);
        Assert.That(sut.IsInternalRedirect, Is.True);

        sut.SetTemplate(Mock.Of<ITemplate>());
        sut.SetPublishedContent(Mock.Of<IPublishedContent>());

        Assert.That(sut.Template, Is.Null);
        Assert.That(sut.IsInternalRedirect, Is.False);
    }

    [Test]
    public void Setting_Domain_Also_Sets_Culture()
    {
        var sut = GetBuilder();

        Assert.That(sut.Culture, Is.Null);

        sut.SetDomain(
            new DomainAndUri(
                new Domain(1, "test", 2, "en-AU", false, 0), new Uri("https://example.com/en-au")));

        Assert.That(sut.Domain, Is.Not.Null);
        Assert.That(sut.Culture, Is.Not.Null);
    }

    [Test]
    public void Builds_All_Values()
    {
        var sut = GetBuilder();

        var content = Mock.Of<IPublishedContent>(x => x.Id == 1);
        var template = Mock.Of<ITemplate>(x => x.Id == 1);
        string[] cacheExt = { "must-revalidate" };
        var auCulture = "en-AU";
        var usCulture = "en-US";
        var domain = new DomainAndUri(
            new Domain(1, "test", 2, auCulture, false, 0), new Uri("https://example.com/en-au"));
        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string> { ["Hello"] = "world" };
        var redirect = "https://test.com";

        sut
            .SetNoCacheHeader(true)
            .SetCacheExtensions(cacheExt)
            .SetDomain(domain)
            .SetCulture(usCulture)
            .SetHeaders(headers)
            .SetInternalRedirect(content)
            .SetRedirect(redirect)
            .SetTemplate(template);

        var request = sut.Build();

        Assert.That(request.SetNoCacheHeader, Is.EqualTo(true));
        Assert.That(request.CacheExtensions, Is.EqualTo(cacheExt));
        Assert.That(request.Culture, Is.EqualTo(usCulture));
        Assert.That(request.Domain, Is.EqualTo(domain));
        Assert.That(request.Headers, Is.EqualTo(headers));
        Assert.That(request.IsInternalRedirect, Is.EqualTo(true));
        Assert.That(request.PublishedContent, Is.EqualTo(content));
        Assert.That(request.RedirectUrl, Is.EqualTo(redirect));
        Assert.That(request.ResponseStatusCode, Is.EqualTo(302));
        Assert.That(request.Template, Is.EqualTo(template));
        Assert.That(request.Uri, Is.EqualTo(_baseUri));
    }
}
