using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.DeliveryApi;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class RichTextParserTests : PropertyValueConverterTests
{
    private readonly Guid _contentKey = Guid.NewGuid();
    private readonly Guid _contentRootKey = Guid.NewGuid();
    private readonly string _contentType = "contentType";
    private readonly Guid _mediaKey = Guid.NewGuid();
    private readonly string _mediaType = Constants.Conventions.MediaTypes.Image;

    [Test]
    public void ParseElement_DocumentElementIsCalledRoot()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse("<p>Hello</p>", RichTextBlockModel.Empty);
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Tag, Is.EqualTo("#root"));
    }

    [Test]
    public void ParseElement_SimpleParagraphHasSingleTextElement()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse("<p>Some text paragraph</p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Elements.Count(), Is.EqualTo(1));
        var paragraph = element.Elements.Single() as RichTextGenericElement;
        Assert.That(paragraph, Is.Not.Null);
        Assert.That(paragraph.Tag, Is.EqualTo("p"));
        var textElement = paragraph.Elements.First() as RichTextTextElement;
        Assert.That(textElement, Is.Not.Null);
        Assert.That(textElement.Text, Is.EqualTo("Some text paragraph"));
    }

    [Test]
    public void ParseElement_ParagraphWithLineBreaksWrapsTextInElements()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse("<p>Some text<br/>More text<br/>Even more text</p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Elements.Count(), Is.EqualTo(1));
        var paragraph = element.Elements.Single() as RichTextGenericElement;
        Assert.That(paragraph, Is.Not.Null);
        Assert.That(paragraph.Tag, Is.EqualTo("p"));
        var paragraphElements = paragraph.Elements.ToArray();
        Assert.That(paragraphElements.Length, Is.EqualTo(5));
        for (var i = 0; i < paragraphElements.Length; i++)
        {
            var paragraphElement = paragraphElements[i] as RichTextGenericElement;
            var textElement = paragraphElements[i] as RichTextTextElement;
            switch (i)
            {
                case 0:
                    Assert.That(paragraphElement, Is.Null);
                    Assert.That(textElement, Is.Not.Null);
                    Assert.That(textElement.Tag, Is.EqualTo("#text"));
                    Assert.That(textElement.Text, Is.EqualTo("Some text"));
                    break;
                case 2:
                    Assert.That(paragraphElement, Is.Null);
                    Assert.That(textElement, Is.Not.Null);
                    Assert.That(textElement.Tag, Is.EqualTo("#text"));
                    Assert.That(textElement.Text, Is.EqualTo("More text"));
                    break;
                case 4:
                    Assert.That(paragraphElement, Is.Null);
                    Assert.That(textElement, Is.Not.Null);
                    Assert.That(textElement.Tag, Is.EqualTo("#text"));
                    Assert.That(textElement.Text, Is.EqualTo("Even more text"));
                    break;
                case 1:
                case 3:
                    Assert.That(textElement, Is.Null);
                    Assert.That(paragraphElement, Is.Not.Null);
                    Assert.That(paragraphElement.Elements, Is.Empty);
                    Assert.That(paragraphElement.Tag, Is.EqualTo("br"));
                    break;
            }
        }
    }

    [Test]
    public void ParseElement_DataAttributesAreSanitized()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse("<p><span data-something=\"the data-something value\">Text in a data-something SPAN</span></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        var span = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.That(span, Is.Not.Null);
        Assert.That(span.Tag, Is.EqualTo("span"));
        Assert.That(span.Attributes, Has.Count.EqualTo(1));
        Assert.That(span.Attributes.First().Key, Is.EqualTo("something"));
        Assert.That(span.Attributes.First().Value, Is.EqualTo("the data-something value"));
        var textElement = span.Elements.Single() as RichTextTextElement;
        Assert.That(textElement, Is.Not.Null);
        Assert.That(textElement.Text, Is.EqualTo("Text in a data-something SPAN"));
    }

    [Test]
    public void ParseElement_DataAttributesDoNotOverwriteExistingAttributes()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse("<p><span something=\"the original something\" data-something=\"the data something\">Text in a data-something SPAN</span></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        var span = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.That(span, Is.Not.Null);
        Assert.That(span.Tag, Is.EqualTo("span"));
        Assert.That(span.Attributes, Has.Count.EqualTo(1));
        Assert.That(span.Attributes.First().Key, Is.EqualTo("something"));
        Assert.That(span.Attributes.First().Value, Is.EqualTo("the original something"));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("#some-anchor")]
    [TestCase("?something=true")]
    public void ParseElement_CanParseContentLink(string? postfix)
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"/{{localLink:umb://document/{_contentKey:N}}}{postfix}\"></a></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.That(link, Is.Not.Null);
        Assert.That(link.Tag, Is.EqualTo("a"));
        Assert.That(link.Attributes, Has.Count.EqualTo(4));
        Assert.That(link.Attributes["route"], Is.Not.Null);
        var route = link.Attributes["route"] as IApiContentRoute;
        Assert.That(route, Is.Not.Null);
        Assert.That(route.Path, Is.EqualTo("/some-content-path"));
        Assert.That(route.QueryString, Is.EqualTo(postfix.NullOrWhiteSpaceAsNull()));
        Assert.That(route.StartItem.Id, Is.EqualTo(_contentRootKey));
        Assert.That(route.StartItem.Path, Is.EqualTo("the-root-path"));

        Assert.That(link.Attributes["destinationId"], Is.Not.Null);
        Assert.That(link.Attributes["destinationType"], Is.Not.Null);
        Assert.That(link.Attributes["linkType"], Is.Not.Null);
        Assert.That(Guid.Parse((link.Attributes["destinationId"] as string)!), Is.EqualTo(_contentKey));
        Assert.That(link.Attributes["destinationType"], Is.EqualTo(_contentType));
        Assert.That(link.Attributes["linkType"], Is.EqualTo(nameof(LinkType.Content)));
    }

    // PascalCase type — historic mis-cased values written by the (now fixed) ConvertLocalLinks migration for Umbraco 15 (see #22597).
    [Test]
    public void ParseElement_CanParseContentLinkWithPascalCaseTypeAttribute()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"/{{localLink:{_contentKey:N}}}\" type=\"Document\"></a></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.That(link, Is.Not.Null);
        Assert.That(link.Tag, Is.EqualTo("a"));

        Assert.That(link.Attributes["route"], Is.Not.Null);
        var route = link.Attributes["route"] as IApiContentRoute;
        Assert.That(route, Is.Not.Null);
        Assert.That(route.Path, Is.EqualTo("/some-content-path"));

        Assert.That(link.Attributes["destinationId"], Is.Not.Null);
        Assert.That(link.Attributes["destinationType"], Is.Not.Null);
        Assert.That(link.Attributes["linkType"], Is.Not.Null);
        Assert.That(Guid.Parse((link.Attributes["destinationId"] as string)!), Is.EqualTo(_contentKey));
        Assert.That(link.Attributes["destinationType"], Is.EqualTo(_contentType));
        Assert.That(link.Attributes["linkType"], Is.EqualTo(nameof(LinkType.Content)));
    }

    [Test]
    public void ParseElement_CanParseMediaLink()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"/{{localLink:umb://media/{_mediaKey:N}}}\"></a></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.That(link, Is.Not.Null);
        Assert.That(link.Tag, Is.EqualTo("a"));
        Assert.That(link.Attributes, Has.Count.EqualTo(4));
        Assert.That(link.Attributes.First().Key, Is.EqualTo("href"));
        Assert.That(link.Attributes.First().Value, Is.EqualTo("/some-media-url"));

        Assert.That(link.Attributes["destinationId"], Is.Not.Null);
        Assert.That(link.Attributes["destinationType"], Is.Not.Null);
        Assert.That(link.Attributes["linkType"], Is.Not.Null);
        Assert.That(Guid.Parse((link.Attributes["destinationId"] as string)!), Is.EqualTo(_mediaKey));
        Assert.That(link.Attributes["destinationType"], Is.EqualTo(_mediaType));
        Assert.That(link.Attributes["linkType"], Is.EqualTo(nameof(LinkType.Media)));
    }

    [Test]
    public void ParseElement_CanHandleNonLocalLink()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"https://some.where/else/\"></a></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.That(link, Is.Not.Null);
        Assert.That(link.Tag, Is.EqualTo("a"));
        Assert.That(link.Attributes, Has.Count.EqualTo(1));
        Assert.That(link.Attributes.First().Key, Is.EqualTo("href"));
        Assert.That(link.Attributes.First().Value, Is.EqualTo("https://some.where/else/"));

        Assert.That(link.Attributes.TryGetValue("destinationId", out _), Is.False);
        Assert.That(link.Attributes.TryGetValue("destinationType", out _), Is.False);
        Assert.That(link.Attributes.TryGetValue("linkType", out _), Is.False);
    }

    [TestCase("#some-anchor")]
    [TestCase("?something=true")]
    public void ParseElement_CanHandleNonLocalLink_WithPostfix(string postfix)
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"https://some.where/else/{postfix}\"></a></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.That(link, Is.Not.Null);
        Assert.That(link.Tag, Is.EqualTo("a"));
        Assert.That(link.Attributes, Has.Count.EqualTo(1));
        Assert.That(link.Attributes.First().Key, Is.EqualTo("href"));
        Assert.That(link.Attributes.First().Value, Is.EqualTo($"https://some.where/else/{postfix}"));

        Assert.That(link.Attributes.TryGetValue("destinationId", out _), Is.False);
        Assert.That(link.Attributes.TryGetValue("destinationType", out _), Is.False);
        Assert.That(link.Attributes.TryGetValue("linkType", out _), Is.False);
    }

    [Test]
    public void ParseElement_LinkTextIsWrappedInTextElement()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"https://some.where/else/\">This is the link text</a></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.That(link, Is.Not.Null);
        Assert.That(link.Tag, Is.EqualTo("a"));
        var textElement = link.Elements.Single() as RichTextTextElement;
        Assert.That(textElement, Is.Not.Null);
        Assert.That(textElement.Text, Is.EqualTo("This is the link text"));

        Assert.That(link.Attributes.TryGetValue("destinationId", out _), Is.False);
        Assert.That(link.Attributes.TryGetValue("destinationType", out _), Is.False);
        Assert.That(link.Attributes.TryGetValue("linkType", out _), Is.False);
    }

    [TestCase("{localLink:umb://document/fe5bf80d37db4373adb9b206896b4a3b}")]
    [TestCase("{localLink:umb://media/03b9a8721c4749a9a7026033ec78d860}")]
    public void ParseElement_InvalidLocalLinkYieldsEmptyLink(string href)
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"/{href}\"></a></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.That(link, Is.Not.Null);
        Assert.That(link.Tag, Is.EqualTo("a"));
        Assert.That(link.Attributes, Is.Empty);
    }

    [Test]
    public void ParseElement_CanParseMediaImage()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><img src=\"/media/whatever/something.png?rmode=max&amp;width=500\" data-udi=\"umb://media/{_mediaKey:N}\"></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.That(link, Is.Not.Null);
        Assert.That(link.Tag, Is.EqualTo("img"));
        Assert.That(link.Attributes, Has.Count.EqualTo(1));
        Assert.That(link.Attributes.First().Key, Is.EqualTo("src"));
        Assert.That(link.Attributes.First().Value, Is.EqualTo("/some-media-url"));

        Assert.That(link.Attributes.TryGetValue("destinationId", out _), Is.False);
        Assert.That(link.Attributes.TryGetValue("destinationType", out _), Is.False);
        Assert.That(link.Attributes.TryGetValue("linkType", out _), Is.False);
    }

    [Test]
    public void ParseElement_CanHandleNonLocalImage()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><img src=\"https://some.where/something.png?rmode=max&amp;width=500\"></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.That(link, Is.Not.Null);
        Assert.That(link.Tag, Is.EqualTo("img"));
        Assert.That(link.Attributes, Has.Count.EqualTo(1));
        Assert.That(link.Attributes.First().Key, Is.EqualTo("src"));
        Assert.That(link.Attributes.First().Value, Is.EqualTo("https://some.where/something.png?rmode=max&amp;width=500"));

        Assert.That(link.Attributes.TryGetValue("destinationId", out _), Is.False);
        Assert.That(link.Attributes.TryGetValue("destinationType", out _), Is.False);
        Assert.That(link.Attributes.TryGetValue("linkType", out _), Is.False);
    }

    [Test]
    public void ParseElement_RemovesComments()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse("<p>some text<!-- a comment -->some more text</p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        var paragraph = element.Elements.Single() as RichTextGenericElement;
        Assert.That(paragraph, Is.Not.Null);
        Assert.That(paragraph.Elements.Count(), Is.EqualTo(2));
        var textElements = paragraph.Elements.OfType<RichTextTextElement>().ToArray();
        Assert.That(textElements.Length, Is.EqualTo(2));
        Assert.That(textElements.First().Text, Is.EqualTo("some text"));
        Assert.That(textElements.Last().Text, Is.EqualTo("some more text"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ParseElement_CleansUpBlocks(bool inlineBlock)
    {
        var parser = CreateRichTextElementParser();
        var id = Guid.NewGuid();

        var tagName = $"umb-rte-block{(inlineBlock ? "-inline" : string.Empty)}";
        var element = parser.Parse($"<p><{tagName} data-content-key=\"{id:N}\"><!--Umbraco-Block--></{tagName}></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        var paragraph = element.Elements.Single() as RichTextGenericElement;
        Assert.That(paragraph, Is.Not.Null);
        var block = paragraph.Elements.Single() as RichTextGenericElement;
        Assert.That(block, Is.Not.Null);
        Assert.That(block.Tag, Is.EqualTo(tagName));
        Assert.That(block.Attributes, Has.Count.EqualTo(1));
        Assert.That(block.Attributes.ContainsKey("content-id"), Is.True);
        Assert.That(block.Attributes["content-id"], Is.EqualTo(id));
        Assert.That(block.Elements, Is.Empty);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ParseElement_AppendsBlocks(bool inlineBlock)
    {
        var parser = CreateRichTextElementParser();
        var block1ContentId = Guid.NewGuid();
        var block2ContentId = Guid.NewGuid();
        var block2SettingsId = Guid.NewGuid();
        RichTextBlockModel richTextBlockModel = new RichTextBlockModel(
            new List<RichTextBlockItem>
            {
                new (
                    block1ContentId,
                    CreateElement(block1ContentId, 123),
                    null,
                    null),
                new (
                    block2ContentId,
                    CreateElement(block2ContentId, 456),
                    block2SettingsId,
                    CreateElement(block2SettingsId, 789))
            });

        var tagName = $"umb-rte-block{(inlineBlock ? "-inline" : string.Empty)}";
        var element = parser.Parse($"<p><{tagName} data-content-key=\"{block1ContentId:N}\"><!--Umbraco-Block--></{tagName}><{tagName} data-content-key=\"{block2ContentId:N}\"><!--Umbraco-Block--></{tagName}></p>", richTextBlockModel) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        var paragraph = element.Elements.Single() as RichTextGenericElement;
        Assert.That(paragraph, Is.Not.Null);
        Assert.That(paragraph.Elements.Count(), Is.EqualTo(2));

        var block1Element = paragraph.Elements.First() as RichTextGenericElement;
        Assert.That(block1Element, Is.Not.Null);
        Assert.That(block1Element.Tag, Is.EqualTo(tagName));
        Assert.That(block1Element.Attributes["content-id"], Is.EqualTo(block1ContentId));

        var block2Element = paragraph.Elements.Last() as RichTextGenericElement;
        Assert.That(block2Element, Is.Not.Null);
        Assert.That(block2Element.Tag, Is.EqualTo(tagName));
        Assert.That(block2Element.Attributes["content-id"], Is.EqualTo(block2ContentId));

        Assert.That(element.Blocks.Count(), Is.EqualTo(2));

        var block1 = element.Blocks.First();
        Assert.That(block1.Content.Id, Is.EqualTo(block1ContentId));
        Assert.That(block1.Content.Properties["number"], Is.EqualTo(123));
        Assert.That(block1.Settings, Is.Null);

        var block2 = element.Blocks.Last();
        Assert.That(block2.Content.Id, Is.EqualTo(block2ContentId));
        Assert.That(block2.Content.Properties["number"], Is.EqualTo(456));
        Assert.That(block2.Settings!.Id, Is.EqualTo(block2SettingsId));
        Assert.That(block2.Settings.Properties["number"], Is.EqualTo(789));
    }

    [Test]
    public void ParseElement_CanHandleMixedInlineAndBlockLevelBlocks()
    {
        var parser = CreateRichTextElementParser();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        var element = parser.Parse($"<p><umb-rte-block-inline data-content-key=\"{id1:N}\"><!--Umbraco-Block--></umb-rte-block-inline></p><umb-rte-block data-content-key=\"{id2:N}\"><!--Umbraco-Block--></umb-rte-block>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Elements.Count(), Is.EqualTo(2));

        var paragraph = element.Elements.First() as RichTextGenericElement;
        Assert.That(paragraph, Is.Not.Null);

        var inlineBlock = paragraph.Elements.Single() as RichTextGenericElement;
        Assert.That(inlineBlock, Is.Not.Null);
        Assert.That(inlineBlock.Tag, Is.EqualTo("umb-rte-block-inline"));
        Assert.That(inlineBlock.Attributes, Has.Count.EqualTo(1));
        Assert.That(inlineBlock.Attributes.ContainsKey("content-id"), Is.True);
        Assert.That(inlineBlock.Attributes["content-id"], Is.EqualTo(id1));
        Assert.That(inlineBlock.Elements, Is.Empty);

        var blockLevelBlock = element.Elements.Last() as RichTextGenericElement;
        Assert.That(blockLevelBlock, Is.Not.Null);
        Assert.That(blockLevelBlock.Tag, Is.EqualTo("umb-rte-block"));
        Assert.That(blockLevelBlock.Attributes, Has.Count.EqualTo(1));
        Assert.That(blockLevelBlock.Attributes.ContainsKey("content-id"), Is.True);
        Assert.That(blockLevelBlock.Attributes["content-id"], Is.EqualTo(id2));
        Assert.That(blockLevelBlock.Elements, Is.Empty);
    }

    private const string TestParagraph = "What follows from <strong>here</strong> <em>is</em> <a href=\"#\">just</a> a bunch of text.";

    [Test]
    public void ParseElement_CanHandleWhitespaceAroundInlineElemements()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p>{TestParagraph}</p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        var paragraphElement = element.Elements.Single() as RichTextGenericElement;
        Assert.That(paragraphElement, Is.Not.Null);

        AssertTestParagraph(paragraphElement);
    }

    [TestCase(1, "\n")]
    [TestCase(2, "\n")]
    [TestCase(1, "\r")]
    [TestCase(2, "\r")]
    [TestCase(1, "\r\n")]
    [TestCase(2, "\r\n")]
    public void ParseElement_RemovesNewLinesAroundHtmlStructuralElements(int numberOfNewLineCharacters, string newlineCharacter)
    {
        var parser = CreateRichTextElementParser();

        var newLineSeparator = string.Concat(Enumerable.Repeat(newlineCharacter, numberOfNewLineCharacters));
        var element = parser.Parse($"<table>{newLineSeparator}<tr>{newLineSeparator}<td>{TestParagraph}</td>{newLineSeparator}</tr>{newLineSeparator}</table>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        var tableElement = element.Elements.Single() as RichTextGenericElement;
        Assert.That(tableElement, Is.Not.Null);

        var rowElement = tableElement.Elements.Single() as RichTextGenericElement;
        Assert.That(rowElement, Is.Not.Null);

        var cellElement = rowElement.Elements.Single() as RichTextGenericElement;
        Assert.That(cellElement, Is.Not.Null);

        AssertTestParagraph(cellElement);
    }

    [TestCase(1, "\n")]
    [TestCase(2, "\n")]
    [TestCase(1, "\r")]
    [TestCase(2, "\r")]
    [TestCase(1, "\r\n")]
    [TestCase(2, "\r\n")]
    public void ParseElement_RemovesNewLinesAroundHtmlContentElements(int numberOfNewLineCharacters, string newlineCharacter)
    {
        var parser = CreateRichTextElementParser();

        var newLineSeparator = string.Concat(Enumerable.Repeat(newlineCharacter, numberOfNewLineCharacters));
        var element = parser.Parse($"<div><p>{TestParagraph}</p>{newLineSeparator}<p></p>{newLineSeparator}<p>&nbsp;</p>{newLineSeparator}<p>{TestParagraph}</p></div>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.That(element, Is.Not.Null);
        var divElement = element.Elements.Single() as RichTextGenericElement;
        Assert.That(divElement, Is.Not.Null);

        var paragraphELements = divElement.Elements;
        Assert.That(paragraphELements.Count(), Is.EqualTo(4));

        AssertTestParagraph(paragraphELements.First() as RichTextGenericElement);
        AssertTestParagraph(paragraphELements.Last() as RichTextGenericElement);
    }

    private static void AssertTestParagraph(RichTextGenericElement paragraphElement)
    {
        var childElements = paragraphElement.Elements.ToArray();
        Assert.That(childElements.Length, Is.EqualTo(7));

        var childElementCounter = 0;

        void AssertNextChildElementIsText(string expectedText)
        {
            var textElement = childElements[childElementCounter++] as RichTextTextElement;
            Assert.That(textElement, Is.Not.Null);
            Assert.That(textElement.Text, Is.EqualTo(expectedText));
        }

        void AssertNextChildElementIsGeneric(string expectedTag, string expectedInnerText)
        {
            var genericElement = childElements[childElementCounter++] as RichTextGenericElement;
            Assert.That(genericElement, Is.Not.Null);
            Assert.That(genericElement.Tag, Is.EqualTo(expectedTag));
            Assert.That(genericElement.Elements.Count(), Is.EqualTo(1));
            var textElement = genericElement.Elements.First() as RichTextTextElement;
            Assert.That(textElement, Is.Not.Null);
            Assert.That(textElement.Text, Is.EqualTo(expectedInnerText));
        }

        AssertNextChildElementIsText("What follows from ");
        AssertNextChildElementIsGeneric("strong", "here");
        AssertNextChildElementIsText(" ");
        AssertNextChildElementIsGeneric("em", "is");
        AssertNextChildElementIsText(" ");
        AssertNextChildElementIsGeneric("a", "just");
        AssertNextChildElementIsText(" a bunch of text.");
    }

    [Test]
    public void ParseMarkup_CanParseContentLink()
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><a href=\"/{{localLink:{_contentKey:N}}}\" type=\"document\"></a></p>");
        Assert.That(result, Does.Contain("href=\"/some-content-path\""));
        Assert.That(result, Does.Contain($"data-destination-id=\"{_contentKey:D}\""));
        Assert.That(result, Does.Contain($"data-destination-type=\"{_contentType}\""));
        Assert.That(result, Does.Contain("data-start-item-path=\"the-root-path\""));
        Assert.That(result, Does.Contain($"data-start-item-id=\"{_contentRootKey:D}\""));
        Assert.That(result, Does.Contain($"data-link-type=\"{LinkType.Content}\""));
    }

    [Test]
    public void ParseMarkup_CanParseLegacyContentLink()
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><a href=\"/{{localLink:umb://document/{_contentKey:N}}}\"></a></p>");
        Assert.That(result, Does.Contain("href=\"/some-content-path\""));
        Assert.That(result, Does.Contain($"data-destination-id=\"{_contentKey:D}\""));
        Assert.That(result, Does.Contain($"data-destination-type=\"{_contentType}\""));
        Assert.That(result, Does.Contain("data-start-item-path=\"the-root-path\""));
        Assert.That(result, Does.Contain($"data-start-item-id=\"{_contentRootKey:D}\""));
        Assert.That(result, Does.Contain($"data-link-type=\"{LinkType.Content}\""));
    }

    [TestCase("#some-anchor")]
    [TestCase("?something=true")]
    [TestCase("#!some-hashbang")]
    [TestCase("?something=true#some-anchor")]
    public void ParseMarkup_CanParseContentLink_WithPostfix(string postfix)
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><a href=\"/{{localLink:{_contentKey:N}}}{postfix}\" type=\"document\"></a></p>");
        Assert.That(result, Does.Contain($"href=\"/some-content-path{postfix}\""));
        Assert.That(result, Does.Contain($"data-destination-id=\"{_contentKey:D}\""));
        Assert.That(result, Does.Contain($"data-destination-type=\"{_contentType}\""));
        Assert.That(result, Does.Contain("data-start-item-path=\"the-root-path\""));
        Assert.That(result, Does.Contain($"data-start-item-id=\"{_contentRootKey:D}\""));
        Assert.That(result, Does.Contain($"data-link-type=\"{LinkType.Content}\""));
    }

    [TestCase("#some-anchor")]
    [TestCase("?something=true")]
    [TestCase("#!some-hashbang")]
    [TestCase("?something=true#some-anchor")]
    public void ParseMarkup_CanParseLegacyContentLink_WithPostfix(string postfix)
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><a href=\"/{{localLink:umb://document/{_contentKey:N}}}{postfix}\"></a></p>");
        Assert.That(result, Does.Contain($"href=\"/some-content-path{postfix}\""));
        Assert.That(result, Does.Contain($"data-destination-id=\"{_contentKey:D}\""));
        Assert.That(result, Does.Contain($"data-destination-type=\"{_contentType}\""));
        Assert.That(result, Does.Contain("data-start-item-path=\"the-root-path\""));
        Assert.That(result, Does.Contain($"data-start-item-id=\"{_contentRootKey:D}\""));
        Assert.That(result, Does.Contain($"data-link-type=\"{LinkType.Content}\""));
    }

    [Test]
    public void ParseMarkup_CanParseMediaLink()
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><a href=\"/{{localLink:umb://media/{_mediaKey:N}}}\"></a></p>");
        Assert.That(result, Does.Contain("href=\"/some-media-url\""));
        Assert.That(result, Does.Contain($"data-destination-id=\"{_mediaKey:D}\""));
        Assert.That(result, Does.Contain($"data-destination-type=\"{_mediaType}\""));
        Assert.That(result, Does.Contain($"data-link-type=\"{LinkType.Media}\""));
    }

    [TestCase("{localLink:umb://document/fe5bf80d37db4373adb9b206896b4a3b}")]
    [TestCase("{localLink:umb://media/03b9a8721c4749a9a7026033ec78d860}")]
    public void ParseMarkup_InvalidLocalLinkYieldsEmptyLink(string href)
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><a href=\"/{href}\"></a></p>");
        Assert.That(result, Is.EqualTo($"<p><a></a></p>"));
    }

    [TestCase("<p><a href=\"https://some.where/else/\"></a></p>")]
    [TestCase("<p><a href=\"https://some.where/else/#some-anchor\"></a></p>")]
    [TestCase("<p><a href=\"https://some.where/else/?something=true\"></a></p>")]
    [TestCase("<p><img src=\"https://some.where/something.png?rmode=max&amp;width=500\"></p>")]
    public void ParseMarkup_CanHandleNonLocalReferences(string html)
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse(html);
        Assert.That(result, Is.EqualTo(html));
    }

    [Test]
    public void ParseMarkup_CanParseMediaImage()
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><img src=\"/media/whatever/something.png?rmode=max&amp;width=500\" data-udi=\"umb://media/{_mediaKey:N}\"></p>");
        Assert.That(result, Does.Contain("src=\"/some-media-url?rmode=max&amp;width=500\""));
        Assert.That(result, Does.Not.Contain("data-udi"));
        Assert.That(result, Does.Not.Contain("data-destination-id"));
        Assert.That(result, Does.Not.Contain("data-destination-type"));
        Assert.That(result, Does.Not.Contain("data-link-type"));
    }

    [Test]
    public void ParseMarkup_RemovesMediaDataCaption()
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><img src=\"/media/whatever/something.png?rmode=max&amp;width=500\" data-udi=\"umb://media/{_mediaKey:N}\"></p>");
        Assert.That(result, Does.Contain("src=\"/some-media-url?rmode=max&amp;width=500\""));
        Assert.That(result, Does.Not.Contain("data-udi"));
        Assert.That(result, Does.Not.Contain("data-destination-id"));
        Assert.That(result, Does.Not.Contain("data-destination-type"));
        Assert.That(result, Does.Not.Contain("data-link-type"));
    }

    [Test]
    public void ParseMarkup_DataAttributesAreRetained()
    {
        var parser = CreateRichTextMarkupParser();

        const string html = "<p><span data-something=\"the data-something value\">Text in a data-something SPAN</span></p>";
        var result = parser.Parse(html);
        Assert.That(result, Is.EqualTo(html));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ParseMarkup_CleansUpBlocks(bool inlineBlock)
    {
        var parser = CreateRichTextMarkupParser();
        var id = Guid.NewGuid();

        var tagName = $"umb-rte-block{(inlineBlock ? "-inline" : string.Empty)}";
        var result = parser.Parse($"<p><{tagName} data-content-key=\"{id:N}\"><!--Umbraco-Block--></{tagName}></p>");
        Assert.That(result, Is.EqualTo($"<p><{tagName} data-content-id=\"{id:D}\"></{tagName}></p>"));
    }

    [Test]
    public void ParseMarkup_CanHandleMixedInlineAndBlockLevelBlocks()
    {
        var parser = CreateRichTextMarkupParser();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        var result = parser.Parse($"<p><umb-rte-block-inline data-content-key=\"{id1:D}\"><!--Umbraco-Block--></umb-rte-block-inline></p><umb-rte-block data-content-key=\"{id2:D}\"><!--Umbraco-Block--></umb-rte-block>");
        Assert.That(result, Is.EqualTo($"<p><umb-rte-block-inline data-content-id=\"{id1:D}\"></umb-rte-block-inline></p><umb-rte-block data-content-id=\"{id2:D}\"></umb-rte-block>"));
    }

    private ApiRichTextElementParser CreateRichTextElementParser()
    {
        SetupTestContent(out var routeBuilder, out var cacheManager, out var urlProvider);

        return new ApiRichTextElementParser(
            routeBuilder,
            urlProvider,
            cacheManager.Content,
            cacheManager.Media,
            new ApiElementBuilder(CreateOutputExpansionStrategyAccessor()),
            Mock.Of<ILogger<ApiRichTextElementParser>>());
    }

    private ApiRichTextMarkupParser CreateRichTextMarkupParser()
    {
        SetupTestContent(out var routeBuilder, out var cacheManager, out var urlProvider);

        return new ApiRichTextMarkupParser(
            routeBuilder,
            urlProvider,
            cacheManager.Content,
            cacheManager.Media,
            Mock.Of<ILogger<ApiRichTextMarkupParser>>());
    }

    private void SetupTestContent(out IApiContentRouteBuilder routeBuilder, out ICacheManager cacheManager, out IApiMediaUrlProvider apiMediaUrlProvider)
    {
        var contentMock = new Mock<IPublishedContent>();
        contentMock.SetupGet(m => m.Key).Returns(_contentKey);
        contentMock.SetupGet(m => m.ItemType).Returns(PublishedItemType.Content);
        contentMock.SetupGet(m => m.ContentType.Alias).Returns(_contentType);

        var mediaMock = new Mock<IPublishedContent>();
        mediaMock.SetupGet(m => m.Key).Returns(_mediaKey);
        mediaMock.SetupGet(m => m.ItemType).Returns(PublishedItemType.Media);
        mediaMock.SetupGet(m => m.ContentType.Alias).Returns(_mediaType);

        var contentCacheMock = new Mock<IPublishedContentCache>();
        contentCacheMock.Setup(m => m.GetById(_contentKey)).Returns(contentMock.Object);
        var mediaCacheMock = new Mock<IPublishedMediaCache>();
        mediaCacheMock.Setup(m => m.GetById(_mediaKey)).Returns(mediaMock.Object);

        var cacheManagerMock = new Mock<ICacheManager>();
        cacheManagerMock.SetupGet(m => m.Content).Returns(contentCacheMock.Object);
        cacheManagerMock.SetupGet(m => m.Media).Returns(mediaCacheMock.Object);
        cacheManager = cacheManagerMock.Object;

        var routeBuilderMock = new Mock<IApiContentRouteBuilder>();
        routeBuilderMock
            .Setup(m => m.Build(contentMock.Object, null))
            .Returns(new ApiContentRoute("/some-content-path", new ApiContentStartItem(_contentRootKey, "the-root-path")));

        var apiMediaUrlProviderMock = new Mock<IApiMediaUrlProvider>();
        apiMediaUrlProviderMock
            .Setup(m => m.GetUrl(mediaMock.Object))
            .Returns("/some-media-url");

        routeBuilder = routeBuilderMock.Object;
        apiMediaUrlProvider = apiMediaUrlProviderMock.Object;
    }

    private IPublishedElement CreateElement(Guid id, int propertyValue)
    {
        var elementType = new Mock<IPublishedContentType>();
        elementType.SetupGet(c => c.Alias).Returns("theElementType");
        elementType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Element);

        var element = new Mock<IPublishedElement>();
        element.SetupGet(c => c.Key).Returns(id);
        element.SetupGet(c => c.ContentType).Returns(elementType.Object);

        var numberPropertyType = SetupPublishedPropertyType(new IntegerValueConverter(), "number", Constants.PropertyEditors.Aliases.Label);
        var propertyData = new PropertyData { Value = propertyValue, Culture = string.Empty, Segment = string.Empty };
        var property = new PublishedProperty(numberPropertyType, element.Object, CreateVariationContextAccessor(), CreatePropertyRenderingContextAccessor(), false, [propertyData], new ElementsDictionaryAppCache(), PropertyCacheLevel.None);

        element.SetupGet(c => c.Properties).Returns(new[] { property });
        return element.Object;
    }
}
