// DEBUG

// We make sure that diagnostics code will not be compiled nor called into Release configuration.
// In Debug configuration, diagnostics code can be enabled by defining DEBUGNAVIGATOR below,
// but by default nothing is written, unless some lines are un-commented in Debug(...) below.
//
// Beware! Diagnostics are extremely verbose and can overflow logging pretty easily.
#if DEBUG
// define to enable diagnostics code
#undef DEBUGNAVIGATOR
#endif

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

namespace Umbraco.Cms.Core.Xml.XPath;

/// <summary>
///     Provides a cursor model for navigating Umbraco data as if it were XML.
/// </summary>
public class NavigableNavigator : XPathNavigator
{
    // "The XmlNameTable stores atomized strings of any local name, namespace URI,
    // and prefix used by the XPathNavigator. This means that when the same Name is
    // returned multiple times (like "book"), the same String object is returned for
    // that Name. This makes it possible to write efficient code that does object
    // comparisons on these strings, instead of expensive string comparisons."
    //
    // "When an element or attribute name occurs multiple times in an XML document,
    // it is stored only once in the NameTable. The names are stored as common
    // language runtime (CLR) object types. This enables you to do object comparisons
    // on these strings rather than a more expensive string comparison. These
    // string objects are referred to as atomized strings."
    //
    // But... "Any instance members are not guaranteed to be thread safe."
    //
    // see http://msdn.microsoft.com/en-us/library/aa735772%28v=vs.71%29.aspx
    // see http://www.hanselman.com/blog/XmlAndTheNametable.aspx
    // see http://blogs.msdn.com/b/mfussell/archive/2004/04/30/123673.aspx
    //
    // "Additionally, all LocalName, NameSpaceUri and Prefix strings must be added to
    // a NameTable, given by the NameTable property. When the LocalName, NamespaceURI,
    // and Prefix properties are returned, the string returned should come from the
    // NameTable. Comparisons between names are done by object comparisons rather
    // than by string comparisons, which are significantly slower.""
    //
    // So what shall we do? Well, here we have no namespace, no prefix, and all
    // local names come from cached instances of INavigableContentType or
    // INavigableFieldType and are already unique. So... create a one nametable
    // because we need one, and share it amongst all clones.
    private readonly XmlNameTable _nameTable;
    private readonly INavigableSource _source;
    private readonly int _lastAttributeIndex; // last index of attributes in the fields collection
    private readonly int _maxDepth;

    #region Constructor

    ///// <summary>
    ///// Initializes a new instance of the <see cref="NavigableNavigator"/> class with a content source.
    ///// </summary>
    ///// <param name="source">The content source.</param>
    ///// <param name="maxDepth">The maximum depth.</param>
    // private NavigableNavigator(INavigableSource source, int maxDepth)
    // {
    //    _source = source;
    //    _lastAttributeIndex = source.LastAttributeIndex;
    //    _maxDepth = maxDepth;
    // }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NavigableNavigator" /> class with a content source,
    ///     and an optional root content.
    /// </summary>
    /// <param name="source">The content source.</param>
    /// <param name="rootId">The root content identifier.</param>
    /// <param name="maxDepth">The maximum depth.</param>
    /// <remarks>When no root content is supplied then the root of the source is used.</remarks>
    public NavigableNavigator(INavigableSource source, int rootId = 0, int maxDepth = int.MaxValue)

        // : this(source, maxDepth)
    {
        _source = source;
        _lastAttributeIndex = source.LastAttributeIndex;
        _maxDepth = maxDepth;

        _nameTable = new NameTable();
        _lastAttributeIndex = source.LastAttributeIndex;
        INavigableContent? content = rootId <= 0 ? source.Root : source.Get(rootId);
        if (content == null)
        {
            throw new ArgumentException("Not the identifier of a content within the source.", nameof(rootId));
        }

        InternalState = new State(content, null, null, 0, StatePosition.Root);

        _contents = new ConcurrentDictionary<int, INavigableContent?>();
    }

    ///// <summary>
    ///// Initializes a new instance of the <see cref="NavigableNavigator"/> class with a content source, a name table and a state.
    ///// </summary>
    ///// <param name="source">The content source.</param>
    ///// <param name="nameTable">The name table.</param>
    ///// <param name="state">The state.</param>
    ///// <param name="maxDepth">The maximum depth.</param>
    ///// <remarks>Privately used for cloning a navigator.</remarks>
    // private NavigableNavigator(INavigableSource source, XmlNameTable nameTable, State state, int maxDepth)
    //    : this(source, rootId: 0, maxDepth: maxDepth)
    // {
    //    _nameTable = nameTable;
    //    _state = state;
    // }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NavigableNavigator" /> class as a clone.
    /// </summary>
    /// <param name="orig">The cloned navigator.</param>
    /// <param name="state">The clone state.</param>
    /// <param name="maxDepth">The clone maximum depth.</param>
    /// <remarks>Privately used for cloning a navigator.</remarks>
    private NavigableNavigator(NavigableNavigator orig, State? state = null, int maxDepth = -1)
        : this(orig._source, 0, orig._maxDepth)
    {
        _nameTable = orig._nameTable;

        InternalState = state ?? orig.InternalState.Clone();
        if (state != null && maxDepth < 0)
        {
            throw new ArgumentException("Both state and maxDepth are required.");
        }

        _maxDepth = maxDepth < 0 ? orig._maxDepth : maxDepth;

        _contents = orig._contents;
    }

    #endregion

    #region Diagnostics

#if DEBUGNAVIGATOR
        private const string Tabs = "                    ";
        private int _tabs;
        private readonly int _uid = GetUid();
        private static int _uidg;
        private readonly static object Uidl = new object();
        private static int GetUid()
        {
            lock (Uidl)
            {
                return _uidg++;
            }
        }
#endif

    // About conditional methods: marking a method with the [Conditional] attribute ensures
    // that no calls to the method will be generated by the compiler. However, the method
    // does exist. Wrapping the method body with #if/endif ensures that no IL is generated
    // and so it's only an empty method.
    [Conditional("DEBUGNAVIGATOR")]
    private void DebugEnter(string name)
    {
#if DEBUGNAVIGATOR
            Debug("");
            DebugState(":");
            Debug(name);
            _tabs = Math.Min(Tabs.Length, _tabs + 2);
#endif
    }

    [Conditional("DEBUGNAVIGATOR")]
    private void DebugCreate(NavigableNavigator nav)
    {
#if DEBUGNAVIGATOR
            Debug("Create: [NavigableNavigator::{0}]", nav._uid);
#endif
    }

    [Conditional("DEBUGNAVIGATOR")]
    private void DebugReturn()
    {
#if DEBUGNAVIGATOR
// ReSharper disable IntroduceOptionalParameters.Local
            DebugReturn("(void)");
// ReSharper restore IntroduceOptionalParameters.Local
#endif
    }

    [Conditional("DEBUGNAVIGATOR")]
    private void DebugReturn(bool value)
    {
#if DEBUGNAVIGATOR
            DebugReturn(value ? "true" : "false");
#endif
    }

    [Conditional("DEBUGNAVIGATOR")]
    private void DebugReturn(string format, params object[] args)
    {
#if DEBUGNAVIGATOR
            Debug("=> " + format, args);
            if (_tabs > 0) _tabs -= 2;
#endif
    }

    [Conditional("DEBUGNAVIGATOR")]
    private void DebugState(string s = " =>")
    {
#if DEBUGNAVIGATOR
            string position;

            switch (_state.Position)
            {
                case StatePosition.Attribute:
                    position = string.Format("At attribute '{0}/@{1}'.",
                        _state.Content.Type.Name,
                        _state.FieldIndex < 0 ? "id" : _state.CurrentFieldType.Name);
                    break;
                case StatePosition.Element:
                    position = string.Format("At element '{0}' (depth={1}).",
                        _state.Content.Type.Name, _state.Depth);
                    break;
                case StatePosition.PropertyElement:
                    position = string.Format("At property '{0}/{1}'.",
                        _state.Content.Type.Name, _state.Content.Type.FieldTypes[this._state.FieldIndex].Name);
                    break;
                case StatePosition.PropertyText:
                    position = string.Format("At property '{0}/{1}' text.",
                        _state.Content.Type.Name, _state.CurrentFieldType.Name);
                    break;
                case StatePosition.PropertyXml:
                    position = string.Format("In property '{0}/{1}' xml fragment.",
                        _state.Content.Type.Name, _state.CurrentFieldType.Name);
                    break;
                case StatePosition.Root:
                    position = string.Format("At root (depth={0}).",
                        _state.Depth);
                    break;
                default:
                    throw new InvalidOperationException("Invalid position.");
            }

            Debug("State{0} {1}", s, position);
#endif
    }

#if DEBUGNAVIGATOR
        void Debug(string format, params object[] args)
        {
            // remove comments to write

            //format = "[" + _uid.ToString("00000") + "] " + Tabs.Substring(0, _tabs) + format;
            //var msg = string.Format(format, args);
        }
#endif

    #endregion

    #region Source management

    private readonly ConcurrentDictionary<int, INavigableContent?> _contents;

    private INavigableContent? SourceGet(int id) =>

        // original version, would keep creating INavigableContent objects
        // return _source.Get(id);
        // improved version, uses a cache, shared with clones
        _contents.GetOrAdd(id, x => _source.Get(x));

    #endregion

    /// <summary>
    ///     Gets the underlying content object.
    /// </summary>
    public override object? UnderlyingObject => InternalState.Content;

    /// <summary>
    ///     Creates a new XPathNavigator positioned at the same node as this XPathNavigator.
    /// </summary>
    /// <returns>A new XPathNavigator positioned at the same node as this XPathNavigator.</returns>
    public override XPathNavigator Clone()
    {
        DebugEnter("Clone");
        var nav = new NavigableNavigator(this);
        DebugCreate(nav);
        DebugReturn("[XPathNavigator]");
        return nav;
    }

    /// <summary>
    ///     Creates a new XPathNavigator using the same source but positioned at a new root.
    /// </summary>
    /// <returns>A new XPathNavigator using the same source and positioned at a new root.</returns>
    /// <remarks>The new root can be above this navigator's root.</remarks>
    public XPathNavigator CloneWithNewRoot(string id, int maxDepth = int.MaxValue)
    {
        int i;
        if (int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out i) == false)
        {
            throw new ArgumentException("Not a valid identifier.", nameof(id));
        }

        return CloneWithNewRoot(id);
    }

    /// <summary>
    ///     Creates a new XPathNavigator using the same source but positioned at a new root.
    /// </summary>
    /// <returns>A new XPathNavigator using the same source and positioned at a new root.</returns>
    /// <remarks>The new root can be above this navigator's root.</remarks>
    public XPathNavigator? CloneWithNewRoot(int id, int maxDepth = int.MaxValue)
    {
        DebugEnter("CloneWithNewRoot");

        State? state = null;

        if (id <= 0)
        {
            state = new State(_source.Root, null, null, 0, StatePosition.Root);
        }
        else
        {
            INavigableContent? content = SourceGet(id);
            if (content != null)
            {
                state = new State(content, null, null, 0, StatePosition.Root);
            }
        }

        NavigableNavigator? clone = null;

        if (state != null)
        {
            clone = new NavigableNavigator(this, state, maxDepth);
            DebugCreate(clone);
            DebugReturn("[XPathNavigator]");
        }
        else
        {
            DebugReturn("[null]");
        }

        return clone;
    }

    /// <summary>
    ///     Gets a value indicating whether the current node is an empty element without an end element tag.
    /// </summary>
    public override bool IsEmptyElement
    {
        get
        {
            DebugEnter("IsEmptyElement");
            bool isEmpty;

            switch (InternalState.Position)
            {
                case StatePosition.Element:
                    // must go through source because of preview/published ie there may be
                    // ids but corresponding to preview elements that we don't see here
                    var hasContentChild = InternalState.GetContentChildIds(_maxDepth).Any(x => SourceGet(x) != null);
                    isEmpty = hasContentChild == false // no content child
                              && InternalState.FieldsCount - 1 == _lastAttributeIndex; // no property element child
                    break;
                case StatePosition.PropertyElement:
                    // value should be
                    // - an XPathNavigator over a non-empty XML fragment
                    // - a non-Xml-whitespace string
                    // - null
                    isEmpty = InternalState.Content?.Value(InternalState.FieldIndex) == null;
                    break;
                case StatePosition.PropertyXml:
                    isEmpty = InternalState.XmlFragmentNavigator?.IsEmptyElement ?? true;
                    break;
                case StatePosition.Attribute:
                case StatePosition.PropertyText:
                case StatePosition.Root:
                    throw new InvalidOperationException("Not an element.");
                default:
                    throw new InvalidOperationException("Invalid position.");
            }

            DebugReturn(isEmpty);
            return isEmpty;
        }
    }

    /// <summary>
    ///     Determines whether the current XPathNavigator is at the same position as the specified XPathNavigator.
    /// </summary>
    /// <param name="nav">The XPathNavigator to compare to this XPathNavigator.</param>
    /// <returns>true if the two XPathNavigator objects have the same position; otherwise, false.</returns>
    public override bool IsSamePosition(XPathNavigator nav)
    {
        DebugEnter("IsSamePosition");
        bool isSame;

        switch (InternalState.Position)
        {
            case StatePosition.PropertyXml:
                isSame = InternalState.XmlFragmentNavigator?.IsSamePosition(nav) ?? false;
                break;
            case StatePosition.Attribute:
            case StatePosition.Element:
            case StatePosition.PropertyElement:
            case StatePosition.PropertyText:
            case StatePosition.Root:
                var other = nav as NavigableNavigator;
                isSame = other != null && other._source == _source && InternalState.IsSamePosition(other.InternalState);
                break;
            default:
                throw new InvalidOperationException("Invalid position.");
        }

        DebugReturn(isSame);
        return isSame;
    }

    /// <summary>
    ///     Gets the qualified name of the current node.
    /// </summary>
    public override string Name
    {
        get
        {
            DebugEnter("Name");
            string name;

            switch (InternalState.Position)
            {
                case StatePosition.PropertyXml:
                    name = InternalState.XmlFragmentNavigator?.Name ?? string.Empty;
                    break;
                case StatePosition.Attribute:
                case StatePosition.PropertyElement:
                    name = InternalState.FieldIndex == -1 ? "id" : InternalState.CurrentFieldType?.Name ?? string.Empty;
                    break;
                case StatePosition.Element:
                    name = InternalState.Content?.Type.Name ?? string.Empty;
                    break;
                case StatePosition.PropertyText:
                    name = string.Empty;
                    break;
                case StatePosition.Root:
                    name = string.Empty;
                    break;
                default:
                    throw new InvalidOperationException("Invalid position.");
            }

            DebugReturn("\"{0}\"", name);
            return name;
        }
    }

    /// <summary>
    ///     Gets the Name of the current node without any namespace prefix.
    /// </summary>
    public override string LocalName
    {
        get
        {
            DebugEnter("LocalName");
            var name = Name;
            DebugReturn("\"{0}\"", name);
            return name;
        }
    }

    /// <summary>
    ///     Moves the XPathNavigator to the same position as the specified XPathNavigator.
    /// </summary>
    /// <param name="nav">The XPathNavigator positioned on the node that you want to move to. </param>
    /// <returns>
    ///     Returns true if the XPathNavigator is successful moving to the same position as the specified XPathNavigator;
    ///     otherwise, false. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveTo(XPathNavigator nav)
    {
        DebugEnter("MoveTo");

        var other = nav as NavigableNavigator;
        var succ = false;

        if (other != null && other._source == _source)
        {
            InternalState = other.InternalState.Clone();
            DebugState();
            succ = true;
        }

        DebugReturn(succ);
        return succ;
    }

    /// <summary>
    ///     Moves the XPathNavigator to the first attribute of the current node.
    /// </summary>
    /// <returns>
    ///     Returns true if the XPathNavigator is successful moving to the first attribute of the current node;
    ///     otherwise, false. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveToFirstAttribute()
    {
        DebugEnter("MoveToFirstAttribute");
        bool succ;

        switch (InternalState.Position)
        {
            case StatePosition.PropertyXml:
                succ = InternalState.XmlFragmentNavigator?.MoveToFirstAttribute() ?? false;
                break;
            case StatePosition.Element:
                InternalState.FieldIndex = -1;
                InternalState.Position = StatePosition.Attribute;
                DebugState();
                succ = true;
                break;
            case StatePosition.Attribute:
            case StatePosition.PropertyElement:
            case StatePosition.PropertyText:
            case StatePosition.Root:
                succ = false;
                break;
            default:
                throw new InvalidOperationException("Invalid position.");
        }

        DebugReturn(succ);
        return succ;
    }

    /// <summary>
    ///     Moves the XPathNavigator to the first child node of the current node.
    /// </summary>
    /// <returns>
    ///     Returns true if the XPathNavigator is successful moving to the first child node of the current node;
    ///     otherwise, false. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveToFirstChild()
    {
        DebugEnter("MoveToFirstChild");
        bool succ;

        switch (InternalState.Position)
        {
            case StatePosition.PropertyXml:
                succ = InternalState.XmlFragmentNavigator?.MoveToFirstChild() ?? false;
                break;
            case StatePosition.Attribute:
            case StatePosition.PropertyText:
                succ = false;
                break;
            case StatePosition.Element:
                var firstPropertyIndex = _lastAttributeIndex + 1;
                if (InternalState.FieldsCount > firstPropertyIndex)
                {
                    InternalState.Position = StatePosition.PropertyElement;
                    InternalState.FieldIndex = firstPropertyIndex;
                    DebugState();
                    succ = true;
                }
                else
                {
                    succ = MoveToFirstChildElement();
                }

                break;
            case StatePosition.PropertyElement:
                succ = MoveToFirstChildProperty();
                break;
            case StatePosition.Root:
                InternalState.Position = StatePosition.Element;
                DebugState();
                succ = true;
                break;
            default:
                throw new InvalidOperationException("Invalid position.");
        }

        DebugReturn(succ);
        return succ;
    }

    private bool MoveToFirstChildElement()
    {
        IList<int> children = InternalState.GetContentChildIds(_maxDepth);

        if (children.Count > 0)
        {
            // children may contain IDs that does not correspond to some content in source
            // because children contains all child IDs including unpublished children - and
            // then if we're not previewing, the source will return null.
            INavigableContent? child = children.Select(id => SourceGet(id)).FirstOrDefault(c => c != null);
            if (child != null)
            {
                InternalState.Position = StatePosition.Element;
                InternalState.FieldIndex = -1;
                InternalState = new State(child, InternalState, children, 0, StatePosition.Element);
                DebugState();
                return true;
            }
        }

        return false;
    }

    private bool MoveToFirstChildProperty()
    {
        var valueForXPath = InternalState.Content?.Value(InternalState.FieldIndex);

        // value should be
        // - an XPathNavigator over a non-empty XML fragment
        // - a non-Xml-whitespace string
        // - null
        var nav = valueForXPath as XPathNavigator;
        if (nav != null)
        {
            nav = nav.Clone(); // never use the one we got
            nav.MoveToFirstChild();
            InternalState.XmlFragmentNavigator = nav;
            InternalState.Position = StatePosition.PropertyXml;
            DebugState();
            return true;
        }

        if (valueForXPath == null)
        {
            return false;
        }

        if (valueForXPath is string)
        {
            InternalState.Position = StatePosition.PropertyText;
            DebugState();
            return true;
        }

        throw new InvalidOperationException("XPathValue must be an XPathNavigator or a string.");
    }

    /// <summary>
    ///     Moves the XPathNavigator to the first namespace node that matches the XPathNamespaceScope specified.
    /// </summary>
    /// <param name="namespaceScope">An XPathNamespaceScope value describing the namespace scope. </param>
    /// <returns>
    ///     Returns true if the XPathNavigator is successful moving to the first namespace node;
    ///     otherwise, false. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
    {
        DebugEnter("MoveToFirstNamespace");
        DebugReturn(false);
        return false;
    }

    /// <summary>
    ///     Moves the XPathNavigator to the next namespace node matching the XPathNamespaceScope specified.
    /// </summary>
    /// <param name="namespaceScope">An XPathNamespaceScope value describing the namespace scope. </param>
    /// <returns>
    ///     Returns true if the XPathNavigator is successful moving to the next namespace node;
    ///     otherwise, false. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
    {
        DebugEnter("MoveToNextNamespace");
        DebugReturn(false);
        return false;
    }

    /// <summary>
    ///     Moves to the node that has an attribute of type ID whose value matches the specified String.
    /// </summary>
    /// <param name="id">A String representing the ID value of the node to which you want to move.</param>
    /// <returns>
    ///     true if the XPathNavigator is successful moving; otherwise, false.
    ///     If false, the position of the navigator is unchanged.
    /// </returns>
    public override bool MoveToId(string id)
    {
        DebugEnter("MoveToId");
        var succ = false;

        // don't look into fragments, just look for element identifiers
        // not sure we actually need to implement it... think of it as
        // as exercise of style, always better than throwing NotImplemented.

        // navigator may be rooted below source root
        // find the navigator root id
        State state = InternalState;

        // root state has no parent
        while (state.Parent != null)
        {
            state = state.Parent;
        }

        var navRootId = state.Content?.Id;

        int contentId;
        if (int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out contentId))
        {
            if (contentId == navRootId)
            {
                InternalState = new State(state.Content, null, null, 0, StatePosition.Element);
                succ = true;
            }
            else
            {
                INavigableContent? content = SourceGet(contentId);
                if (content != null)
                {
                    // walk up to the navigator's root - or the source's root
                    var s = new Stack<INavigableContent>();
                    while (content != null && content.ParentId != navRootId)
                    {
                        s.Push(content);
                        content = SourceGet(content.ParentId);
                    }

                    if (content != null && s.Count < _maxDepth)
                    {
                        InternalState = new State(state.Content, null, null, 0, StatePosition.Element);
                        while (content != null)
                        {
                            InternalState = new State(content, InternalState, InternalState.Content?.ChildIds, InternalState.Content?.ChildIds?.IndexOf(content.Id) ?? -1, StatePosition.Element);
                            content = s.Count == 0 ? null : s.Pop();
                        }

                        DebugState();
                        succ = true;
                    }
                }
            }
        }

        DebugReturn(succ);
        return succ;
    }

    /// <summary>
    ///     Moves the XPathNavigator to the next sibling node of the current node.
    /// </summary>
    /// <returns>
    ///     true if the XPathNavigator is successful moving to the next sibling node;
    ///     otherwise, false if there are no more siblings or if the XPathNavigator is currently
    ///     positioned on an attribute node. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveToNext()
    {
        DebugEnter("MoveToNext");
        bool succ;

        switch (InternalState.Position)
        {
            case StatePosition.PropertyXml:
                succ = InternalState.XmlFragmentNavigator?.MoveToNext() ?? false;
                break;
            case StatePosition.Element:
                succ = false;
                while (InternalState.Siblings != null && InternalState.SiblingIndex < InternalState.Siblings.Count - 1)
                {
                    // Siblings may contain IDs that does not correspond to some content in source
                    // because children contains all child IDs including unpublished children - and
                    // then if we're not previewing, the source will return null.
                    INavigableContent? node = SourceGet(InternalState.Siblings[++InternalState.SiblingIndex]);
                    if (node == null)
                    {
                        continue;
                    }

                    InternalState.Content = node;
                    DebugState();
                    succ = true;
                    break;
                }

                break;
            case StatePosition.PropertyElement:
                if (InternalState.FieldIndex == InternalState.FieldsCount - 1)
                {
                    // after property elements may come some children elements
                    // if successful, will push a new state
                    succ = MoveToFirstChildElement();
                }
                else
                {
                    ++InternalState.FieldIndex;
                    DebugState();
                    succ = true;
                }

                break;
            case StatePosition.PropertyText:
            case StatePosition.Attribute:
            case StatePosition.Root:
                succ = false;
                break;
            default:
                throw new InvalidOperationException("Invalid position.");
        }

        DebugReturn(succ);
        return succ;
    }

    /// <summary>
    ///     Moves the XPathNavigator to the previous sibling node of the current node.
    /// </summary>
    /// <returns>
    ///     Returns true if the XPathNavigator is successful moving to the previous sibling node;
    ///     otherwise, false if there is no previous sibling node or if the XPathNavigator is currently
    ///     positioned on an attribute node. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveToPrevious()
    {
        DebugEnter("MoveToPrevious");
        bool succ;

        switch (InternalState.Position)
        {
            case StatePosition.PropertyXml:
                succ = InternalState.XmlFragmentNavigator?.MoveToPrevious() ?? false;
                break;
            case StatePosition.Element:
                succ = false;
                while (InternalState.Siblings != null && InternalState.SiblingIndex > 0)
                {
                    // children may contain IDs that does not correspond to some content in source
                    // because children contains all child IDs including unpublished children - and
                    // then if we're not previewing, the source will return null.
                    INavigableContent? content = SourceGet(InternalState.Siblings[--InternalState.SiblingIndex]);
                    if (content == null)
                    {
                        continue;
                    }

                    InternalState.Content = content;
                    DebugState();
                    succ = true;
                    break;
                }

                if (succ == false && InternalState.SiblingIndex == 0 &&
                    InternalState.FieldsCount - 1 > _lastAttributeIndex)
                {
                    // before children elements may come some property elements
                    // pops the state
                    if (MoveToParentElement())
                    {
                        InternalState.FieldIndex = InternalState.FieldsCount - 1;
                        DebugState();
                        succ = true;
                    }
                }

                break;
            case StatePosition.PropertyElement:
                succ = false;
                if (InternalState.FieldIndex > _lastAttributeIndex)
                {
                    --InternalState.FieldIndex;
                    DebugState();
                    succ = true;
                }

                break;
            case StatePosition.Attribute:
            case StatePosition.PropertyText:
            case StatePosition.Root:
                succ = false;
                break;
            default:
                throw new InvalidOperationException("Invalid position.");
        }

        DebugReturn(succ);
        return succ;
    }

    /// <summary>
    ///     Moves the XPathNavigator to the next attribute.
    /// </summary>
    /// <returns>
    ///     Returns true if the XPathNavigator is successful moving to the next attribute;
    ///     false if there are no more attributes. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveToNextAttribute()
    {
        DebugEnter("MoveToNextAttribute");
        bool succ;

        switch (InternalState.Position)
        {
            case StatePosition.PropertyXml:
                succ = InternalState.XmlFragmentNavigator?.MoveToNextAttribute() ?? false;
                break;
            case StatePosition.Attribute:
                if (InternalState.FieldIndex == _lastAttributeIndex)
                {
                    succ = false;
                }
                else
                {
                    ++InternalState.FieldIndex;
                    DebugState();
                    succ = true;
                }

                break;
            case StatePosition.Element:
            case StatePosition.PropertyElement:
            case StatePosition.PropertyText:
            case StatePosition.Root:
                succ = false;
                break;
            default:
                throw new InvalidOperationException("Invalid position.");
        }

        DebugReturn(succ);
        return succ;
    }

    /// <summary>
    ///     Moves the XPathNavigator to the parent node of the current node.
    /// </summary>
    /// <returns>
    ///     Returns true if the XPathNavigator is successful moving to the parent node of the current node;
    ///     otherwise, false. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveToParent()
    {
        DebugEnter("MoveToParent");
        bool succ;

        switch (InternalState.Position)
        {
            case StatePosition.Attribute:
            case StatePosition.PropertyElement:
                InternalState.Position = StatePosition.Element;
                InternalState.FieldIndex = -1;
                DebugState();
                succ = true;
                break;
            case StatePosition.Element:
                succ = MoveToParentElement();
                if (succ == false)
                {
                    InternalState.Position = StatePosition.Root;
                    succ = true;
                }

                break;
            case StatePosition.PropertyText:
                InternalState.Position = StatePosition.PropertyElement;
                DebugState();
                succ = true;
                break;
            case StatePosition.PropertyXml:
                if (InternalState.XmlFragmentNavigator?.MoveToParent() == false)
                {
                    throw new InvalidOperationException("Could not move to parent in fragment.");
                }

                if (InternalState.XmlFragmentNavigator?.NodeType == XPathNodeType.Root)
                {
                    InternalState.XmlFragmentNavigator = null;
                    InternalState.Position = StatePosition.PropertyElement;
                    DebugState();
                }

                succ = true;
                break;
            case StatePosition.Root:
                succ = false;
                break;
            default:
                throw new InvalidOperationException("Invalid position.");
        }

        DebugReturn(succ);
        return succ;
    }

    private bool MoveToParentElement()
    {
        State? p = InternalState.Parent;
        if (p != null)
        {
            InternalState = p;
            DebugState();
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Moves the XPathNavigator to the root node that the current node belongs to.
    /// </summary>
    public override void MoveToRoot()
    {
        DebugEnter("MoveToRoot");

        while (InternalState.Parent != null)
        {
            InternalState = InternalState.Parent;
        }

        DebugState();

        DebugReturn();
    }

    /// <summary>
    ///     Gets the base URI for the current node.
    /// </summary>
    public override string BaseURI => string.Empty;

    /// <summary>
    ///     Gets the XmlNameTable of the XPathNavigator.
    /// </summary>
    public override XmlNameTable NameTable => _nameTable;

    /// <summary>
    ///     Gets the namespace URI of the current node.
    /// </summary>
    public override string NamespaceURI => string.Empty;

    /// <summary>
    ///     Gets the XPathNodeType of the current node.
    /// </summary>
    public override XPathNodeType NodeType
    {
        get
        {
            DebugEnter("NodeType");
            XPathNodeType type;

            switch (InternalState.Position)
            {
                case StatePosition.PropertyXml:
                    type = InternalState.XmlFragmentNavigator?.NodeType ?? XPathNodeType.Root;
                    break;
                case StatePosition.Attribute:
                    type = XPathNodeType.Attribute;
                    break;
                case StatePosition.Element:
                case StatePosition.PropertyElement:
                    type = XPathNodeType.Element;
                    break;
                case StatePosition.PropertyText:
                    type = XPathNodeType.Text;
                    break;
                case StatePosition.Root:
                    type = XPathNodeType.Root;
                    break;
                default:
                    throw new InvalidOperationException("Invalid position.");
            }

            DebugReturn("\'{0}\'", type);
            return type;
        }
    }

    /// <summary>
    ///     Gets the namespace prefix associated with the current node.
    /// </summary>
    public override string Prefix => string.Empty;

    /// <summary>
    ///     Gets the string value of the item.
    /// </summary>
    /// <remarks>
    ///     Does not fully behave as per the specs, as we report empty value on content elements, and we start
    ///     reporting values only on property elements. This is because, otherwise, we would dump the whole database
    ///     and it probably does not make sense at Umbraco level.
    /// </remarks>
    public override string Value
    {
        get
        {
            DebugEnter("Value");
            string value;

            switch (InternalState.Position)
            {
                case StatePosition.PropertyXml:
                    value = InternalState.XmlFragmentNavigator?.Value ?? string.Empty;
                    break;
                case StatePosition.Attribute:
                case StatePosition.PropertyText:
                case StatePosition.PropertyElement:
                    if (InternalState.FieldIndex == -1)
                    {
                        value = InternalState.Content?.Id.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                    }
                    else
                    {
                        var valueForXPath = InternalState.Content?.Value(InternalState.FieldIndex);

                        // value should be
                        // - an XPathNavigator over a non-empty XML fragment
                        // - a non-Xml-whitespace string
                        // - null
                        var nav = valueForXPath as XPathNavigator;
                        var s = valueForXPath as string;
                        if (valueForXPath == null)
                        {
                            value = string.Empty;
                        }
                        else if (nav != null)
                        {
                            nav = nav.Clone(); // never use the one we got
                            value = nav.Value;
                        }
                        else if (s != null)
                        {
                            value = s;
                        }
                        else
                        {
                            throw new InvalidOperationException("XPathValue must be an XPathNavigator or a string.");
                        }
                    }

                    break;
                case StatePosition.Element:
                case StatePosition.Root:
                    value = string.Empty;
                    break;
                default:
                    throw new InvalidOperationException("Invalid position.");
            }

            DebugReturn("\"{0}\"", value);
            return value;
        }
    }

    #region State management

    // the possible state positions
    public enum StatePosition
    {
        Root,
        Element,
        Attribute,
        PropertyElement,
        PropertyText,
        PropertyXml,
    }

    // gets the state
    // for unit tests only
    public State InternalState { get; private set; }

    // represents the XPathNavigator state
    public class State
    {
        private static readonly int[] NoChildIds = new int[0];

        // the current content
        private INavigableContent? _content;

        // initialize a new state
        private State(StatePosition position)
        {
            Position = position;
            FieldIndex = -1;
        }

        // initialize a new state
        // used for creating the very first state
        // and also when moving to a child element
        public State(INavigableContent? content, State? parent, IList<int>? siblings, int siblingIndex, StatePosition position)
            : this(position)
        {
            Content = content;
            Parent = parent;
            Depth = parent?.Depth + 1 ?? 0;
            Siblings = siblings;
            SiblingIndex = siblingIndex;
        }

        // initialize a clone state
        private State(State other, bool recurse = false)
        {
            Position = other.Position;

            _content = other._content;
            SiblingIndex = other.SiblingIndex;
            Siblings = other.Siblings;
            FieldsCount = other.FieldsCount;
            FieldIndex = other.FieldIndex;
            Depth = other.Depth;

            if (Position == StatePosition.PropertyXml)
            {
                XmlFragmentNavigator = other.XmlFragmentNavigator?.Clone();
            }

            // NielsK did
            // Parent = other.Parent;
            // but that creates corrupted stacks of states when cloning
            // because clones share the parents : have to clone the whole
            // stack of states. Avoid recursion.
            if (recurse)
            {
                return;
            }

            State clone = this;
            while (other.Parent != null)
            {
                clone.Parent = new State(other.Parent, true);
                clone = clone.Parent;
                other = other.Parent;
            }
        }

        public StatePosition Position { get; set; }

        // the parent state
        public State? Parent { get; private set; }

        // the depth
        public int Depth { get; }

        // the current content
        public INavigableContent? Content
        {
            get => _content;
            set
            {
                FieldsCount = value?.Type.FieldTypes.Length ?? 0;
                _content = value;
            }
        }

        // the index of the current content within Siblings
        public int SiblingIndex { get; set; }

        // the list of content identifiers for all children of the current content's parent
        public IList<int>? Siblings { get; }

        // the number of fields of the current content
        // properties include attributes and properties
        public int FieldsCount { get; private set; }

        // the index of the current field
        // index -1 means special attribute "id"
        public int FieldIndex { get; set; }

        // the current field type
        // beware, no check on the index
        public INavigableFieldType? CurrentFieldType => Content?.Type.FieldTypes[FieldIndex];

        // gets or sets the xml fragment navigator
        public XPathNavigator? XmlFragmentNavigator { get; set; }

        public State Clone() => new State(this);

        // the current content child ids
        public IList<int> GetContentChildIds(int maxDepth) =>
            Depth < maxDepth && _content?.ChildIds != null ? _content.ChildIds : NoChildIds;

        // gets a value indicating whether this state is at the same position as another one.
        public bool IsSamePosition(State other)
        {
            if (other.XmlFragmentNavigator is null || XmlFragmentNavigator is null)
            {
                return false;
            }

            return other.Position == Position
                   && (Position != StatePosition.PropertyXml ||
                       other.XmlFragmentNavigator.IsSamePosition(XmlFragmentNavigator))
                   && other.Content == Content
                   && other.FieldIndex == FieldIndex;
        }
    }

    #endregion
}
