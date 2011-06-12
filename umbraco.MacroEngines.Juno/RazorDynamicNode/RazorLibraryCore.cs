using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces;
using System.Xml.Linq;

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
            return null;
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
    }
}
