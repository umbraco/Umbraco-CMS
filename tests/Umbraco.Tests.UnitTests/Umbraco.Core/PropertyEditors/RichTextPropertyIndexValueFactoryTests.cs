using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Tests for <see cref="RichTextPropertyIndexValueFactory"/> to ensure it correctly creates index values from rich text properties.
/// </summary>
public class RichTextPropertyIndexValueFactoryTests
{
    /// <summary>
    /// Tests that the factory can create index values from a rich text property with valid content
    /// </summary>
    /// <param name="testContent"></param>
    /// <param name="expected"></param>
    [TestCase("<p>Sample text</p>", "Sample text")]
    [TestCase("<p>John Smith<br>Company ABC<br>London</p>", "John Smith Company ABC London")]
    [TestCase("<p>John Smith<break>Company ABC<break>London</p>", "John SmithCompany ABCLondon")]
    [TestCase("<p>John Smith<br>Company ABC<branything>London</p>", "John Smith Company ABCLondon")]
    [TestCase("<p>Another sample text with <strong>bold</strong> content</p>", "Another sample text with bold content")]
    [TestCase("<p>Text with <a href=\"https://example.com\">link</a></p>", "Text with link")]
    [TestCase("<p>Text with <img src=\"image.jpg\" alt=\"image\" /></p>", "Text with")]
    [TestCase("<p>Text with <span style=\"color: red;\">styled text</span></p>", "Text with styled text")]
    [TestCase("<p>Text with <em>emphasized</em> content</p>", "Text with emphasized content")]
    [TestCase("<p>Text with <u>underlined</u> content</p>", "Text with underlined content")]
    [TestCase("<p>Text with <code>inline code</code></p>", "Text with inline code")]
    [TestCase("<p>Text with <pre><code>code block</code></pre></p>", "Text with code block")]
    [TestCase("<p>Text with <blockquote>quoted text</blockquote></p>", "Text with quoted text")]
    [TestCase(
        "<p>Text with <ul><li>list item 1</li><li>list item 2</li></ul></p>",
        "Text with list item 1list item 2")]
    [TestCase(
        "<p>Text with <ol><li>ordered item 1</li><li>ordered item 2</li></ol></p>",
        "Text with ordered item 1ordered item 2")]
    [TestCase("<p>Text with <div class=\"class-name\">div content</div></p>", "Text with div content")]
    [TestCase("<p>Text with <span class=\"class-name\">span content</span></p>", "Text with span content")]
    [TestCase(
        "<p>Text with <strong>bold</strong> and <em>italic</em> content</p>",
        "Text with bold and italic content")]
    [TestCase(
        "<p>Text with <a href=\"https://example.com\" target=\"_blank\">external link</a></p>",
        "Text with external link")]
    [TestCase("<p>John Smith<br class=\"test\">Company ABC<br>London</p>", "John Smith Company ABC London")]
    [TestCase("<p>John Smith<br \r\n />Company ABC<br>London</p>", "John Smith Company ABC London")]
    public void Can_Create_Index_Values_From_RichText_Property(string testContent, string expected)
    {
        var propertyEditorCollection = new PropertyEditorCollection(new DataEditorCollection(() => null));
        var jsonSerializer = Mock.Of<IJsonSerializer>();
        var indexingSettings = Mock.Of<IOptionsMonitor<IndexingSettings>>();
        Mock.Get(indexingSettings).Setup(x => x.CurrentValue).Returns(new IndexingSettings { });
        var logger = Mock.Of<ILogger<RichTextPropertyIndexValueFactory>>();
        string alias = "richText";

        var factory = new RichTextPropertyIndexValueFactory(
            propertyEditorCollection,
            jsonSerializer,
            indexingSettings,
            logger);

        // create a mock property with the rich text value
        var property = Mock.Of<IProperty>(p => p.Alias == alias
                                               && (string)p.GetValue(
                                                   It.IsAny<string>(),
                                                   It.IsAny<string>(),
                                                   It.IsAny<bool>()) == testContent);

        // get the index value for the property
        var indexValue = factory
            .GetIndexValues(property, null, null, true, [], new Dictionary<Guid, IContentType>())
            .FirstOrDefault(kvp => kvp.FieldName == alias);
        Assert.IsNotNull(indexValue);

        // assert that index the value is created correctly (it might contain a trailing whitespace, but that's OK)
        var expectedIndexValue = indexValue.Values.SingleOrDefault() as string;
        Assert.IsNotNull(expectedIndexValue);
        Assert.AreEqual(expected, expectedIndexValue.TrimEnd());
    }
}
