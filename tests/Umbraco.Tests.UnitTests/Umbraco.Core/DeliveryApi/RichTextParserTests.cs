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
        Assert.IsNotNull(element);
        Assert.AreEqual("#root", element.Tag);
    }

    [Test]
    public void ParseElement_SimpleParagraphHasSingleTextElement()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse("<p>Some text paragraph</p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(1, element.Elements.Count());
        var paragraph = element.Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual("p", paragraph.Tag);
        var textElement = paragraph.Elements.First() as RichTextTextElement;
        Assert.IsNotNull(textElement);
        Assert.AreEqual("Some text paragraph", textElement.Text);
    }

    [Test]
    public void ParseElement_ParagraphWithLineBreaksWrapsTextInElements()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse("<p>Some text<br/>More text<br/>Even more text</p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(1, element.Elements.Count());
        var paragraph = element.Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual("p", paragraph.Tag);
        var paragraphElements = paragraph.Elements.ToArray();
        Assert.AreEqual(5, paragraphElements.Length);
        for (var i = 0; i < paragraphElements.Length; i++)
        {
            var paragraphElement = paragraphElements[i] as RichTextGenericElement;
            var textElement = paragraphElements[i] as RichTextTextElement;
            switch (i)
            {
                case 0:
                    Assert.IsNull(paragraphElement);
                    Assert.IsNotNull(textElement);
                    Assert.AreEqual("#text", textElement.Tag);
                    Assert.AreEqual("Some text", textElement.Text);
                    break;
                case 2:
                    Assert.IsNull(paragraphElement);
                    Assert.IsNotNull(textElement);
                    Assert.AreEqual("#text", textElement.Tag);
                    Assert.AreEqual("More text", textElement.Text);
                    break;
                case 4:
                    Assert.IsNull(paragraphElement);
                    Assert.IsNotNull(textElement);
                    Assert.AreEqual("#text", textElement.Tag);
                    Assert.AreEqual("Even more text", textElement.Text);
                    break;
                case 1:
                case 3:
                    Assert.IsNull(textElement);
                    Assert.IsNotNull(paragraphElement);
                    Assert.IsEmpty(paragraphElement.Elements);
                    Assert.AreEqual("br", paragraphElement.Tag);
                    break;
            }
        }
    }

    [Test]
    public void ParseElement_DataAttributesAreSanitized()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse("<p><span data-something=\"the data-something value\">Text in a data-something SPAN</span></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.IsNotNull(element);
        var span = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.Tag);
        Assert.AreEqual(1, span.Attributes.Count);
        Assert.AreEqual("something", span.Attributes.First().Key);
        Assert.AreEqual("the data-something value", span.Attributes.First().Value);
        var textElement = span.Elements.Single() as RichTextTextElement;
        Assert.IsNotNull(textElement);
        Assert.AreEqual("Text in a data-something SPAN", textElement.Text);
    }

    [Test]
    public void ParseElement_DataAttributesDoNotOverwriteExistingAttributes()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse("<p><span something=\"the original something\" data-something=\"the data something\">Text in a data-something SPAN</span></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.IsNotNull(element);
        var span = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(span);
        Assert.AreEqual("span", span.Tag);
        Assert.AreEqual(1, span.Attributes.Count);
        Assert.AreEqual("something", span.Attributes.First().Key);
        Assert.AreEqual("the original something", span.Attributes.First().Value);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("#some-anchor")]
    [TestCase("?something=true")]
    public void ParseElement_CanParseContentLink(string? postfix)
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"/{{localLink:umb://document/{_contentKey:N}}}{postfix}\"></a></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.IsNotNull(element);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(link);
        Assert.AreEqual("a", link.Tag);
        Assert.AreEqual(4, link.Attributes.Count);
        Assert.IsNotNull(link.Attributes["route"]);
        var route = link.Attributes["route"] as IApiContentRoute;
        Assert.IsNotNull(route);
        Assert.AreEqual("/some-content-path", route.Path);
        Assert.AreEqual(postfix.NullOrWhiteSpaceAsNull(), route.QueryString);
        Assert.AreEqual(_contentRootKey, route.StartItem.Id);
        Assert.AreEqual("the-root-path", route.StartItem.Path);

        Assert.IsNotNull(link.Attributes["destinationId"]);
        Assert.IsNotNull(link.Attributes["destinationType"]);
        Assert.IsNotNull(link.Attributes["linkType"]);
        Assert.AreEqual(_contentKey, Guid.Parse((link.Attributes["destinationId"] as string)!));
        Assert.AreEqual(_contentType, link.Attributes["destinationType"]);
        Assert.AreEqual(nameof(LinkType.Content), link.Attributes["linkType"]);
    }

    [Test]
    public void ParseElement_CanParseMediaLink()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"/{{localLink:umb://media/{_mediaKey:N}}}\"></a></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.IsNotNull(element);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(link);
        Assert.AreEqual("a", link.Tag);
        Assert.AreEqual(4, link.Attributes.Count);
        Assert.AreEqual("href", link.Attributes.First().Key);
        Assert.AreEqual("/some-media-url", link.Attributes.First().Value);

        Assert.IsNotNull(link.Attributes["destinationId"]);
        Assert.IsNotNull(link.Attributes["destinationType"]);
        Assert.IsNotNull(link.Attributes["linkType"]);
        Assert.AreEqual(_mediaKey, Guid.Parse((link.Attributes["destinationId"] as string)!));
        Assert.AreEqual(_mediaType, link.Attributes["destinationType"]);
        Assert.AreEqual(nameof(LinkType.Media), link.Attributes["linkType"]);
    }

    [Test]
    public void ParseElement_CanHandleNonLocalLink()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"https://some.where/else/\"></a></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.IsNotNull(element);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(link);
        Assert.AreEqual("a", link.Tag);
        Assert.AreEqual(1, link.Attributes.Count);
        Assert.AreEqual("href", link.Attributes.First().Key);
        Assert.AreEqual("https://some.where/else/", link.Attributes.First().Value);

        Assert.IsFalse(link.Attributes.TryGetValue("destinationId", out _));
        Assert.IsFalse(link.Attributes.TryGetValue("destinationType", out _));
        Assert.IsFalse(link.Attributes.TryGetValue("linkType", out _));
    }

    [TestCase("#some-anchor")]
    [TestCase("?something=true")]
    public void ParseElement_CanHandleNonLocalLink_WithPostfix(string postfix)
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"https://some.where/else/{postfix}\"></a></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.IsNotNull(element);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(link);
        Assert.AreEqual("a", link.Tag);
        Assert.AreEqual(1, link.Attributes.Count);
        Assert.AreEqual("href", link.Attributes.First().Key);
        Assert.AreEqual($"https://some.where/else/{postfix}", link.Attributes.First().Value);

        Assert.IsFalse(link.Attributes.TryGetValue("destinationId", out _));
        Assert.IsFalse(link.Attributes.TryGetValue("destinationType", out _));
        Assert.IsFalse(link.Attributes.TryGetValue("linkType", out _));
    }

    [Test]
    public void ParseElement_LinkTextIsWrappedInTextElement()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"https://some.where/else/\">This is the link text</a></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.IsNotNull(element);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(link);
        Assert.AreEqual("a", link.Tag);
        var textElement = link.Elements.Single() as RichTextTextElement;
        Assert.IsNotNull(textElement);
        Assert.AreEqual("This is the link text", textElement.Text);

        Assert.IsFalse(link.Attributes.TryGetValue("destinationId", out _));
        Assert.IsFalse(link.Attributes.TryGetValue("destinationType", out _));
        Assert.IsFalse(link.Attributes.TryGetValue("linkType", out _));
    }

    [TestCase("{localLink:umb://document/fe5bf80d37db4373adb9b206896b4a3b}")]
    [TestCase("{localLink:umb://media/03b9a8721c4749a9a7026033ec78d860}")]
    public void ParseElement_InvalidLocalLinkYieldsEmptyLink(string href)
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><a href=\"/{href}\"></a></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.IsNotNull(element);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(link);
        Assert.AreEqual("a", link.Tag);
        Assert.IsEmpty(link.Attributes);
    }

    [Test]
    public void ParseElement_CanParseMediaImage()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><img src=\"/media/whatever/something.png?rmode=max&amp;width=500\" data-udi=\"umb://media/{_mediaKey:N}\"></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.IsNotNull(element);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(link);
        Assert.AreEqual("img", link.Tag);
        Assert.AreEqual(1, link.Attributes.Count);
        Assert.AreEqual("src", link.Attributes.First().Key);
        Assert.AreEqual("/some-media-url", link.Attributes.First().Value);

        Assert.IsFalse(link.Attributes.TryGetValue("destinationId", out _));
        Assert.IsFalse(link.Attributes.TryGetValue("destinationType", out _));
        Assert.IsFalse(link.Attributes.TryGetValue("linkType", out _));
    }

    [Test]
    public void ParseElement_CanHandleNonLocalImage()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p><img src=\"https://some.where/something.png?rmode=max&amp;width=500\"></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.IsNotNull(element);
        var link = element.Elements.OfType<RichTextGenericElement>().Single().Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(link);
        Assert.AreEqual("img", link.Tag);
        Assert.AreEqual(1, link.Attributes.Count);
        Assert.AreEqual("src", link.Attributes.First().Key);
        Assert.AreEqual("https://some.where/something.png?rmode=max&amp;width=500", link.Attributes.First().Value);

        Assert.IsFalse(link.Attributes.TryGetValue("destinationId", out _));
        Assert.IsFalse(link.Attributes.TryGetValue("destinationType", out _));
        Assert.IsFalse(link.Attributes.TryGetValue("linkType", out _));
    }

    [Test]
    public void ParseElement_RemovesComments()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse("<p>some text<!-- a comment -->some more text</p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.IsNotNull(element);
        var paragraph = element.Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual(2, paragraph.Elements.Count());
        var textElements = paragraph.Elements.OfType<RichTextTextElement>().ToArray();
        Assert.AreEqual(2, textElements.Length);
        Assert.AreEqual("some text", textElements.First().Text);
        Assert.AreEqual("some more text", textElements.Last().Text);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ParseElement_CleansUpBlocks(bool inlineBlock)
    {
        var parser = CreateRichTextElementParser();
        var id = Guid.NewGuid();

        var tagName = $"umb-rte-block{(inlineBlock ? "-inline" : string.Empty)}";
        var element = parser.Parse($"<p><{tagName} data-content-key=\"{id:N}\"><!--Umbraco-Block--></{tagName}></p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.IsNotNull(element);
        var paragraph = element.Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(paragraph);
        var block = paragraph.Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(block);
        Assert.AreEqual(tagName, block.Tag);
        Assert.AreEqual(1, block.Attributes.Count);
        Assert.IsTrue(block.Attributes.ContainsKey("content-id"));
        Assert.AreEqual(id, block.Attributes["content-id"]);
        Assert.IsEmpty(block.Elements);
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
                    Udi.Create(Constants.UdiEntityType.Element, block1ContentId),
                    CreateElement(block1ContentId, 123),
                    null!,
                    null!),
                new (
                    Udi.Create(Constants.UdiEntityType.Element, block2ContentId),
                    CreateElement(block2ContentId, 456),
                    Udi.Create(Constants.UdiEntityType.Element, block2SettingsId),
                    CreateElement(block2SettingsId, 789))
            });

        var tagName = $"umb-rte-block{(inlineBlock ? "-inline" : string.Empty)}";
        var element = parser.Parse($"<p><{tagName} data-content-key=\"{block1ContentId:N}\"><!--Umbraco-Block--></{tagName}><{tagName} data-content-key=\"{block2ContentId:N}\"><!--Umbraco-Block--></{tagName}></p>", richTextBlockModel) as RichTextRootElement;
        Assert.IsNotNull(element);
        var paragraph = element.Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual(2, paragraph.Elements.Count());

        var block1Element = paragraph.Elements.First() as RichTextGenericElement;
        Assert.IsNotNull(block1Element);
        Assert.AreEqual(tagName, block1Element.Tag);
        Assert.AreEqual(block1ContentId, block1Element.Attributes["content-id"]);

        var block2Element = paragraph.Elements.Last() as RichTextGenericElement;
        Assert.IsNotNull(block2Element);
        Assert.AreEqual(tagName, block2Element.Tag);
        Assert.AreEqual(block2ContentId, block2Element.Attributes["content-id"]);

        Assert.AreEqual(2, element.Blocks.Count());

        var block1 = element.Blocks.First();
        Assert.AreEqual(block1ContentId, block1.Content.Id);
        Assert.AreEqual(123, block1.Content.Properties["number"]);
        Assert.IsNull(block1.Settings);

        var block2 = element.Blocks.Last();
        Assert.AreEqual(block2ContentId, block2.Content.Id);
        Assert.AreEqual(456, block2.Content.Properties["number"]);
        Assert.AreEqual(block2SettingsId, block2.Settings!.Id);
        Assert.AreEqual(789, block2.Settings.Properties["number"]);
    }

    [Test]
    public void ParseElement_CanHandleMixedInlineAndBlockLevelBlocks()
    {
        var parser = CreateRichTextElementParser();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        var element = parser.Parse($"<p><umb-rte-block-inline data-content-key=\"{id1:N}\"><!--Umbraco-Block--></umb-rte-block-inline></p><umb-rte-block data-content-key=\"{id2:N}\"><!--Umbraco-Block--></umb-rte-block>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.IsNotNull(element);
        Assert.AreEqual(2, element.Elements.Count());

        var paragraph = element.Elements.First() as RichTextGenericElement;
        Assert.IsNotNull(paragraph);

        var inlineBlock = paragraph.Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(inlineBlock);
        Assert.AreEqual("umb-rte-block-inline", inlineBlock.Tag);
        Assert.AreEqual(1, inlineBlock.Attributes.Count);
        Assert.IsTrue(inlineBlock.Attributes.ContainsKey("content-id"));
        Assert.AreEqual(id1, inlineBlock.Attributes["content-id"]);
        Assert.IsEmpty(inlineBlock.Elements);

        var blockLevelBlock = element.Elements.Last() as RichTextGenericElement;
        Assert.IsNotNull(blockLevelBlock);
        Assert.AreEqual("umb-rte-block", blockLevelBlock.Tag);
        Assert.AreEqual(1, blockLevelBlock.Attributes.Count);
        Assert.IsTrue(blockLevelBlock.Attributes.ContainsKey("content-id"));
        Assert.AreEqual(id2, blockLevelBlock.Attributes["content-id"]);
        Assert.IsEmpty(blockLevelBlock.Elements);
    }

    private const string TestParagraph = "What follows from <strong>here</strong> <em>is</em> <a href=\"#\">just</a> a bunch of text.";

    [Test]
    public void ParseElement_CanHandleWhitespaceAroundInlineElemements()
    {
        var parser = CreateRichTextElementParser();

        var element = parser.Parse($"<p>{TestParagraph}</p>", RichTextBlockModel.Empty) as RichTextRootElement;
        Assert.IsNotNull(element);
        var paragraphElement = element.Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(paragraphElement);

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
        Assert.IsNotNull(element);
        var tableElement = element.Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(tableElement);

        var rowElement = tableElement.Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(rowElement);

        var cellElement = rowElement.Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(cellElement);

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
        Assert.IsNotNull(element);
        var divElement = element.Elements.Single() as RichTextGenericElement;
        Assert.IsNotNull(divElement);

        var paragraphELements = divElement.Elements;
        Assert.AreEqual(4, paragraphELements.Count());

        AssertTestParagraph(paragraphELements.First() as RichTextGenericElement);
        AssertTestParagraph(paragraphELements.Last() as RichTextGenericElement);
    }

    private static void AssertTestParagraph(RichTextGenericElement paragraphElement)
    {
        var childElements = paragraphElement.Elements.ToArray();
        Assert.AreEqual(7, childElements.Length);

        var childElementCounter = 0;

        void AssertNextChildElementIsText(string expectedText)
        {
            var textElement = childElements[childElementCounter++] as RichTextTextElement;
            Assert.IsNotNull(textElement);
            Assert.AreEqual(expectedText, textElement.Text);
        }

        void AssertNextChildElementIsGeneric(string expectedTag, string expectedInnerText)
        {
            var genericElement = childElements[childElementCounter++] as RichTextGenericElement;
            Assert.IsNotNull(genericElement);
            Assert.AreEqual(expectedTag, genericElement.Tag);
            Assert.AreEqual(1, genericElement.Elements.Count());
            var textElement = genericElement.Elements.First() as RichTextTextElement;
            Assert.IsNotNull(textElement);
            Assert.AreEqual(expectedInnerText, textElement.Text);
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
        Assert.IsTrue(result.Contains("href=\"/some-content-path\""));
        Assert.IsTrue(result.Contains($"data-destination-id=\"{_contentKey:D}\""));
        Assert.IsTrue(result.Contains($"data-destination-type=\"{_contentType}\""));
        Assert.IsTrue(result.Contains("data-start-item-path=\"the-root-path\""));
        Assert.IsTrue(result.Contains($"data-start-item-id=\"{_contentRootKey:D}\""));
        Assert.IsTrue(result.Contains($"data-link-type=\"{LinkType.Content}\""));
    }

    [Test]
    public void ParseMarkup_CanParseLegacyContentLink()
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><a href=\"/{{localLink:umb://document/{_contentKey:N}}}\"></a></p>");
        Assert.IsTrue(result.Contains("href=\"/some-content-path\""));
        Assert.IsTrue(result.Contains($"data-destination-id=\"{_contentKey:D}\""));
        Assert.IsTrue(result.Contains($"data-destination-type=\"{_contentType}\""));
        Assert.IsTrue(result.Contains("data-start-item-path=\"the-root-path\""));
        Assert.IsTrue(result.Contains($"data-start-item-id=\"{_contentRootKey:D}\""));
        Assert.IsTrue(result.Contains($"data-link-type=\"{LinkType.Content}\""));
    }

    [TestCase("#some-anchor")]
    [TestCase("?something=true")]
    [TestCase("#!some-hashbang")]
    [TestCase("?something=true#some-anchor")]
    public void ParseMarkup_CanParseContentLink_WithPostfix(string postfix)
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><a href=\"/{{localLink:{_contentKey:N}}}{postfix}\" type=\"document\"></a></p>");
        Assert.IsTrue(result.Contains($"href=\"/some-content-path{postfix}\""));
        Assert.IsTrue(result.Contains($"data-destination-id=\"{_contentKey:D}\""));
        Assert.IsTrue(result.Contains($"data-destination-type=\"{_contentType}\""));
        Assert.IsTrue(result.Contains("data-start-item-path=\"the-root-path\""));
        Assert.IsTrue(result.Contains($"data-start-item-id=\"{_contentRootKey:D}\""));
        Assert.IsTrue(result.Contains($"data-link-type=\"{LinkType.Content}\""));
    }

    [TestCase("#some-anchor")]
    [TestCase("?something=true")]
    [TestCase("#!some-hashbang")]
    [TestCase("?something=true#some-anchor")]
    public void ParseMarkup_CanParseLegacyContentLink_WithPostfix(string postfix)
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><a href=\"/{{localLink:umb://document/{_contentKey:N}}}{postfix}\"></a></p>");
        Assert.IsTrue(result.Contains($"href=\"/some-content-path{postfix}\""));
        Assert.IsTrue(result.Contains($"data-destination-id=\"{_contentKey:D}\""));
        Assert.IsTrue(result.Contains($"data-destination-type=\"{_contentType}\""));
        Assert.IsTrue(result.Contains("data-start-item-path=\"the-root-path\""));
        Assert.IsTrue(result.Contains($"data-start-item-id=\"{_contentRootKey:D}\""));
        Assert.IsTrue(result.Contains($"data-link-type=\"{LinkType.Content}\""));
    }

    [Test]
    public void ParseMarkup_CanParseMediaLink()
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><a href=\"/{{localLink:umb://media/{_mediaKey:N}}}\"></a></p>");
        Assert.IsTrue(result.Contains("href=\"/some-media-url\""));
        Assert.IsTrue(result.Contains($"data-destination-id=\"{_mediaKey:D}\""));
        Assert.IsTrue(result.Contains($"data-destination-type=\"{_mediaType}\""));
        Assert.IsTrue(result.Contains($"data-link-type=\"{LinkType.Media}\""));
    }

    [TestCase("{localLink:umb://document/fe5bf80d37db4373adb9b206896b4a3b}")]
    [TestCase("{localLink:umb://media/03b9a8721c4749a9a7026033ec78d860}")]
    public void ParseMarkup_InvalidLocalLinkYieldsEmptyLink(string href)
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><a href=\"/{href}\"></a></p>");
        Assert.AreEqual($"<p><a></a></p>", result);
    }

    [TestCase("<p><a href=\"https://some.where/else/\"></a></p>")]
    [TestCase("<p><a href=\"https://some.where/else/#some-anchor\"></a></p>")]
    [TestCase("<p><a href=\"https://some.where/else/?something=true\"></a></p>")]
    [TestCase("<p><img src=\"https://some.where/something.png?rmode=max&amp;width=500\"></p>")]
    public void ParseMarkup_CanHandleNonLocalReferences(string html)
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse(html);
        Assert.AreEqual(html, result);
    }

    [Test]
    public void ParseMarkup_CanParseMediaImage()
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><img src=\"/media/whatever/something.png?rmode=max&amp;width=500\" data-udi=\"umb://media/{_mediaKey:N}\"></p>");
        Assert.IsTrue(result.Contains("src=\"/some-media-url?rmode=max&amp;width=500\""));
        Assert.IsFalse(result.Contains("data-udi"));
        Assert.IsFalse(result.Contains("data-destination-id"));
        Assert.IsFalse(result.Contains("data-destination-type"));
        Assert.IsFalse(result.Contains("data-link-type"));
    }

    [Test]
    public void ParseMarkup_RemovesMediaDataCaption()
    {
        var parser = CreateRichTextMarkupParser();

        var result = parser.Parse($"<p><img src=\"/media/whatever/something.png?rmode=max&amp;width=500\" data-udi=\"umb://media/{_mediaKey:N}\"></p>");
        Assert.IsTrue(result.Contains("src=\"/some-media-url?rmode=max&amp;width=500\""));
        Assert.IsFalse(result.Contains("data-udi"));
        Assert.IsFalse(result.Contains("data-destination-id"));
        Assert.IsFalse(result.Contains("data-destination-type"));
        Assert.IsFalse(result.Contains("data-link-type"));
    }

    [Test]
    public void ParseMarkup_DataAttributesAreRetained()
    {
        var parser = CreateRichTextMarkupParser();

        const string html = "<p><span data-something=\"the data-something value\">Text in a data-something SPAN</span></p>";
        var result = parser.Parse(html);
        Assert.AreEqual(html, result);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ParseMarkup_CleansUpBlocks(bool inlineBlock)
    {
        var parser = CreateRichTextMarkupParser();
        var id = Guid.NewGuid();

        var tagName = $"umb-rte-block{(inlineBlock ? "-inline" : string.Empty)}";
        var result = parser.Parse($"<p><{tagName} data-content-key=\"{id:N}\"><!--Umbraco-Block--></{tagName}></p>");
        Assert.AreEqual($"<p><{tagName} data-content-id=\"{id:D}\"></{tagName}></p>", result);
    }

    [Test]
    public void ParseMarkup_CanHandleMixedInlineAndBlockLevelBlocks()
    {
        var parser = CreateRichTextMarkupParser();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        var result = parser.Parse($"<p><umb-rte-block-inline data-content-key=\"{id1:D}\"><!--Umbraco-Block--></umb-rte-block-inline></p><umb-rte-block data-content-key=\"{id2:D}\"><!--Umbraco-Block--></umb-rte-block>");
        Assert.AreEqual($"<p><umb-rte-block-inline data-content-id=\"{id1:D}\"></umb-rte-block-inline></p><umb-rte-block data-content-id=\"{id2:D}\"></umb-rte-block>", result);
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
        var property = new PublishedElementPropertyBase(numberPropertyType, element.Object, false, PropertyCacheLevel.None, VariationContext, CacheManager, propertyValue);

        element.SetupGet(c => c.Properties).Returns(new[] { property });
        return element.Object;
    }
}
