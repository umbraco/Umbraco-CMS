using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

internal sealed partial class HtmlIndexValueParser : IHtmlIndexValueParser
{
    private readonly ILogger<HtmlIndexValueParser> _logger;

    public HtmlIndexValueParser(ILogger<HtmlIndexValueParser> logger)
        => _logger = logger;

    public IndexValue? Parse(string html)
    {
        try
        {
            var doc = new HtmlDocument
            {
                OptionEnableBreakLineForInnerText = true
            };

            // HTML agility pack does not support indenting, which causes issues when extracting text from nested tags
            // like (un)ordered lists. as a workaround we'll enforce newlines for all tag endings, and explicitly trim
            // the resulting texts before utilizing them.
            doc.LoadHtml(html.Replace("</", "\n</"));

            // all nodes of textual relevance to indexing is at document root level
            HtmlNodeCollection children = doc.DocumentNode.ChildNodes;

            HtmlNode[] h1 = children.Where(c => c.Name.InvariantEquals("h1")).ToArray();
            HtmlNode[] h2 = children.Where(c => c.Name.InvariantEquals("h2")).ToArray();
            HtmlNode[] h3 = children.Where(c => c.Name.InvariantEquals("h3")).ToArray();
            HtmlNode[] texts = children.Except(h1.Union(h2).Union(h3)).ToArray();

            var indexValue = new IndexValue
            {
                TextsR1 = ParseNodes(h1).NullIfEmpty(),
                TextsR2 = ParseNodes(h2).NullIfEmpty(),
                TextsR3 = ParseNodes(h3).NullIfEmpty(),
                Texts = ParseNodes(texts).NullIfEmpty()
            };

            return indexValue.TextsR1 is not null
                || indexValue.TextsR2 is not null
                || indexValue.TextsR3 is not null
                || indexValue.Texts is not null
                ? indexValue
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not parse markdown HTML, see exception for details");
            return null;
        }
    }

    private string[] ParseNodes(HtmlNode[] nodes)
        => nodes
            .Select(node => MultipleWhiteSpaces().Replace(Newlines().Replace(node.InnerText, " "), " ").Trim())
            .Where(text => text.IsNullOrWhiteSpace() is false)
            .ToArray();

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex MultipleWhiteSpaces();

    [GeneratedRegex(@"[\r\n]")]
    private static partial Regex Newlines();
}
