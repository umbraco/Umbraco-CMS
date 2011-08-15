using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Web;
using System.IO;

namespace umbraco.MacroEngines.Library
{
    public class RazorLibraryCore
    {
        private INode _node;
        public INode Node
        {
            get { return _node; }
        }
        public RazorLibraryCore(INode node)
        {
            this._node = node;
        }

        public dynamic NodeById(int Id)
        {
            return new DynamicNode(Id);
        }
        public dynamic NodeById(string Id)
        {
            return new DynamicNode(Id);
        }
        public dynamic NodeById(object Id)
        {
            if (Id.GetType() == typeof(DynamicNull))
            {
                return Id;
            }
            return new DynamicNode(Id);
        }
        public dynamic NodesById(List<object> Ids)
        {
            List<DynamicNode> nodes = new List<DynamicNode>();
            foreach (object eachId in Ids)
                nodes.Add(new DynamicNode(eachId));
            return new DynamicNodeList(nodes);
        }
        public dynamic NodesById(List<int> Ids)
        {
            List<DynamicNode> nodes = new List<DynamicNode>();
            foreach (int eachId in Ids)
                nodes.Add(new DynamicNode(eachId));
            return new DynamicNodeList(nodes);
        }
        public dynamic NodesById(List<int> Ids, DynamicBackingItemType ItemType)
        {
            List<DynamicNode> nodes = new List<DynamicNode>();
            foreach (int eachId in Ids)
                nodes.Add(new DynamicNode(eachId, ItemType));
            return new DynamicNodeList(nodes);
        }
        public dynamic NodesById(params object[] Ids)
        {
            return NodesById(Ids.ToList());
        }

        public dynamic MediaById(int Id)
        {
            return new DynamicMedia(new DynamicBackingItem(ExamineBackedMedia.GetUmbracoMedia(Id)));
        }
        public dynamic MediaById(string Id)
        {
            int mediaId = 0;
            if (int.TryParse(Id, out mediaId))
            {
                return MediaById(mediaId);
            }
            throw new ArgumentException("Cannot get MediaById without an id");
        }
        public dynamic MediaById(object Id)
        {
            if (Id.GetType() == typeof(DynamicNull))
            {
                return Id;
            }
            int mediaId = 0;
            if (int.TryParse(string.Format("{0}", Id), out mediaId))
            {
                return MediaById(mediaId);
            }
            return null;
        }
        public dynamic MediaById(List<object> Ids)
        {
            List<DynamicNode> nodes = new List<DynamicNode>();
            foreach (object eachId in Ids)
                nodes.Add(MediaById(eachId));
            return new DynamicNodeList(nodes);
        }
        public dynamic MediaById(List<int> Ids)
        {
            List<DynamicNode> nodes = new List<DynamicNode>();
            foreach (int eachId in Ids)
                nodes.Add(MediaById(eachId));
            return new DynamicNodeList(nodes);
        }
        public dynamic MediaById(params object[] Ids)
        {
            return MediaById(Ids.ToList());
        }

        public T As<T>() where T : class
        {
            return (this as T);
        }

        public dynamic ToDynamicXml(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) return null;
            var xElement = XElement.Parse(xml);
            return new umbraco.MacroEngines.DynamicXml(xElement);
        }
        public dynamic ToDynamicXml(XElement xElement)
        {
            return new DynamicXml(xElement);
        }
        public dynamic ToDynamicXml(XPathNodeIterator xpni)
        {
            return new DynamicXml(xpni);
        }
        public string Coalesce(params object[] args)
        {
            foreach (var arg in args)
            {
                if (arg != null && arg.GetType() != typeof(DynamicNull))
                {
                    var sArg = string.Format("{0}", arg);
                    if (!string.IsNullOrWhiteSpace(sArg))
                    {
                        return sArg;
                    }
                }
            }
            return string.Empty;
        }

        public string Concatenate(params object[] args)
        {
            StringBuilder result = new StringBuilder();
            foreach (var arg in args)
            {
                if (arg != null && arg.GetType() != typeof(DynamicNull))
                {
                    var sArg = string.Format("{0}", arg);
                    if (!string.IsNullOrWhiteSpace(sArg))
                    {
                        result.Append(sArg);
                    }
                }
            }
            return result.ToString();
        }
        public string Join(string seperator, params object[] args)
        {
            List<string> results = new List<string>();
            foreach (var arg in args)
            {
                if (arg != null && arg.GetType() != typeof(DynamicNull))
                {
                    var sArg = string.Format("{0}", arg);
                    if (!string.IsNullOrWhiteSpace(sArg))
                    {
                        results.Add(sArg);
                    }
                }
            }
            return string.Join(seperator, results);
        }

        public HtmlString If(bool test, string valueIfTrue, string valueIfFalse)
        {
            return test ? new HtmlString(valueIfTrue) : new HtmlString(valueIfFalse);
        }
        public HtmlString If(bool test, string valueIfTrue)
        {
            return test ? new HtmlString(valueIfTrue) : new HtmlString(string.Empty);
        }

        public HtmlTagWrapper Wrap(string tag, string innerText, params HtmlTagWrapperBase[] Children)
        {
            var item = Wrap(tag, innerText, (object)null);
            foreach (var child in Children)
            {
                item.AddChild(child);
            }
            return item;
        }
        public HtmlTagWrapper Wrap(string tag, string innerText)
        {
            return Wrap(tag, innerText, (object)null);
        }
        public HtmlTagWrapper Wrap(string tag, object inner, object anonymousAttributes)
        {
            string innerText = null;
            if (inner.GetType() != typeof(DynamicNull) && inner != null)
            {
                innerText = string.Format("{0}", inner);
            }
            return Wrap(tag, innerText, anonymousAttributes);
        }

        public HtmlTagWrapper Wrap(string tag, object inner, object anonymousAttributes, params HtmlTagWrapperBase[] Children)
        {
            string innerText = null;
            if (inner.GetType() != typeof(DynamicNull) && inner != null)
            {
                innerText = string.Format("{0}", inner);
            }
            var item = Wrap(tag, innerText, anonymousAttributes);
            foreach (var child in Children)
            {
                item.AddChild(child);
            }
            return item;
        }
        public HtmlTagWrapper Wrap(string tag, object inner)
        {
            string innerText = null;
            if (inner.GetType() != typeof(DynamicNull) && inner != null)
            {
                innerText = string.Format("{0}", inner);
            }
            return Wrap(tag, innerText, null);
        }
        public HtmlTagWrapper Wrap(string tag, string innerText, object anonymousAttributes)
        {
            HtmlTagWrapper wrap = new HtmlTagWrapper(tag);
            if (anonymousAttributes != null)
            {
                wrap.ReflectAttributesFromAnonymousType(anonymousAttributes);
            }
            if (!string.IsNullOrWhiteSpace(innerText))
            {
                wrap.Children.Add(new HtmlTagWrapperTextNode(innerText));
            }
            return wrap;
        }
        public HtmlTagWrapper Wrap(string tag, string innerText, object anonymousAttributes, params HtmlTagWrapperBase[] Children)
        {
            HtmlTagWrapper wrap = new HtmlTagWrapper(tag);
            if (anonymousAttributes != null)
            {
                wrap.ReflectAttributesFromAnonymousType(anonymousAttributes);
            }
            if (!string.IsNullOrWhiteSpace(innerText))
            {
                wrap.Children.Add(new HtmlTagWrapperTextNode(innerText));
            }
            foreach (var child in Children)
            {
                wrap.AddChild(child);
            }
            return wrap;
        }

        public HtmlTagWrapper Wrap(bool visible, string tag, string innerText, object anonymousAttributes)
        {
            var item = Wrap(tag, innerText, anonymousAttributes);
            item.Visible = visible;
            return item;
        }
        public HtmlTagWrapper Wrap(bool visible, string tag, string innerText, object anonymousAttributes, params HtmlTagWrapperBase[] Children)
        {
            var item = Wrap(tag, innerText, anonymousAttributes, Children);
            item.Visible = visible;
            foreach (var child in Children)
            {
                item.AddChild(child);
            }
            return item;
        }
        public IHtmlString Truncate(IHtmlString html, int length)
        {
            return Truncate(html.ToHtmlString(), length, true, false);
        }
        public IHtmlString Truncate(IHtmlString html, int length, bool addElipsis)
        {
            return Truncate(html.ToHtmlString(), length, addElipsis, false);
        }
        public IHtmlString Truncate(IHtmlString html, int length, bool addElipsis, bool treatTagsAsContent)
        {
            return Truncate(html.ToHtmlString(), length, addElipsis, treatTagsAsContent);
        }
        public IHtmlString Truncate(DynamicNull html, int length)
        {
            return new HtmlString(string.Empty);
        }
        public IHtmlString Truncate(DynamicNull html, int length, bool addElipsis)
        {
            return new HtmlString(string.Empty);
        }
        public IHtmlString Truncate(DynamicNull html, int length, bool addElipsis, bool treatTagsAsContent)
        {
            return new HtmlString(string.Empty);
        }
        public IHtmlString Truncate(string html, int length)
        {
            return Truncate(html, length, true, false);
        }
        public IHtmlString Truncate(string html, int length, bool addElipsis)
        {
            return Truncate(html, length, addElipsis, false);
        }
        public IHtmlString Truncate(string html, int length, bool addElipsis, bool treatTagsAsContent)
        {
            using (MemoryStream outputms = new MemoryStream())
            {
                using (TextWriter outputtw = new StreamWriter(outputms))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (TextWriter tw = new StreamWriter(ms))
                        {
                            tw.Write(html);
                            tw.Flush();
                            ms.Position = 0;
                            Stack<string> tagStack = new Stack<string>();
                            using (TextReader tr = new StreamReader(ms))
                            {
                                bool IsInsideElement = false;
                                bool lengthReached = false;
                                int ic = 0;
                                int currentLength = 0, currentTextLength = 0;
                                string currentTag = string.Empty;
                                string tagContents = string.Empty;
                                bool insideTagSpaceEncountered = false;
                                bool isTagClose = false;
                                while ((ic = tr.Read()) != -1)
                                {
                                    bool write = true;

                                    if (ic == (int)'<')
                                    {
                                        if (!lengthReached)
                                        {
                                            IsInsideElement = true;
                                        }
                                        insideTagSpaceEncountered = false;
                                        currentTag = string.Empty;
                                        tagContents = string.Empty;
                                        isTagClose = false;
                                        if (tr.Peek() == (int)'/')
                                        {
                                            isTagClose = true;
                                        }
                                    }
                                    else if (ic == (int)'>')
                                    {
                                        //if (IsInsideElement)
                                        //{
                                        IsInsideElement = false;
                                        //if (write)
                                        //{
                                        //  outputtw.Write('>');
                                        //}
                                        currentTextLength++;
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
                                                if (tr.Peek() != (int)' ')
                                                {
                                                    if (!string.IsNullOrEmpty(tagContents))
                                                    {
                                                        if (tagContents.EndsWith("/"))
                                                        {
                                                            //short close
                                                            tagStack.Pop();
                                                        }
                                                        outputtw.Write(tagContents);
                                                    }
                                                    outputtw.Write(">");
                                                }
                                            }
                                        }
                                        //}
                                        continue;
                                    }
                                    else
                                    {
                                        if (IsInsideElement)
                                        {
                                            if (ic == (int)' ')
                                            {
                                                if (!insideTagSpaceEncountered)
                                                {
                                                    insideTagSpaceEncountered = true;
                                                    //if (!isTagClose)
                                                    //{
                                                    // tagStack.Push(currentTag);
                                                    //}
                                                }
                                            }
                                            if (!insideTagSpaceEncountered)
                                            {
                                                currentTag += (char)ic;
                                            }
                                        }
                                    }
                                    if (IsInsideElement || insideTagSpaceEncountered)
                                    {
                                        write = false;
                                        if (insideTagSpaceEncountered)
                                        {
                                            tagContents += (char)ic;
                                        }
                                    }
                                    if (!IsInsideElement || treatTagsAsContent)
                                    {
                                        currentTextLength++;
                                    }
                                    currentLength++;
                                    if (currentTextLength <= length || (lengthReached && IsInsideElement))
                                    {
                                        if (write)
                                        {
                                            outputtw.Write((char)ic);
                                        }
                                    }
                                    if (!lengthReached && currentTextLength >= length)
                                    {
                                        //reached truncate point
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


        public HtmlString StripHtml(IHtmlString html)
        {
            return StripHtml(html.ToHtmlString(), (List<string>)null);
        }
        public HtmlString StripHtml(DynamicNull html)
        {
            return new HtmlString(string.Empty);
        }
        public HtmlString StripHtml(string html)
        {
            return StripHtmlTags(html, (List<string>)null);
        }

        public HtmlString StripHtml(IHtmlString html, List<string> tags)
        {
            return StripHtml(html.ToHtmlString(), tags);
        }
        public HtmlString StripHtml(DynamicNull html, List<string> tags)
        {
            return new HtmlString(string.Empty);
        }
        public HtmlString StripHtml(string html, List<string> tags)
        {
            return StripHtmlTags(html, tags);
        }

        public HtmlString StripHtml(IHtmlString html, params string[] tags)
        {
            return StripHtml(html.ToHtmlString(), tags.ToList());
        }
        public HtmlString StripHtml(DynamicNull html, params string[] tags)
        {
            return new HtmlString(string.Empty);
        }
        public HtmlString StripHtml(string html, params string[] tags)
        {
            return StripHtmlTags(html, tags.ToList());
        }

        //ge: this method won't deal with <script> or <style> tags as they could have nested < or >
        private HtmlString StripHtmlTags(string html, List<string> tags)
        {
            if (tags == null)
            {
                tags = new List<string>();
            }
            using (MemoryStream outputms = new MemoryStream())
            {
                using (TextWriter outputtw = new StreamWriter(outputms))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (TextWriter tw = new StreamWriter(ms))
                        {
                            tw.Write(html);
                            tw.Flush();
                            ms.Position = 0;
                            using (TextReader tr = new StreamReader(ms))
                            {
                                bool IsInsideElement = false, InsideTagSpaceEncountered = false, DeniedTag = false;
                                string currentTag = string.Empty;
                                bool isTagClose = false;
                                int ic = 0;
                                while ((ic = tr.Read()) != -1)
                                {
                                    if (ic == (int)'<')
                                    {
                                        IsInsideElement = true;
                                        InsideTagSpaceEncountered = false;
                                        currentTag = string.Empty;
                                        isTagClose = false;
                                        DeniedTag = false;
                                        if (tr.Peek() == (int)'/')
                                        {
                                            isTagClose = true;
                                            DeniedTag = true;
                                        }
                                    }
                                    if (ic == (int)'>')
                                    {
                                        IsInsideElement = false;
                                        outputtw.Write(' ');
                                        continue;
                                    }
                                    if (ic == (int)' ' && IsInsideElement)
                                    {
                                        InsideTagSpaceEncountered = true;
                                    }
                                    if (!IsInsideElement && !DeniedTag)
                                    {
                                        outputtw.Write((char)ic);
                                    }
                                    if (IsInsideElement)
                                    {
                                        currentTag += ic;
                                        if (!InsideTagSpaceEncountered)
                                        {
                                            InsideTagSpaceEncountered = true;
                                            //space on open
                                            if (tags.Any(tag => tag.Equals(currentTag, StringComparison.CurrentCultureIgnoreCase)))
                                            {
                                                DeniedTag = true;
                                                outputtw.Write("<" + currentTag + " ");
                                            }
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
                        return new HtmlString(outputtr.ReadToEnd().Replace("  ", " ").Trim());
                    }
                }
            }
        }


    }
}
