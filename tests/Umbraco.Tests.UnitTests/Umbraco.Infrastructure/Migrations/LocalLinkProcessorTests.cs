// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations;

[TestFixture]
public class LocalLinkProcessorTests
{
    /// <summary>
    /// Verifies that a UDI-based document link with no hyphens in the GUID
    /// is converted to the new format with a hyphenated GUID and type attribute.
    /// </summary>
    [Test]
    public void ProcessStringValue_UdiDocumentLink_ConvertsToGuidWithType()
    {
        var processor = CreateProcessor();

        var input = @"<a href=""{localLink:umb://document/ac2038d9dfc24294b7787edf90d1a178}"">link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            @"<a href=""{localLink:ac2038d9-dfc2-4294-b778-7edf90d1a178}"" type=""document"">link</a>",
            result);
    }

    /// <summary>
    /// Verifies that a UDI-based media link with no hyphens in the GUID
    /// is converted to the new format with a hyphenated GUID and type attribute.
    /// </summary>
    [Test]
    public void ProcessStringValue_UdiMediaLink_ConvertsToGuidWithType()
    {
        var processor = CreateProcessor();

        var input = @"<a href=""{localLink:umb://media/7e21a725b9054c5f86dc8c41ec116e39}"">image</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            @"<a href=""{localLink:7e21a725-b905-4c5f-86dc-8c41ec116e39}"" type=""media"">image</a>",
            result);
    }

    /// <summary>
    /// Verifies that a localLink with a leading slash is converted correctly.
    /// </summary>
    [Test]
    public void ProcessStringValue_LeadingSlash_ConvertsToGuidWithType()
    {
        var processor = CreateProcessor();

        var input = @"<a href=""/{localLink:umb://document/ac2038d9dfc24294b7787edf90d1a178}"">link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            @"<a href=""/{localLink:ac2038d9-dfc2-4294-b778-7edf90d1a178}"" type=""document"">link</a>",
            result);
    }

    /// <summary>
    /// Verifies that an integer-based document link is resolved via IIdKeyMap
    /// and converted to the new GUID-based format with a type attribute.
    /// </summary>
    [Test]
    public void ProcessStringValue_IntegerDocumentLink_ConvertsToGuidWithType()
    {
        var documentKey = Guid.NewGuid();
        var idKeyMap = new Mock<IIdKeyMap>();
        idKeyMap
            .Setup(x => x.GetKeyForId(1234, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(documentKey));

        var processor = CreateProcessor(idKeyMap.Object);

        var input = @"<a href=""{localLink:1234}"">link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            $@"<a href=""{{localLink:{documentKey}}}"" type=""Document"">link</a>",
            result);
    }

    /// <summary>
    /// Verifies that an integer-based link that resolves to media (not document)
    /// is converted with the correct "Media" type attribute.
    /// </summary>
    [Test]
    public void ProcessStringValue_IntegerMediaLink_ConvertsToGuidWithType()
    {
        var mediaKey = Guid.NewGuid();
        var idKeyMap = new Mock<IIdKeyMap>();
        idKeyMap
            .Setup(x => x.GetKeyForId(5678, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Fail());
        idKeyMap
            .Setup(x => x.GetKeyForId(5678, UmbracoObjectTypes.Media))
            .Returns(Attempt<Guid>.Succeed(mediaKey));

        var processor = CreateProcessor(idKeyMap.Object);

        var input = @"<a href=""{localLink:5678}"">link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            $@"<a href=""{{localLink:{mediaKey}}}"" type=""Media"">link</a>",
            result);
    }

    /// <summary>
    /// Verifies that an integer-based link that cannot be resolved to either
    /// a document or media key is left unchanged in the output.
    /// </summary>
    [Test]
    public void ProcessStringValue_UnresolvableIntegerLink_LeftUnchanged()
    {
        var idKeyMap = new Mock<IIdKeyMap>();
        idKeyMap
            .Setup(x => x.GetKeyForId(9999, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Fail());
        idKeyMap
            .Setup(x => x.GetKeyForId(9999, UmbracoObjectTypes.Media))
            .Returns(Attempt<Guid>.Fail());

        var processor = CreateProcessor(idKeyMap.Object);

        var input = @"<a href=""{localLink:9999}"">link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(input, result);
    }

    /// <summary>
    /// Verifies that input with no local links is returned unchanged.
    /// </summary>
    [Test]
    public void ProcessStringValue_NoLocalLinks_ReturnsInputUnchanged()
    {
        var processor = CreateProcessor();

        var input = @"<a href=""https://example.com"">external</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(input, result);
    }

    /// <summary>
    /// Verifies that multiple local links in the same input are all converted.
    /// </summary>
    [Test]
    public void ProcessStringValue_MultipleLinks_ConvertsAll()
    {
        var processor = CreateProcessor();

        var input =
            @"<a href=""{localLink:umb://document/ac2038d9dfc24294b7787edf90d1a178}"">doc</a>" +
            @"<a href=""{localLink:umb://media/7e21a725b9054c5f86dc8c41ec116e39}"">img</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            @"<a href=""{localLink:ac2038d9-dfc2-4294-b778-7edf90d1a178}"" type=""document"">doc</a>" +
            @"<a href=""{localLink:7e21a725-b905-4c5f-86dc-8c41ec116e39}"" type=""media"">img</a>",
            result);
    }

    /// <summary>
    /// Verifies that URL-encoded braces (%7B / %7D) are handled and converted.
    /// </summary>
    [Test]
    public void ProcessStringValue_UrlEncodedBraces_ConvertsToGuidWithType()
    {
        var processor = CreateProcessor();

        var input = @"<a href=""%7BlocalLink:umb://document/ac2038d9dfc24294b7787edf90d1a178%7D"">link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            @"<a href=""%7BlocalLink:ac2038d9-dfc2-4294-b778-7edf90d1a178%7D"" type=""document"">link</a>",
            result);
    }

    /// <summary>
    /// Verifies that existing attributes on the anchor tag (title, rel, class, etc.)
    /// are preserved after the local link conversion.
    /// </summary>
    [Test]
    public void ProcessStringValue_ExistingAttributes_PreservedAfterConversion()
    {
        var processor = CreateProcessor();

        var input = @"<a href=""{localLink:umb://document/ac2038d9dfc24294b7787edf90d1a178}"" title=""My Page"" rel=""noopener"" class=""btn"">link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            @"<a href=""{localLink:ac2038d9-dfc2-4294-b778-7edf90d1a178}"" type=""document"" title=""My Page"" rel=""noopener"" class=""btn"">link</a>",
            result);
    }

    /// <summary>
    /// When a local link href contains a fragment identifier (#section-99) after the closing brace,
    /// the fragment should be preserved in the href and the type attribute should be a separate attribute.
    /// This is an initially failing test for https://github.com/umbraco/Umbraco-CMS/issues/22152.
    /// </summary>
    [Test]
    public void ProcessStringValue_UdiDocumentLink_WithFragment_PreservesFragment()
    {
        var processor = CreateProcessor();

        var input = @"<a href=""/{localLink:umb://document/ac2038d9dfc24294b7787edf90d1a178}#section-99"">link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            @"<a href=""/{localLink:ac2038d9-dfc2-4294-b778-7edf90d1a178}#section-99"" type=""document"">link</a>",
            result);
    }

    /// <summary>
    /// Verifies that a query string after the localLink closing brace is preserved in the href
    /// and not merged into the type attribute value.
    /// </summary>
    [Test]
    public void ProcessStringValue_UdiDocumentLink_WithQueryString_PreservesQueryString()
    {
        var processor = CreateProcessor();

        var input = @"<a href=""/{localLink:umb://document/ac2038d9dfc24294b7787edf90d1a178}?foo=bar"">link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            @"<a href=""/{localLink:ac2038d9-dfc2-4294-b778-7edf90d1a178}?foo=bar"" type=""document"">link</a>",
            result);
    }

    /// <summary>
    /// Verifies that a fragment identifier on a media UDI link is preserved in the href.
    /// </summary>
    [Test]
    public void ProcessStringValue_UdiMediaLink_WithFragment_PreservesFragment()
    {
        var processor = CreateProcessor();

        var input = @"<a href=""{localLink:umb://media/7e21a725b9054c5f86dc8c41ec116e39}#top"">image</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            @"<a href=""{localLink:7e21a725-b905-4c5f-86dc-8c41ec116e39}#top"" type=""media"">image</a>",
            result);
    }

    /// <summary>
    /// Verifies that both a query string and a fragment identifier after the localLink
    /// closing brace are preserved in the correct position in the href.
    /// </summary>
    [Test]
    public void ProcessStringValue_UdiDocumentLink_WithQueryStringAndFragment_PreservesBoth()
    {
        var processor = CreateProcessor();

        var input = @"<a href=""/{localLink:umb://document/ac2038d9dfc24294b7787edf90d1a178}?v=1#section"">link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            @"<a href=""/{localLink:ac2038d9-dfc2-4294-b778-7edf90d1a178}?v=1#section"" type=""document"">link</a>",
            result);
    }

    /// <summary>
    /// Verifies that a fragment identifier on an integer-based link is preserved.
    /// This exercises the IntId code path (distinct from the UDI path) with trailing content.
    /// </summary>
    [Test]
    public void ProcessStringValue_IntegerLink_WithFragment_PreservesFragment()
    {
        var documentKey = Guid.NewGuid();
        var idKeyMap = new Mock<IIdKeyMap>();
        idKeyMap
            .Setup(x => x.GetKeyForId(1234, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(documentKey));

        var processor = CreateProcessor(idKeyMap.Object);

        var input = @"<a href=""{localLink:1234}#anchor"">link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            $@"<a href=""{{localLink:{documentKey}}}#anchor"" type=""Document"">link</a>",
            result);
    }

    /// <summary>
    /// Verifies that a query string on an integer-based link is preserved.
    /// </summary>
    [Test]
    public void ProcessStringValue_IntegerLink_WithQueryString_PreservesQueryString()
    {
        var documentKey = Guid.NewGuid();
        var idKeyMap = new Mock<IIdKeyMap>();
        idKeyMap
            .Setup(x => x.GetKeyForId(1234, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(documentKey));

        var processor = CreateProcessor(idKeyMap.Object);

        var input = @"<a href=""{localLink:1234}?page=2"">link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            $@"<a href=""{{localLink:{documentKey}}}?page=2"" type=""Document"">link</a>",
            result);
    }

    /// <summary>
    /// Verifies that when multiple links are present and only some have fragments,
    /// each link is converted independently without corrupting its neighbours.
    /// </summary>
    [Test]
    public void ProcessStringValue_MultipleLinks_OnlyOneWithFragment_ConvertsAllCorrectly()
    {
        var processor = CreateProcessor();

        var input =
            @"<a href=""{localLink:umb://document/ac2038d9dfc24294b7787edf90d1a178}"">no fragment</a>" +
            @"<a href=""{localLink:umb://media/7e21a725b9054c5f86dc8c41ec116e39}#top"">with fragment</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            @"<a href=""{localLink:ac2038d9-dfc2-4294-b778-7edf90d1a178}"" type=""document"">no fragment</a>" +
            @"<a href=""{localLink:7e21a725-b905-4c5f-86dc-8c41ec116e39}#top"" type=""media"">with fragment</a>",
            result);
    }

    /// <summary>
    /// Verifies that an empty fragment (just '#' with nothing after it) is preserved.
    /// </summary>
    [Test]
    public void ProcessStringValue_EmptyFragment_PreservesHashSign()
    {
        var processor = CreateProcessor();

        var input = @"<a href=""{localLink:umb://document/ac2038d9dfc24294b7787edf90d1a178}#"">link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            @"<a href=""{localLink:ac2038d9-dfc2-4294-b778-7edf90d1a178}#"" type=""document"">link</a>",
            result);
    }

    /// <summary>
    /// Verifies that an empty query string (just '?' with nothing after it) is preserved.
    /// </summary>
    [Test]
    public void ProcessStringValue_EmptyQueryString_PreservesQuestionMark()
    {
        var processor = CreateProcessor();

        var input = @"<a href=""{localLink:umb://document/ac2038d9dfc24294b7787edf90d1a178}?"">link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            @"<a href=""{localLink:ac2038d9-dfc2-4294-b778-7edf90d1a178}?"" type=""document"">link</a>",
            result);
    }

    /// <summary>
    /// Verifies that a single-quoted href attribute is converted correctly.
    /// </summary>
    [Test]
    public void ProcessStringValue_SingleQuotedHref_ConvertsToGuidWithType()
    {
        var processor = CreateProcessor();

        var input = "<a href='{localLink:umb://document/ac2038d9dfc24294b7787edf90d1a178}'>link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            "<a href='{localLink:ac2038d9-dfc2-4294-b778-7edf90d1a178}' type=\"document\">link</a>",
            result);
    }

    /// <summary>
    /// Verifies that a single-quoted href with a fragment is handled correctly,
    /// preserving the fragment in the href and placing the type as a separate attribute.
    /// </summary>
    [Test]
    public void ProcessStringValue_SingleQuotedHref_WithFragment_PreservesFragment()
    {
        var processor = CreateProcessor();

        var input = "<a href='{localLink:umb://document/ac2038d9dfc24294b7787edf90d1a178}#anchor'>link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            "<a href='{localLink:ac2038d9-dfc2-4294-b778-7edf90d1a178}#anchor' type=\"document\">link</a>",
            result);
    }

    /// <summary>
    /// Verifies that a single-quoted href followed by a double-quoted attribute (e.g. title)
    /// correctly identifies the closing single quote rather than the double quote from the
    /// next attribute. Without this, the closing-quote detection would match the wrong quote
    /// and corrupt the tag.
    /// </summary>
    [Test]
    public void ProcessStringValue_SingleQuotedHref_WithDoubleQuotedAttributeAfter_DoesNotCorrupt()
    {
        var processor = CreateProcessor();

        var input = "<a href='{localLink:umb://document/ac2038d9dfc24294b7787edf90d1a178}#anchor' title=\"My Page\">link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            "<a href='{localLink:ac2038d9-dfc2-4294-b778-7edf90d1a178}#anchor' type=\"document\" title=\"My Page\">link</a>",
            result);
    }

    /// <summary>
    /// Verifies that a single-quoted href without a fragment but followed by a double-quoted
    /// attribute still works correctly.
    /// </summary>
    [Test]
    public void ProcessStringValue_SingleQuotedHref_NoFragment_WithDoubleQuotedAttributeAfter()
    {
        var processor = CreateProcessor();

        var input = "<a href='{localLink:umb://document/ac2038d9dfc24294b7787edf90d1a178}' title=\"My Page\">link</a>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            "<a href='{localLink:ac2038d9-dfc2-4294-b778-7edf90d1a178}' type=\"document\" title=\"My Page\">link</a>",
            result);
    }

    /// <summary>
    /// Verifies that a local link embedded within a larger HTML document (paragraphs,
    /// surrounding text) is converted without corrupting the broader HTML context.
    /// </summary>
    [Test]
    public void ProcessStringValue_EmbeddedInSurroundingHtml_PreservesContext()
    {
        var processor = CreateProcessor();

        var input = @"<p>Click <a href=""{localLink:umb://document/ac2038d9dfc24294b7787edf90d1a178}"">here</a> for more info.</p><p>Another paragraph.</p>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            @"<p>Click <a href=""{localLink:ac2038d9-dfc2-4294-b778-7edf90d1a178}"" type=""document"">here</a> for more info.</p><p>Another paragraph.</p>",
            result);
    }

    /// <summary>
    /// Verifies that a local link with a fragment embedded within a larger HTML document
    /// preserves both the fragment and the surrounding HTML context.
    /// </summary>
    [Test]
    public void ProcessStringValue_EmbeddedInSurroundingHtml_WithFragment_PreservesBoth()
    {
        var processor = CreateProcessor();

        var input = @"<p>See <a href=""{localLink:umb://document/ac2038d9dfc24294b7787edf90d1a178}#details"">details</a> below.</p>";

        var result = processor.ProcessStringValue(input);

        Assert.AreEqual(
            @"<p>See <a href=""{localLink:ac2038d9-dfc2-4294-b778-7edf90d1a178}#details"" type=""document"">details</a> below.</p>",
            result);
    }

#pragma warning disable CS0618 // Type or member is obsolete
    private LocalLinkProcessor CreateProcessor(IIdKeyMap? idKeyMap = null)
    {
        var parser = new HtmlLocalLinkParser(Mock.Of<IPublishedUrlProvider>());
        return new LocalLinkProcessor(
            parser,
            idKeyMap ?? Mock.Of<IIdKeyMap>(),
            Enumerable.Empty<ITypedLocalLinkProcessor>());
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
