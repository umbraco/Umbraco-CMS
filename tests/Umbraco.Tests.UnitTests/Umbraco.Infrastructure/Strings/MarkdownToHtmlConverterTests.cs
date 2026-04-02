using System.Text.RegularExpressions;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Strings;

[TestFixture]
public partial class MarkdownToHtmlConverterTests
{
    private const string MarkdownSample = """
        # Heading

        This is **bold** and *italic* text.

        - Item one
        - Item two

        ```
        var x = 1;
        ```
        """;

    [Test]
    public void MarkdigMarkdownToHtmlConverter_ToHtml_ConvertsMarkdownToHtml()
    {
        var converter = new MarkdigMarkdownToHtmlConverter();

        var result = converter.ToHtml(MarkdownSample);

        const string ExpectedHtml = """
            <h1>Heading</h1>
            <p>This is <strong>bold</strong> and <em>italic</em> text.</p>
            <ul>
            <li>Item one</li>
            <li>Item two</li>
            </ul>
            <pre><code>var x = 1;</code></pre>
        """;

    Assert.That(NormalizeHtml(result), Is.EqualTo(NormalizeHtml(ExpectedHtml)));
    }

#pragma warning disable CS0618 // Type or member is obsolete
    [Test]
    public void HeyRedMarkdownToHtmlConverter_ToHtml_ConvertsMarkdownToHtml()
    {
        var converter = new HeyRedMarkdownToHtmlConverter();

        var result = converter.ToHtml(MarkdownSample);

        const string ExpectedHtml = """
            <h1>Heading</h1>
            <p>This is <strong>bold</strong> and <em>italic</em> text.</p>
            <ul>
            <li>Item one</li>
            <li>Item two</li>
            </ul>
            <p><code>var x = 1;</code></p>
        """;

        Assert.That(NormalizeHtml(result), Is.EqualTo(NormalizeHtml(ExpectedHtml)));
    }
#pragma warning restore CS0618 // Type or member is obsolete

    private static string NormalizeHtml(string html) =>
        WhitespaceBeforeTagRegex().Replace(
            WhitespaceAfterTagRegex().Replace(html, ">"),
            "<").Trim();

    [GeneratedRegex(@">\s+")]
    private static partial Regex WhitespaceAfterTagRegex();

    [GeneratedRegex(@"\s+<")]
    private static partial Regex WhitespaceBeforeTagRegex();
}
