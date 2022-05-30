// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.Extensions.WebEncoders.Testing;
using Moq;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Extensions;

[TestFixture]
public class HtmlHelperExtensionMethodsTests
{
    [SetUp]
    public virtual void Initialize() =>

        // Create an empty HtmlHelper.
        _htmlHelper = new HtmlHelper(
            Mock.Of<IHtmlGenerator>(),
            Mock.Of<ICompositeViewEngine>(),
            Mock.Of<IModelMetadataProvider>(),
            Mock.Of<IViewBufferScope>(),
            new HtmlTestEncoder(),
            UrlEncoder.Default);

    private const string SampleWithAnchorElement = "Hello world, this is some text <a href='blah'>with a link</a>";

    private const string SampleWithBoldAndAnchorElements =
        "Hello world, <b>this</b> is some text <a href='blah'>with a link</a>";

    private HtmlHelper _htmlHelper;

    [Test]
    public void Truncate_Simple()
    {
        var result = _htmlHelper.Truncate(SampleWithAnchorElement, 25).ToString();

        Assert.AreEqual("Hello world, this is some&hellip;", result);
    }

    [Test]
    public void When_Truncating_A_String_Ends_With_A_Space_We_Should_Trim_The_Space_Before_Appending_The_Ellipsis()
    {
        var result = _htmlHelper.Truncate(SampleWithAnchorElement, 26).ToString();

        Assert.AreEqual("Hello world, this is some&hellip;", result);
    }

    [Test]
    public void Truncate_Inside_Word()
    {
        var result = _htmlHelper.Truncate(SampleWithAnchorElement, 24).ToString();

        Assert.AreEqual("Hello world, this is som&hellip;", result);
    }

    [Test]
    public void Truncate_With_Tag()
    {
        var result = _htmlHelper.Truncate(SampleWithAnchorElement, 35).ToString();

        Assert.AreEqual("Hello world, this is some text <a href='blah'>with&hellip;</a>", result);
    }

    [Test]
    public void Truncate_By_Words()
    {
        var result = _htmlHelper.TruncateByWords(SampleWithAnchorElement, 4).ToString();

        Assert.AreEqual("Hello world, this is&hellip;", result);
    }

    [Test]
    public void Truncate_By_Words_With_Tag()
    {
        var result = _htmlHelper.TruncateByWords(SampleWithBoldAndAnchorElements, 4).ToString();

        Assert.AreEqual("Hello world, <b>this</b> is&hellip;", result);
    }

    [Test]
    public void Truncate_By_Words_Mid_Tag()
    {
        var result = _htmlHelper.TruncateByWords(SampleWithAnchorElement, 7).ToString();

        Assert.AreEqual("Hello world, this is some text <a href='blah'>with&hellip;</a>", result);
    }

    [Test]
    public void Strip_All_Html()
    {
        var result = _htmlHelper.StripHtml(SampleWithBoldAndAnchorElements, null).ToString();

        Assert.AreEqual("Hello world, this is some text with a link", result);
    }

    [Test]
    public void Strip_Specific_Html()
    {
        string[] tags = { "b" };

        var result = _htmlHelper.StripHtml(SampleWithBoldAndAnchorElements, tags).ToString();

        Assert.AreEqual(SampleWithAnchorElement, result);
    }

    [Test]
    public void Strip_Invalid_Html()
    {
        const string text = "Hello world, <bthis</b> is some text <a href='blah'>with a link</a>";

        var result = _htmlHelper.StripHtml(text).ToString();

        Assert.AreEqual("Hello world, is some text with a link", result);
    }
}
