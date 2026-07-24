using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Html;

namespace Umbraco.Cms.Web.Common.Mvc;

/// <summary>
///     Provides utility methods for UmbracoHelper for working with strings and HTML in views.
/// </summary>
public sealed partial class HtmlStringUtilities
{
    /// <summary>
    ///     HTML encodes the text and replaces text line breaks with HTML line breaks.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>
    ///     The HTML encoded text with text line breaks replaced with HTML line breaks (<c>&lt;br /&gt;</c>).
    /// </returns>
    public IHtmlContent ReplaceLineBreaks(string text)
    {
        var value = WebUtility.HtmlEncode(text)
            .Replace("\r\n", "<br />")
            .Replace("\r", "<br />")
            .Replace("\n", "<br />");

        return new HtmlString(value);
    }

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex MultiSpaceRegex();

    [GeneratedRegex(@"\s([\.,;:!?)\]}])")]
    private static partial Regex PunctuationRegex();

    /// <summary>
    ///    Strips HTML tags from the given HTML string, optionally filtering by specific tags.
    /// </summary>
    /// <param name="html">The HTML string to process.</param>
    /// <param name="tags">The tags to filter by. If null or empty, all tags will be stripped.</param>
    /// <returns>The processed HTML string with specified tags removed.</returns>
    public HtmlString StripHtmlTags(string html, params string[]? tags)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var targets = new List<HtmlNode>();
        HtmlNodeCollection? nodes = doc.DocumentNode.SelectNodes(".//*");

        if (nodes is not null)
        {
            bool filterAllTags = tags == null || !tags.Any();
            foreach (HtmlNode node in nodes)
            {
                // is element
                if (node.NodeType != HtmlNodeType.Element)
                {
                    continue;
                }

                if (filterAllTags ||
                    (tags?.Any(tag => string.Equals(tag, node.Name, StringComparison.CurrentCultureIgnoreCase)) ?? false))
                {
                    targets.Add(node);
                }
            }

            // we have to reverse the list in order to not override the changes to the nodes later
            targets.Reverse();
            foreach (HtmlNode target in CollectionsMarshal.AsSpan(targets))
            {
                HtmlNode content = doc.CreateTextNode(target.InnerHtml + " ");
                target.ParentNode.ReplaceChild(content, target);
            }
        }
        else
        {
            return new HtmlString(html);
        }

        var text = MultiSpaceRegex().Replace(doc.DocumentNode.InnerHtml, " ").Trim();
        text = PunctuationRegex().Replace(text, "$1");
        return new HtmlString(text);
    }

    /// <summary>
    ///     Joins the string representations of the specified objects using the specified separator, ignoring null or whitespace values.
    /// </summary>
    /// <param name="separator">The string to use as a separator.</param>
    /// <param name="args">The objects to join.</param>
    /// <returns>A string that consists of the elements in <paramref name="args"/> delimited by the <paramref name="separator"/> string.</returns>
    public string Join(string separator, params object[] args)
    {
        IEnumerable<string?> results = args
            .Select(x => x.ToString())
            .Where(x => string.IsNullOrWhiteSpace(x) == false);
        return string.Join(separator, results);
    }

    /// <summary>
    ///     Concatenates the string representations of the specified objects, ignoring null or whitespace values.
    /// </summary>
    /// <param name="args">The objects to concatenate.</param>
    /// <returns>A string that consists of the concatenated elements in <paramref name="args"/>.</returns>
    public string Concatenate(params object[] args)
    {
        var sb = new StringBuilder();
        foreach (var arg in args
                     .Select(x => x.ToString())
                     .Where(x => string.IsNullOrWhiteSpace(x) == false))
        {
            sb.Append(arg);
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Returns the first non-null and non-whitespace string representation of the specified objects.
    /// </summary>
    /// <param name="args">The objects to evaluate.</param>
    /// <returns>The first non-null and non-whitespace string representation, or an empty string if none found.</returns>
    public string Coalesce(params object?[] args)
    {
        var arg = args
            .Select(x => x?.ToString())
            .FirstOrDefault(x => string.IsNullOrWhiteSpace(x) == false);

        return arg ?? string.Empty;
    }

    /// <summary>
    ///    Truncates the given HTML string to the specified length, optionally adding an ellipsis and treating tags as content.
    /// </summary>
    /// <param name="html">The HTML string to truncate.</param>
    /// <param name="length">The maximum length of the truncated string.</param>
    /// <param name="addElipsis">Whether to add an ellipsis ("&hellip;") at the end of the truncated string.</param>
    /// <param name="treatTagsAsContent">Whether to treat HTML tags as content when calculating the length.</param>
    /// <returns>The truncated HTML string as an <see cref="IHtmlContent"/>.</returns>
    public IHtmlContent Truncate(string html, int length, bool addElipsis, bool treatTagsAsContent)
    {
        const string hellip = "&hellip;";

        using var outputms = new MemoryStream();
        var lengthReached = false;

        using var outputtw = new StreamWriter(outputms);
        using (var ms = new MemoryStream())
        {
            using (var tw = new StreamWriter(ms))
            {
                tw.Write(html);
                tw.Flush();
                ms.Position = 0;
                var tagStack = new Stack<string>();

                using (TextReader tr = new StreamReader(ms))
                {
                    bool isInsideElement = false,
                        insideTagSpaceEncountered = false,
                        isTagClose = false;

                    int ic,

                        // currentLength = 0,
                        currentTextLength = 0;

                    string currentTag = string.Empty,
                        tagContents = string.Empty;

                    while ((ic = tr.Read()) != -1)
                    {
                        var write = true;

                        switch ((char)ic)
                        {
                            case '<':
                                if (!lengthReached)
                                {
                                    isInsideElement = true;
                                }

                                insideTagSpaceEncountered = false;
                                currentTag = string.Empty;
                                tagContents = string.Empty;
                                isTagClose = false;
                                if (tr.Peek() == '/')
                                {
                                    isTagClose = true;
                                }

                                break;

                            case '>':
                                isInsideElement = false;

                                if (isTagClose && tagStack.Count > 0)
                                {
                                    var thisTag = tagStack.Pop();
                                    outputtw.Write("</");
                                    outputtw.Write(thisTag);
                                    outputtw.Write('>');
                                    if (treatTagsAsContent)
                                    {
                                        currentTextLength++;
                                    }
                                }

                                if (!isTagClose && currentTag.Length > 0)
                                {
                                    if (!lengthReached)
                                    {
                                        tagStack.Push(currentTag);
                                        outputtw.Write('<');
                                        outputtw.Write(currentTag);
                                        if (treatTagsAsContent)
                                        {
                                            currentTextLength++;
                                        }

                                        if (!string.IsNullOrEmpty(tagContents))
                                        {
                                            if (tagContents.EndsWith('/'))
                                            {
                                                // No end tag e.g. <br />.
                                                tagStack.Pop();
                                            }

                                            outputtw.Write(tagContents);
                                            insideTagSpaceEncountered = false;
                                        }

                                        outputtw.Write(">");
                                    }
                                }

                                // Continue to next iteration of the text reader.
                                continue;

                            default:
                                if (isInsideElement)
                                {
                                    if (ic == ' ')
                                    {
                                        if (!insideTagSpaceEncountered)
                                        {
                                            insideTagSpaceEncountered = true;
                                        }
                                    }

                                    if (!insideTagSpaceEncountered)
                                    {
                                        currentTag += (char)ic;
                                    }
                                }

                                break;
                        }

                        if (isInsideElement || insideTagSpaceEncountered)
                        {
                            write = false;
                            if (insideTagSpaceEncountered)
                            {
                                tagContents += (char)ic;
                            }
                        }

                        if (!isInsideElement || treatTagsAsContent)
                        {
                            currentTextLength++;
                        }

                        if (currentTextLength <= length || (lengthReached && isInsideElement))
                        {
                            if (write)
                            {
                                var charToWrite = (char)ic;
                                outputtw.Write(charToWrite);

                                // currentLength++;
                            }
                        }

                        if (!lengthReached)
                        {
                            if (currentTextLength == length)
                            {
                                // if the last character added was the first of a two character unicode pair, add the second character
                                if (char.IsHighSurrogate((char)ic))
                                {
                                    var lowSurrogate = tr.Read();
                                    outputtw.Write((char)lowSurrogate);
                                }
                            }

                            // only add elipsis if current length greater than original length
                            if (currentTextLength > length)
                            {
                                if (addElipsis)
                                {
                                    outputtw.Write(hellip);
                                }

                                lengthReached = true;
                            }
                        }
                    }
                }
            }
        }

        outputtw.Flush();
        outputms.Position = 0;
        using (TextReader outputtr = new StreamReader(outputms))
        {
            string result;

            var firstTrim = outputtr.ReadToEnd().Replace("  ", " ").Trim();

            // Check to see if there is an empty char between the hellip and the output string
            // if there is, remove it
            if (addElipsis && lengthReached && string.IsNullOrWhiteSpace(firstTrim) == false)
            {
                result = firstTrim[firstTrim.Length - hellip.Length - 1] == ' '
                    ? firstTrim.Remove(firstTrim.Length - hellip.Length - 1, 1)
                    : firstTrim;
            }
            else
            {
                result = firstTrim;
            }

            return new HtmlString(result);
        }
    }

    /// <summary>
    ///     Returns the length of the words from a HTML block.
    /// </summary>
    /// <param name="html">The HTML string to measure.</param>
    /// <param name="words">The number of words to measure.</param>
    /// <returns>The length of the specified number of words in the HTML string.</returns>
    public int WordsToLength(string html, int words)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        int wordCount = 0,
            length = 0,
            maxWords = words;

        html = StripHtmlTags(html, null).ToString();

        while (length < html.Length)
        {
            // Check to see if the current wordCount reached the maxWords allowed
            if (wordCount.Equals(maxWords))
            {
                break;
            }

            // Check if current char is part of a word
            while (length < html.Length && char.IsWhiteSpace(html[length]) == false)
            {
                length++;
            }

            wordCount++;

            // Skip whitespace until the next word
            while (length < html.Length && char.IsWhiteSpace(html[length]) && wordCount.Equals(maxWords) == false)
            {
                length++;
            }
        }

        return length;
    }
}
