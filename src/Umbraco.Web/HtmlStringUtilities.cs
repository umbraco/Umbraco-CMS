using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;

namespace Umbraco.Web
{
    /// <summary>
    /// Utility class for working with strings and HTML in views
    /// </summary>
    /// <remarks>
    /// The UmbracoHelper uses this class for it's string methods
    /// </remarks>
    public sealed class HtmlStringUtilities
    {
        /// <summary>
        /// Replaces text line breaks with html line breaks
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The text with text line breaks replaced with html linebreaks (<br/>)</returns>
        public string ReplaceLineBreaksForHtml(string text)
        {
            return text.Replace("\n", "<br/>\n");
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
            return new HtmlString(doc.DocumentNode.FirstChild.InnerHtml);
        }

        internal string Join<TIgnore>(string seperator, params object[] args)
        {
            var results = args.Where(arg => arg != null && arg.GetType() != typeof(TIgnore)).Select(arg => string.Format("{0}", arg)).Where(sArg => !string.IsNullOrWhiteSpace(sArg)).ToList();
            return string.Join(seperator, results);
        }

        internal string Concatenate<TIgnore>(params object[] args)
        {
            var result = new StringBuilder();
            foreach (var sArg in args.Where(arg => arg != null && arg.GetType() != typeof(TIgnore)).Select(arg => string.Format("{0}", arg)).Where(sArg => !string.IsNullOrWhiteSpace(sArg)))
            {
                result.Append(sArg);
            }
            return result.ToString();
        }

        internal string Coalesce<TIgnore>(params object[] args)
        {
            foreach (var sArg in args.Where(arg => arg != null && arg.GetType() != typeof(TIgnore)).Select(arg => string.Format("{0}", arg)).Where(sArg => !string.IsNullOrWhiteSpace(sArg)))
            {
                return sArg;
            }
            return string.Empty;
        }

        public IHtmlString Truncate(string html, int length, bool addElipsis, bool treatTagsAsContent)
        {
            using (var outputms = new MemoryStream())
            {
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
                                    lengthReached = false,
                                    insideTagSpaceEncountered = false,
                                    isTagClose = false;

                                int ic = 0,
                                    currentLength = 0,
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
                                            }
                                            if (!isTagClose && currentTag.Length > 0)
                                            {
                                                if (!lengthReached)
                                                {
                                                    tagStack.Push(currentTag);
                                                    outputtw.Write("<" + currentTag);
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
                                            currentLength++;
                                        }
                                    }

                                    if (!lengthReached && currentTextLength >= length)
                                    {
                                        // Reached truncate limit.
                                        if (addElipsis)
                                        {
                                            outputtw.Write("&hellip;");
                                        }
                                        lengthReached = true;
                                    }

                                }

                            }
                        }
                    }
                    outputtw.Flush();
                    outputms.Position = 0;
                    using (TextReader outputtr = new StreamReader(outputms))
                    {
                        return new HtmlString(outputtr.ReadToEnd().Replace("  ", " ").Trim());
                    }
                }
            }
        }
    }
}