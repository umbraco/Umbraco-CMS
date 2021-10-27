using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web
{
    /// <summary>
    /// Provides utility methods for UmbracoHelper for working with strings and HTML in views.
    /// </summary>
    public sealed class HtmlStringUtilities
    {
        /// <summary>
        /// Replaces text line breaks with HTML line breaks.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        /// The text with text line breaks replaced with HTML line breaks (<c>&lt;br /&gt;</c>).
        /// </returns>
        [Obsolete("This method doesn't HTML encode the text. Use ReplaceLineBreaks instead.")]
        public HtmlString ReplaceLineBreaksForHtml(string text)
        {
            return new HtmlString(text.Replace("\r\n", @"<br />").Replace("\n", @"<br />").Replace("\r", @"<br />"));
        }

        /// <summary>
        /// HTML encodes the text and replaces text line breaks with HTML line breaks.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        /// The HTML encoded text with text line breaks replaced with HTML line breaks (<c>&lt;br /&gt;</c>).
        /// </returns>
        public IHtmlString ReplaceLineBreaks(string text)
        {
            var value = HttpUtility.HtmlEncode(text)?
                .Replace("\r\n", "<br />")
                .Replace("\r", "<br />")
                .Replace("\n", "<br />");

            return new HtmlString(value);
        }

        public HtmlString StripHtmlTags(string html, params string[] tags)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml("<p>" + html + "</p>");

            var targets = new List<HtmlNode>();

            var nodes = doc.DocumentNode.FirstChild.SelectNodes(".//*");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    //is element
                    if (node.NodeType != HtmlNodeType.Element) continue;
                    var filterAllTags = (tags == null || !tags.Any());
                    if (filterAllTags || tags.Any(tag => string.Equals(tag, node.Name, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        targets.Add(node);
                    }
                }
                foreach (var target in targets)
                {
                    HtmlNode content = doc.CreateTextNode(target.InnerText);
                    target.ParentNode.ReplaceChild(content, target);
                }
            }
            else
            {
                return new HtmlString(html);
            }
            return new HtmlString(doc.DocumentNode.FirstChild.InnerHtml.Replace("  ", " "));
        }

        internal string Join(string separator, params object[] args)
        {
            var results = args
                .Where(x => x != null)
                .Select(x => x.ToString())
                .Where(x => string.IsNullOrWhiteSpace(x) == false);
            return string.Join(separator, results);
        }

        internal string Concatenate(params object[] args)
        {
            var sb = new StringBuilder();
            foreach (var arg in args
                .Where(x => x != null)
                .Select(x => x.ToString())
                .Where(x => string.IsNullOrWhiteSpace(x) == false))
            {
                sb.Append(arg);
            }
            return sb.ToString();
        }

        internal string Coalesce(params object[] args)
        {
            var arg = args
                .Where(x => x != null)
                .Select(x => x.ToString())
                .FirstOrDefault(x => string.IsNullOrWhiteSpace(x) == false);

            return arg ?? string.Empty;
        }

        public IHtmlString Truncate(string html, int length, bool addElipsis, bool treatTagsAsContent)
        {
            const string hellip = "&hellip;";

            using (var outputms = new MemoryStream())
            {
                bool lengthReached = false;

                using (var outputtw = new StreamWriter(outputms))
                {
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

                                int ic = 0,
                                    //currentLength = 0,
                                    currentTextLength = 0;

                                string currentTag = string.Empty,
                                    tagContents = string.Empty;

                                while ((ic = tr.Read()) != -1)
                                {
                                    bool write = true;

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
                                            if (tr.Peek() == (int)'/')
                                            {
                                                isTagClose = true;
                                            }
                                            break;

                                        case '>':
                                            isInsideElement = false;

                                            if (isTagClose && tagStack.Count > 0)
                                            {
                                                string thisTag = tagStack.Pop();
                                                outputtw.Write("</" + thisTag + ">");
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
                                                    outputtw.Write("<" + currentTag);
                                                    if (treatTagsAsContent)
                                                    {
                                                        currentTextLength++;
                                                    }
                                                    if (!string.IsNullOrEmpty(tagContents))
                                                    {
                                                        if (tagContents.EndsWith("/"))
                                                        {
                                                            // No end tag e.g. <br />.
                                                            tagStack.Pop();
                                                        }

                                                        outputtw.Write(tagContents);
                                                        write = true;
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
                                                if (ic == (int)' ')
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
                                            //currentLength++;
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
                        string result = string.Empty;

                        string firstTrim = outputtr.ReadToEnd().Replace("  ", " ").Trim();

                        // Check to see if there is an empty char between the hellip and the output string
                        // if there is, remove it
                        if (addElipsis && lengthReached && string.IsNullOrWhiteSpace(firstTrim) == false)
                        {
                            result = firstTrim[firstTrim.Length - hellip.Length - 1] == ' ' ? firstTrim.Remove(firstTrim.Length - hellip.Length - 1, 1) : firstTrim;
                        }
                        else
                        {
                            result = firstTrim;
                        }

                        return new HtmlString(result);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the length of the words from a HTML block
        /// </summary>
        /// <param name="html">HTML text</param>
        /// <param name="words">Amount of words you would like to measure</param>
        /// <param name="tagsAsContent"></param>
        /// <returns></returns>
        public int WordsToLength(string html, int words)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            int wordCount = 0,
                length = 0,
                maxWords = words;

            html = StripHtmlTags(html, null).ToString();

            while (length < html.Length)
            {
                // Check to see if the current wordCount reached the maxWords allowed
                if (wordCount.Equals(maxWords)) break;
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
}
