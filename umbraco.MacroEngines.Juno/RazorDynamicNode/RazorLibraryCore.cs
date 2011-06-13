using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces;
using System.Xml.Linq;
using System.Xml.XPath;

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

        public DynamicNode NodeById(int Id)
        {
            return new DynamicNode(Id);
        }
        public DynamicNode NodeById(string Id)
        {
            return new DynamicNode(Id);
        }
        public DynamicNode NodeById(object Id)
        {
            return new DynamicNode(Id);
        }
        public DynamicNodeList NodeById(List<object> Ids)
        {
            List<DynamicNode> nodes = new List<DynamicNode>();
            foreach (object eachId in Ids)
                nodes.Add(new DynamicNode(eachId));
            return new DynamicNodeList(nodes);
        }
        public DynamicNodeList NodeById(List<int> Ids)
        {
            List<DynamicNode> nodes = new List<DynamicNode>();
            foreach (int eachId in Ids)
                nodes.Add(new DynamicNode(eachId));
            return new DynamicNodeList(nodes);
        }
        public DynamicNodeList NodeById(params object[] Ids)
        {
            return NodeById(Ids.ToList());
        }
        public DynamicNode MediaById(int Id)
        {
            return new DynamicNode(new DynamicBackingItem(ExamineBackedMedia.GetUmbracoMedia(Id)));
        }
        public DynamicNode MediaById(string Id)
        {
            int mediaId = 0;
            if (int.TryParse(Id, out mediaId))
            {
                return MediaById(mediaId);
            }
            throw new ArgumentException("Cannot get MediaById without an id");
        }
        public DynamicNode MediaById(object Id)
        {
            int mediaId = 0;
            if (int.TryParse(string.Format("{0}", Id), out mediaId))
            {
                return MediaById(mediaId);
            }
            return null;
        }
        public DynamicNodeList MediaById(List<object> Ids)
        {
            List<DynamicNode> nodes = new List<DynamicNode>();
            foreach (object eachId in Ids)
                nodes.Add(MediaById(eachId));
            return new DynamicNodeList(nodes);
        }
        public DynamicNodeList MediaById(List<int> Ids)
        {
            List<DynamicNode> nodes = new List<DynamicNode>();
            foreach (int eachId in Ids)
                nodes.Add(MediaById(eachId));
            return new DynamicNodeList(nodes);
        }
        public DynamicNodeList MediaById(params object[] Ids)
        {
            return MediaById(Ids.ToList());
        }

        public T As<T>() where T : class
        {
            return (this as T);
        }

        public DynamicXml ToDynamicXml(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) return null;
            var xElement = XElement.Parse(xml);
            return new umbraco.MacroEngines.DynamicXml(xElement);
        }
        public DynamicXml ToDynamicXml(XElement xElement)
        {
            return new DynamicXml(xElement);
        }
        public DynamicXml ToDynamicXml(XPathNodeIterator xpni)
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

        public string If(bool test, string valueIfTrue, string valueIfFalse)
        {
            return test ? valueIfTrue : valueIfFalse;
        }

        public HtmlTagWrapper Wrap(string tag, string innerText, params HtmlTagWrapperBase[] Children)
        {
            var item = Wrap(tag, innerText, null);
            foreach (var child in Children)
            {
                item.AddChild(child);
            }
            return item;
        }
        public HtmlTagWrapper Wrap(string tag, string innerText)
        {
            return Wrap(tag, innerText, null);
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

        public HtmlTagWrapper Wrap(bool predicate, string tag, string innerText, object anonymousAttributes)
        {
            if (predicate)
            {
                return Wrap(tag, innerText, anonymousAttributes);
            }
            return null;
        }
        public HtmlTagWrapper Wrap(bool predicate, string tag, string innerText, object anonymousAttributes, params HtmlTagWrapperBase[] Children)
        {
            if (predicate)
            {
                var item = Wrap(tag, innerText, anonymousAttributes, Children);
                foreach (var child in Children)
                {
                    item.AddChild(child);
                }
                return item;
            }
            return null;
        }
    }
}
