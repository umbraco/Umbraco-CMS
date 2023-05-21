using System.Xml;
using System.Xml.XPath;

namespace Umbraco.Cms.Core.Xml.XPath;

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

    public override bool IsEmptyElement => _navigator.IsEmptyElement;

    public override string LocalName
    {
        get
        {
            // local name without prefix
            XPathNavigator nav = _navigator.Clone();
            if (nav.MoveToParent() && nav.MoveToParent())
            {
                return _navigator.LocalName;
            }

            return _rootName;
        }
    }

    public override string Name
    {
        get
        {
            // qualified name with prefix
            XPathNavigator nav = _navigator.Clone();
            if (nav.MoveToParent() && nav.MoveToParent())
            {
                return _navigator.Name;
            }

            var name = _navigator.Name;
            var pos = name.IndexOf(':');
            return pos < 0 ? _rootName : name[..(pos + 1)] + _rootName;
        }
    }

    public override XmlNameTable NameTable => _navigator.NameTable;

    public override string NamespaceURI => _navigator.NamespaceURI;

    public override XPathNodeType NodeType => _navigator.NodeType;

    public override string Prefix => _navigator.Prefix;

    public override string Value => _navigator.Value;

    public override XPathNavigator Clone() => new RenamedRootNavigator(_navigator.Clone(), _rootName);

    public override bool IsSamePosition(XPathNavigator other) => _navigator.IsSamePosition(other);

    public override bool MoveTo(XPathNavigator other) => _navigator.MoveTo(other);

    public override bool MoveToFirstAttribute() => _navigator.MoveToFirstAttribute();

    public override bool MoveToFirstChild() => _navigator.MoveToFirstChild();

    public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope) =>
        _navigator.MoveToFirstNamespace(namespaceScope);

    public override bool MoveToId(string id) => _navigator.MoveToId(id);

    public override bool MoveToNext() => _navigator.MoveToNext();

    public override bool MoveToNextAttribute() => _navigator.MoveToNextAttribute();

    public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope) =>
        _navigator.MoveToNextNamespace(namespaceScope);

    public override bool MoveToParent() => _navigator.MoveToParent();

    public override bool MoveToPrevious() => _navigator.MoveToPrevious();
}
