using System.Xml;
using System.Xml.XPath;

namespace Umbraco.Core.Xml.XPath
{
    public class RenamedRootNavigator : XPathNavigator
    {
        private readonly XPathNavigator _navigator;
        private readonly string _rootName;

        public RenamedRootNavigator(XPathNavigator navigator, string rootName)
        {
            _navigator = navigator;
            _rootName = rootName;
        }

        public override string BaseURI => _navigator.BaseURI;

        public override XPathNavigator Clone()
        {
            return new RenamedRootNavigator(_navigator.Clone(), _rootName);
        }

        public override bool IsEmptyElement => _navigator.IsEmptyElement;

        public override bool IsSamePosition(XPathNavigator other)
        {
            return _navigator.IsSamePosition(other);
        }

        public override string LocalName
        {
            get
            {
                // local name without prefix

                var nav = _navigator.Clone();
                if (nav.MoveToParent() && nav.MoveToParent())
                    return _navigator.LocalName;
                return _rootName;
            }
        }

        public override bool MoveTo(XPathNavigator other)
        {
            return _navigator.MoveTo(other);
        }

        public override bool MoveToFirstAttribute()
        {
            return _navigator.MoveToFirstAttribute();
        }

        public override bool MoveToFirstChild()
        {
            return _navigator.MoveToFirstChild();
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            return _navigator.MoveToFirstNamespace(namespaceScope);
        }

        public override bool MoveToId(string id)
        {
            return _navigator.MoveToId(id);
        }

        public override bool MoveToNext()
        {
            return _navigator.MoveToNext();
        }

        public override bool MoveToNextAttribute()
        {
            return _navigator.MoveToNextAttribute();
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            return _navigator.MoveToNextNamespace(namespaceScope);
        }

        public override bool MoveToParent()
        {
            return _navigator.MoveToParent();
        }

        public override bool MoveToPrevious()
        {
            return _navigator.MoveToPrevious();
        }

        public override string Name
        {
            get
            {
                // qualified name with prefix

                var nav = _navigator.Clone();
                if (nav.MoveToParent() && nav.MoveToParent())
                    return _navigator.Name;
                var name = _navigator.Name;
                var pos = name.IndexOf(':');
                return pos < 0 ? _rootName : (name.Substring(0, pos + 1) + _rootName);
            }
        }

        public override XmlNameTable NameTable => _navigator.NameTable;

        public override string NamespaceURI => _navigator.NamespaceURI;

        public override XPathNodeType NodeType => _navigator.NodeType;

        public override string Prefix => _navigator.Prefix;

        public override string Value => _navigator.Value;
    }
}
