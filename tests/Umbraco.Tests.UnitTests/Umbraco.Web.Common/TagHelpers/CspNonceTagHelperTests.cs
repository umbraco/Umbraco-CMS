// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.TagHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.TagHelpers;

[TestFixture]
public class CspNonceTagHelperTests
{
    [TestCase("script")]
    [TestCase("style")]
    public void Process_AddsNonceAttribute_WhenNonceIsAvailable(string tagName)
    {
        var expectedNonce = "test-nonce-value";
        var cspNonceService = Mock.Of<ICspNonceService>(x => x.GetNonce() == expectedNonce);
        var tagHelper = new CspNonceTagHelper(cspNonceService);

        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput(tagName);

        tagHelper.Process(context, output);

        Assert.That(output.Attributes.ContainsName("nonce"), Is.True);
        Assert.That(output.Attributes["nonce"].Value, Is.EqualTo(expectedNonce));
    }

    [TestCase(null)]
    [TestCase("")]
    public void Process_DoesNotAddNonceAttribute_WhenNonceIsNullOrEmpty(string? nonce)
    {
        var cspNonceService = Mock.Of<ICspNonceService>(x => x.GetNonce() == nonce);
        var tagHelper = new CspNonceTagHelper(cspNonceService);

        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("script");

        tagHelper.Process(context, output);

        Assert.That(output.Attributes.ContainsName("nonce"), Is.False);
    }

    [Test]
    public void Process_PreservesExistingAttributes()
    {
        var expectedNonce = "test-nonce";
        var cspNonceService = Mock.Of<ICspNonceService>(x => x.GetNonce() == expectedNonce);
        var tagHelper = new CspNonceTagHelper(cspNonceService);

        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("script");
        output.Attributes.Add("type", "module");
        output.Attributes.Add("src", "/scripts/app.js");

        tagHelper.Process(context, output);

        Assert.That(output.Attributes.ContainsName("type"), Is.True);
        Assert.That(output.Attributes["type"].Value, Is.EqualTo("module"));
        Assert.That(output.Attributes.ContainsName("src"), Is.True);
        Assert.That(output.Attributes["src"].Value, Is.EqualTo("/scripts/app.js"));
        Assert.That(output.Attributes.ContainsName("nonce"), Is.True);
        Assert.That(output.Attributes["nonce"].Value, Is.EqualTo(expectedNonce));
    }

    [Test]
    public void Process_OverwritesExistingNonceAttribute()
    {
        var expectedNonce = "new-nonce-value";
        var cspNonceService = Mock.Of<ICspNonceService>(x => x.GetNonce() == expectedNonce);
        var tagHelper = new CspNonceTagHelper(cspNonceService);

        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput("script");
        output.Attributes.Add("nonce", "old-nonce-value");

        tagHelper.Process(context, output);

        Assert.That(output.Attributes["nonce"].Value, Is.EqualTo(expectedNonce));
    }

    private static TagHelperContext CreateTagHelperContext()
        => new(
            tagName: "script",
            allAttributes: [],
            items: new Dictionary<object, object>(),
            uniqueId: Guid.NewGuid().ToString());

    private static TagHelperOutput CreateTagHelperOutput(string tagName)
        => new(
            tagName,
            attributes: [],
            getChildContentAsync: (useCachedResult, encoder) =>
                Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
}
