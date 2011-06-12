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


        public HtmlTagWrapper Wrap(string tag, string innerText)
        {
            HtmlTagWrapper wrap = new HtmlTagWrapper(tag);
            wrap.Children.Add(new HtmlTagWrapperTextNode(innerText));
            return wrap;
        }
    }
}
